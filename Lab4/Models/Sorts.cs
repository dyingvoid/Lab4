using System.Threading.Tasks;
using Lab4.ViewModels;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static async void Sort(Batch<object> batch1, Batch<object> batch2)
        {
            
        }

        public static async void Merge(Batch<object> batch1, Batch<object> batch2)
        {

        }

        public static async Task<int> InsertionSort(Batch<object> batch)
        {
            for (int i = 1; i < batch.Data.Count; i++)
            {
                int j = i - 1;
                bool result;
                while (j >= 0 && (result = batch.IsBigger(j, j+1, "Age", typeof(int), out var record)))
                {
                    batch.Swap(j + 1, j, record);
                    j--;
                    await Task.Delay(1000);
                }
            }

            return 1;
        }
    }
}
