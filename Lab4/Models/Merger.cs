using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Lab4.ViewModels;

namespace Lab4.Models;

public class Merger
{
    public ObservableCollection<Connection> Connections { get; private set; }
    public ObservableCollection<object?> Values { get; private init; }

    public Merger(string[] fileNames)
    {
        Connections = OpenConnections(fileNames);
        Values = new ObservableCollection<object?>(new object?[Connections.Count]);
    }

    public void MultiPathMerge(string propertyName, Type propertyType)
    {
        var batch = new Batch<object>(){FullPath = @"C:\Users\Dying\RiderProjects\Lab4\Lab4\Files\Sorted.csv"};
        
        while (true)
        {
            ReadRecords();
            if (!Min(propertyName, propertyType, out var min))
                break;
            batch.Data.Add(min);
        }
        
        batch.TestToFile(Connections[0].CsvType);
    }

    public bool ReadRecords()
    {
        bool read = false;
        for (var i = 0; i < Values.Count; i++)
        {
            if (Values[i] is null)
            {
                Values[i] = Connections[i].ReadRecord();
                read |= Values[i] is not null;
            }
        }

        return read;
    }

    private bool Min(string propertyName, Type propertyType, out object? min)
    {
        min = null;
        int minIndex = -1;
        for (int i = 0; i < Values.Count; i++)
        {
            var temp = min;
            min = min.Min(Values[i], propertyName, propertyType);

            if (temp != min)
                minIndex = i;
        }
        
        if(minIndex != -1)
            Values[minIndex] = null;
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
}