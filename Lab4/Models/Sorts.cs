using System.Threading.Tasks;
using Lab4.ViewModels;
using Lab4.Models;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static async void Sort(Batch<object> batch1, Batch<object> batch2)
        {
            
        }

        public static async void Merge(Batch<object> batch1, Batch<object> batch2, Batch<object> mergeBatch)
        {
            var i = 0;
            var j = 0;
            var k = 0;

            while(k < batch1.Data.Count + batch2.Data.Count)
            {
                bool result;
                if(j != batch2.Data.Count &&
                    (i == batch1.Data.Count ||
                    (result = Batch<object>.IsBigger(batch1.Data[i], batch2.Data[j], "Age", typeof(int))))
                    )
                {
                    mergeBatch.Data.Add(batch2.Data[j]);
                    mergeBatch.Log.Add(new Record() { 
                        RowIndex1 = i, RowIndex2 = j, InsertedId = j
                    });
                    j += 1;
                }
                else
                {
                    mergeBatch.Data.Add(batch1.Data[i]);
                    mergeBatch.Log.Add(new Record()
                    {
                        RowIndex1 = i,
                        RowIndex2 = j,
                        InsertedId = i
                    });
                    i += 1;
                }
                k++;
                await Task.Delay(100);
            }
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
                    await Task.Delay(50);
                }
            }

            return 1;
        }
    }
}
