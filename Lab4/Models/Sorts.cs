using System.Threading.Tasks;
using Lab4.ViewModels;
using System;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static async Task<int> InsertionSort(Batch<object> batch, string propertyName, Type propertyType)
        {
            for (int i = 1; i < batch.Data.Count; i++)
            {
                int j = i - 1;
                bool result;
                while (j >= 0 && 
                       (result = batch.IsBigger(j, j+1, propertyName, propertyType, out var record)))
                {
                    await Task.Delay(500);
                    batch.Swap(j + 1, j, record);
                    j--;
                    await Task.Delay(1);
                }
                await Task.Delay(500);
            }

            return 1;
        }
    }
}
