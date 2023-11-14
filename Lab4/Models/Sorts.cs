using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Lab4.ViewModels;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static async void SimpleSort(Batch<object> batch)
        {
            for (int i = 0; i < batch.Data.Count - 1; i++)
            {
                for (int j = 0; j < batch.Data.Count - i - 1; j++)
                {
                    batch.CompareSwapElements(j, j + 1, "Age", typeof(int));
                    await Task.Delay(1000);
                }
            }
        }

        public static async void Merge(Batch<object> batch1, Batch<object> batch2)
        {

        }

        public static async void InsertionSort(Batch<object> batch)
        {
            int i, j;
            object key;
            for (i = 1; i < batch.Data.Count; i++)
            {
                j = i - 1;
                bool result;
                while (j >= 0 && (result = batch.IsBigger(j, i, "Age", typeof(int), out var record)))
                {
                    batch.Swap(j + 1, j, record);
                    j = j - 1;
                    await Task.Delay(1000);
                }
            }
        }
    }
}
