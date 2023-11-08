using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class Batch<T> : ObservableObject
    {
        public ObservableCollection<T> Data { get; set; }
        public ISort SortingAlgorithm { get; set; }
        public RelayCommand SortCommand { get; set; }
        public double Delay { get; set; }

        public Batch()
        {
            Data = new ObservableCollection<T>();
            SortCommand = new RelayCommand(Sort);
        }

        public Batch(ISort sortingAlgorithm) : this()
        {
            SortingAlgorithm = sortingAlgorithm;
        }

        private void Sort()
        {
            if(SortingAlgorithm is not null)
                SortingAlgorithm.Sort(this);
        }
    }
}
