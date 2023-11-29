using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CsvHelper;
using Lab4.ViewModels;

namespace Lab4.Models;

public class Merger
{
    public ObservableCollection<Connection> Connections { get; private set; }
    public ObservableCollection<object> Values { get; private init; }
    public ObservableCollection<Record> Records { get; set; }
    private List<bool> ValuesStatuses { get; set; }

    public Merger()
    {
        Values = new ObservableCollection<object>();
        Records = new ObservableCollection<Record>();
        Connections = new();
        ValuesStatuses = new();
    }
    public Merger(string[] fileNames) : this()
    {
        Connections = OpenConnections(fileNames);
        ValuesStatuses = new List<bool>(new bool[Connections.Count]);
    }

    public async void ExternalMerge(FileInfo? file, string name, Type type, string parameter)
    {
        if (file is not null && !file.Exists)
            return;

        var connectionFactory = new ConnectionFactory();
        var connection = connectionFactory.StartConnection(file);
        if (connection is null)
            return;

        var files = InitialSetUp(connection, parameter);
        var sortedFile = await Merge(files, name, type);
        File.Move(sortedFile.FullName, 
            MoveExists($@"{sortedFile.DirectoryName}\sorted.csv"));
        MessageBox.Show("End");
    }

    private string MoveExists(string fullPath)
    {
        int count = 1;

        string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
        string extension = Path.GetExtension(fullPath);
        string path = Path.GetDirectoryName(fullPath);
        string newFullPath = fullPath;

        while(File.Exists(newFullPath)) 
        {
            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
            newFullPath = Path.Combine(path, tempFileName + extension);
        }

        return newFullPath;
    }

    private async Task<FileInfo> Merge(List<FileInfo> files, string name, Type type)
    {
        while (files.Count > 1)
        {
            var newFiles = new List<FileInfo>();
            
            for (var i = 0; i < files.Count; i += 2)
            {
                if (i + 1 < files.Count)
                {
                    var result = await MergeTwoFiles(files[i], files[i + 1], name, type);
                    if(result is not null)
                        newFiles.Add(result);
                }
                else
                    newFiles.Add(files[i]);
            }

            files = newFiles;
        }

        return files[0];
    }

    private async Task<FileInfo?> MergeTwoFiles(FileInfo file1, FileInfo file2, string name, Type type)
    {
        Connections = OpenConnections(new string[] {file1.FullName, file2.FullName});
        ValuesStatuses = new List<bool>(new bool[Connections.Count]);
        string directory = @"C:\Users\Dying\RiderProjects\Lab4\Lab4\Files";
        string fileName = $@"{directory}\{file1.Name + file2.Name}";
        await using var streamWriter = File.AppendText(fileName);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        FileInfo? fileInfo = null;
        
        while (true)
        {
            await ReadRecords();
            if (!Min(name, type, out var min))
                break;
            
            csvWriter.WriteRecords(new object[] { min });
        }
        
        foreach (var connection in Connections)
        {
            connection.Close();
            connection.CsvFile.Delete();
        }

        Connections.Clear();

        if (fileName.Length > 0)
            fileInfo = new FileInfo(fileName);

        return fileInfo;
    }

    private List<FileInfo> InitialSetUp(Connection connection, string parameter)
    {
        object? record;
        int counter = 0;
        string filesDirectory = @"C:\\Users\\Dying\\RiderProjects\\Lab4\\Lab4\\Files";
        var files = new List<FileInfo>();

        while ((record = connection.ReadRecord()) != null && counter < 200)
        {
            string path = $@"{filesDirectory}\{counter}.csv";

            if (parameter.Length == 0 || !MatchesParameter(parameter, record))
                continue;
            
            using var streamWriter = File.AppendText(path);
            using var csvStream = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvStream.WriteRecords(new object[] {record});
            files.Add(new FileInfo(path));

            counter++;
        }

        return files;
    }

    private static bool MatchesParameter(string parameter, object record)
    {
        var properties = record.GetType().GetProperties().ToList();
        bool hasProperty = false;
        foreach (var property in properties)
        {
            var value = (string) record.GetProperty(property.Name, typeof(string));
            if (value == parameter)
            {
                hasProperty = true;
            }
        }

        return hasProperty;
    }

    public async Task MultiPathMerge(string propertyName, Type propertyType)
    {
        var batch = new Batch<object>()
        {
            FullPath = @"C:\Users\Dying\RiderProjects\Lab4\Lab4\Files\Sorted.csv",
            RecordType = SingleConnectionType.CsvType
        };

        while (true)
        {
            await ReadRecords();
            if (!Min(propertyName, propertyType, out var min))
                break;
            batch.Data.Add(min);
            await Task.Delay(10);
        }
        
        batch.ToFile();
        
        foreach (var connection in Connections)
        {
            connection.Close();
            connection.CsvFile.Delete();
        }
        MessageBox.Show("End");
    }

    public async Task ReadRecords()
    {
        for (var i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].BufferedRecord is null)
            {
                var record = Connections[i].ReadRecord();
                if (record is not null)
                {
                    Connections[i].BufferedRecord = record;
                    Values.Add(record);
                }
            }

            await Task.Delay(100);
        }
    }

    private bool Min(string propertyName, Type propertyType, out object? min)
    {
        min = null;
        int minIndex = -1;
        for (int i = 0; i < Connections.Count; i++)
        {
            var temp = min;
            min = min.Min(Connections[i].BufferedRecord, propertyName, propertyType);

            if (temp != null && min != null)
            {
                Records.Add(new Record()
                {
                    RowIndex1 = i - 1,
                    RowIndex2 = i
                });
            }
            // Check if min is updated
            if (temp != min)
                minIndex = i;
        }

        if (minIndex != -1)
        {
            Values.Remove(Connections[minIndex].BufferedRecord);
            Connections[minIndex].BufferedRecord = null;
            Records.Add(new Record() {RowIndex1 = -1, RowIndex2 = -1, InsertedId = minIndex});
        }
        
        return min is not null;
    }

    private ObservableCollection<Connection> OpenConnections(string[] fileNames)
    {
        var connections = new ObservableCollection<Connection>();

        foreach (var fileName in fileNames)
        {
            var connection = new Connection(new FileInfo(fileName));
            
            if(connection.Status)
                connections.Add(connection);
        }

        return connections;
    }

    private void CloseDeleteConnections(ICollection<Connection> connections)
    {
        foreach (var connection in connections)
        {
            connection.Close();
        }
    }
}