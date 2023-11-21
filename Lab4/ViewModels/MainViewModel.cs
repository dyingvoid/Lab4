using System;
using System.Collections.Generic;
using System.IO;
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

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(ReadFile, () => CanOpenFile);
            SortFileCommand = new RelayCommand(SortFile);

            CurrentFile = new FileInfo(@"C:\Users\Dying\RiderProjects\Lab4\Lab4\Files\Test.csv");
            ReadFile();
        }

        private async void SortFile()
        {
            //Task<int> t1 = Sorts.InsertionSort(CurrentBatch);
            //var res = await Task.WhenAll(t1);
        }
        private async void ReadFile()
        {
            int batchSize = 10;
            var factory = new ConnectionFactory();
            var connection = factory.StartConnection(CurrentFile);

            if (connection is null)
                return;
            
            var filePathes = new List<string>();
            while (true)
            {
                var batch = connection.ReadBatch(batchSize);
                if (batch.Data.Count == 0)
                    break;
                
                CurrentBatch = batch;
                var task = await Sorts.InsertionSort(batch, "Age", typeof(int));
                filePathes.Add(CurrentBatch.FullPath);
                CurrentBatch.ToFile();
            }

            var merger = new Merger(filePathes.ToArray());
            merger.MultiPathMerge("Age", typeof(int));
            
        }
    }
}
