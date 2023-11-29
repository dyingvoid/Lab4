using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private FileInfo? _file;
        public FileInfo? CurrentFile
        {
            get => _file;
            set
            {
                if (value is null)
                {
                    CanOpenFile = false;
                    SetProperty(ref _file, value);
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
                    BatchSize > 1 &&
                    CanOpenFile)
                    SetProperty(ref _canSortFile, true);
                else
                    SetProperty(ref _canSortFile, false);
                
                SortFileCommand.NotifyCanExecuteChanged();
                RofloSort.NotifyCanExecuteChanged();
            }
        }

        private Batch<object> _currrentBatch1;
        public Batch<object> CurrentBatch1 
        { 
            get => _currrentBatch1;
            set => SetProperty(ref _currrentBatch1, value);
        }
        
        private Batch<object> _currrentBatch2;
        public Batch<object> CurrentBatch2 
        { 
            get => _currrentBatch2;
            set => SetProperty(ref _currrentBatch2, value);
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

        private Merger _csvMerger;

        public Merger CsvMerger
        {
            get => _csvMerger;
            set => SetProperty(ref _csvMerger, value);
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

        private string _parameter;

        public string Parameter
        {
            get => _parameter;
            set => SetProperty(ref _parameter, value);
        }

        public ObservableCollection<string> CsvProperties { get; set; } = new();
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SortFileCommand { get; set; }
        public RelayCommand RofloSort { get; set; }

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(OpenFile, () => CanOpenFile);
            RofloSort = new RelayCommand(SortFile, () => CanSortFile);
            SortFileCommand = new RelayCommand(Test, () => CanSortFile);
        }

        private void OpenFile()
        {
            var factory = new ConnectionFactory();
            CurrentConnection = factory.StartConnection(CurrentFile);
            
            CsvProperties.Clear();
            SingleConnectionType.CsvType
                .GetProperties()
                .Select(property => property.Name)
                .ToList()
                .ForEach(name => CsvProperties.Add(name));
            
            CanSortFile = true;
        }

        private async void Test()
        {
            CsvMerger = new Merger();
            CsvMerger.ExternalMerge(CurrentFile, PropertyName, AsType, Parameter);
        }
        
        private async void SortFile()
        {
            if (CurrentConnection is null)
                return;
            
            var filePathes = new List<string>();
            try
            {
                await AsyncInsertionSort(filePathes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            CsvMerger = new Merger(filePathes.ToArray());
            await CsvMerger.MultiPathMerge(PropertyName, AsType);

            OpenFile();
        }

        private async Task AsyncInsertionSort(List<string> filePathes)
        {
            while (true)
            {
                Batch<object> batch1 = null;
                Batch<object> batch2 = null;
                
                if (Parameter is null || Parameter.Length == 0)
                {
                    batch1 = CurrentConnection.ReadBatch(BatchSize);
                    batch2 = CurrentConnection.ReadBatch(BatchSize);
                }
                else
                {
                    batch1 = CurrentConnection.ReadBatch(BatchSize, Parameter);
                    batch2 = CurrentConnection.ReadBatch(BatchSize, Parameter);
                }

                if (batch1.Data.Count == 0 && batch2.Data.Count == 0)
                    break;

                CurrentBatch1 = batch1;
                CurrentBatch2 = batch2;

                if (CurrentBatch1.Data.Count != 0 && CurrentBatch2.Data.Count != 0)
                {
                    var task1 = Sorts.InsertionSort(CurrentBatch1, PropertyName, AsType);
                    var task2 = Sorts.InsertionSort(CurrentBatch2, PropertyName, AsType);

                    var res = await Task.WhenAll(task1, task2);
                    filePathes.Add(CurrentBatch1.FullPath);
                    filePathes.Add(CurrentBatch2.FullPath);

                    CurrentBatch1.ToFile();
                    CurrentBatch2.ToFile();
                }
                else if (CurrentBatch1.Data.Count != 0)
                {
                    var task = await Sorts.InsertionSort(batch1, PropertyName, AsType);
                    filePathes.Add(CurrentBatch1.FullPath);
                    CurrentBatch1.ToFile();
                }
                else
                {
                    var task = await Sorts.InsertionSort(batch2, PropertyName, AsType);
                    filePathes.Add(CurrentBatch2.FullPath);
                    CurrentBatch2.ToFile();
                }
            }
        }
    }
}
