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

        private Batch<object> _currrentBatch;

        public Batch<object> CurrentBatch 
        { 
            get => _currrentBatch;
            set
            {
                SetProperty(ref _currrentBatch, value);
            }
        }
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
            Task<int> t1 = Sorts.InsertionSort(CurrentBatch);
            var res = await Task.WhenAll(t1);
        }
        private async void ReadFile()
        {
            try
            {
                int batch_size = 8;
                int counter = 0;

                using var reader = new StreamReader(CurrentFile.FullName);
                using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

                var pathes = new List<string>();

                while (true)
                {
                    var batch = ReadBatch(csvReader, batch_size);
                    if (batch is null)
                        break;

                    batch.FullPath = CurrentFile.FullName+counter;

                    CurrentBatch = batch;

                    await Sorts.InsertionSort(CurrentBatch);

                    CurrentBatch.ToFile();
                    counter++;
                    pathes.Add(CurrentBatch.FullPath);
                }

                Sorts.GreatMerge(pathes, CurrentBatch.RecordType);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private Batch<object>? ReadBatch(CsvReader csvReader, int batchSize)
        {
            if (!csvReader.Read())
                return null;

            var batch = new Batch<object>();

            bool isEOF = false;
            var csvType = GetType(csvReader);
            batch.RecordType = csvType;

            batch.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
            for (var i = 0; i < batchSize - 1; i++)
            {
                if ((isEOF = csvReader.Read()) is not false)
                {
                    batch.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
                }
            }

            return batch;
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
