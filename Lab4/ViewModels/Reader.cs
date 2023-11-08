using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public Batch<object> CurrentBatch { get; set; } = new Batch<object>();
        
        public RelayCommand OpenFileCommand { get; set; }

        public Reader()
        {
            OpenFileCommand = new RelayCommand(ReadFile, () => CanOpenFile);
        }
        private void ReadFile()
        {
            try
            {
                using var reader = new StreamReader(CurrentFile.FullName);
                using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                if (!csvReader.Read())
                    return;

                CurrentBatch.Data.Clear();

                bool isEOF;
                var csvType = GetType(csvReader);

                while ((isEOF = csvReader.Read()) is not false)
                    CurrentBatch.Data.Add(CreateObjectRecord(csvType, csvReader.GetRecord<dynamic>()));
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
