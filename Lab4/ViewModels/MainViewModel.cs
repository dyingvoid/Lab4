using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class MainViewModel : ObservableObject
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

        private bool _canSortFile;

        public bool CanSortFile
        {
            get => _canSortFile;
            set
            {
                if (CurrentConnection is not null &&
                    PropertyName is not null &&
                    AsType is not null &&
                    BatchSize > 1)
                    SetProperty(ref _canSortFile, true);
                else
                    SetProperty(ref _canSortFile, false);
                
                SortFileCommand.NotifyCanExecuteChanged();
            }
        }

        private Batch<object> _currrentBatch;

        public Batch<object> CurrentBatch 
        { 
            get => _currrentBatch;
            set => SetProperty(ref _currrentBatch, value);
        }

        private Connection? _currentConnection;

        public Connection? CurrentConnection
        {
            get => _currentConnection;
            set
            {
                SetProperty(ref _currentConnection, value);
                CanSortFile = true;
            }
        }

        private string? _propertyName;

        public string? PropertyName
        {
            get => _propertyName;
            set
            {
                SetProperty(ref _propertyName, value);
                CanSortFile = true;
            }
        }

        private Merger _csvMerger;

        public Merger CsvMerger
        {
            get => _csvMerger;
            set => SetProperty(ref _csvMerger, value);
        }

        private int _batchSize;
        public int BatchSize
        {
            get => _batchSize;
            set
            {
                if (value <= 1)
                {
                    SetProperty(ref _batchSize, 0);
                    CanSortFile = false;
                    throw new ArgumentException("Size must be more than 1");
                }
                
                CanSortFile = true;
                SetProperty(ref _batchSize, value);
            }
        }

        private Type? _asType;
        public Type? AsType
        {
            get => _asType;
            set
            {
                if (value is null)
                {
                    SetProperty(ref _asType, value);
                    CanSortFile = false;
                    throw new ArgumentException("Wrong type");
                }

                CanSortFile = true;
                SetProperty(ref _asType, value);
            }
        }

        public ObservableCollection<string> CsvProperties { get; set; } = new();
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SortFileCommand { get; set; }

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(OpenFile, () => CanOpenFile);
            SortFileCommand = new RelayCommand(SortFile, () => CanSortFile);
        }

        private void OpenFile()
        {
            var factory = new ConnectionFactory();
            CurrentConnection = factory.StartConnection(CurrentFile);
            
            SingleConnectionType.CsvType
                .GetProperties()
                .Select(property => property.Name)
                .ToList()
                .ForEach(name => CsvProperties.Add(name));
            
            CanSortFile = true;
        }
        
        private async void SortFile()
        {
            if (CurrentConnection is null)
                return;
            
            var filePathes = new List<string>();
            try
            {
                while (true)
                {
                    var batch = CurrentConnection.ReadBatch(BatchSize);
                    if (batch.Data.Count == 0)
                        break;

                    CurrentBatch = batch;
                    var task = await Sorts.InsertionSort(batch, PropertyName, AsType);
                    filePathes.Add(CurrentBatch.FullPath);
                    CurrentBatch.ToFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            CsvMerger = new Merger(filePathes.ToArray());
            await CsvMerger.MultiPathMerge("Age", typeof(int));
        }
    }
}
