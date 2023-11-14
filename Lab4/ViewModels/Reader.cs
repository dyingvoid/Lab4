using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class Reader : ObservableObject
    {
        private FileInfo _file;
        public FileInfo? CurrentFile
        {
            get => _file;
            set
            {
                if (value is null)
                {
                    CanOpenFile = false;
                    throw new ArgumentNullException("File path is wrong");
                }

                CanOpenFile = value.Exists;
                if (!CanOpenFile)
                    throw new ArgumentException("File does not exist.");

                SetProperty(ref _file, value);
            }
        }

        private bool _canOpenFile;
        public bool CanOpenFile
        {
            get => _canOpenFile;
            set
            {
                SetProperty(ref _canOpenFile, value);
                OpenFileCommand.NotifyCanExecuteChanged();
            }
        }

        public Batch<object> CurrentBatch1 { get; set; } = new Batch<object>();
        public Batch<object> CurrentBatch2 { get; set; } = new Batch<object>();
        public Batch<object> MergedBatch { get; set; } = new Batch<object>();
        
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SortFileCommand { get; set; }

        public Reader()
        {
            OpenFileCommand = new RelayCommand(ReadFile, () => CanOpenFile);
            SortFileCommand = new RelayCommand(SortFile);
        }

        private async void SortFile()
        {
            Task<int> t1 = Sorts.InsertionSort(CurrentBatch1);
            Task<int> t2 = Sorts.InsertionSort(CurrentBatch2);
            var res = await Task.WhenAll(t1, t2);

            Sorts.Merge(CurrentBatch1, CurrentBatch2, MergedBatch);

        }
        private async void ReadFile()
        {
            try
            {
                int batch_size = 1;

                while (true)
                {
                    using var reader = new StreamReader(CurrentFile.FullName);
                    using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                    if (!csvReader.Read())
                        return;

                    CurrentBatch1.Clear();
                    CurrentBatch2.Clear();
                    MergedBatch.Clear();

                    bool isEOF = false;
                    var csvType = GetType(csvReader);

                    CurrentBatch1.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
                    for (var i = 0; i < batch_size; i++)
                    {
                        if ((isEOF = csvReader.Read()) is not false)
                        {
                            CurrentBatch1.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
                        }
                        else
                        {
                            return;
                        }
                    }

                    for (var i = 0; i <= batch_size; i++)
                    {
                        if ((isEOF = csvReader.Read()) is not false)
                        {
                            CurrentBatch2.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
                        }
                    }

                    Task<int> t1 = Sorts.InsertionSort(CurrentBatch1);
                    Task<int> t2 = Sorts.InsertionSort(CurrentBatch2);
                    var res = await Task.WhenAll(t1, t2);

                    Task<int> merge = Sorts.Merge(CurrentBatch1, CurrentBatch2, MergedBatch);
                    var res2 = await Task.WhenAll(merge);

                    reader.Close();

                    using var fstream = File.Create(CurrentFile.FullName + batch_size);
                    using var writer = new StreamWriter(fstream);
                    using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

                    csvWriter.WriteRecords<dynamic>(MergedBatch.Data);

                    CurrentFile = new FileInfo(CurrentFile.FullName + batch_size);
                    batch_size *= 2;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private object CreateObjectRecord(Type objectType, dynamic record)
        {
            object dynamicObject = Activator.CreateInstance(objectType);

            foreach(var data in record)
                dynamicObject.GetType().GetProperty(data.Key).SetValue(dynamicObject, data.Value);
            

            return dynamicObject;
        }

        private Type GetType(CsvReader reader)
        {
            var record = reader.GetRecord<dynamic>();
            var typeDict = new Dictionary<string, object>();

            foreach (var data in record)
                typeDict.TryAdd(data.Key, data.Value);


            return DataCreator.CreateType(typeDict);
        }
    }
}
