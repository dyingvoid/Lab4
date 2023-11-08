using Lab4.ViewModels;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static void SimpleSort<T>(Batch<T> batch)
        {
            for(int i = 0; i < batch.Data.Count - 1; i++)
            {
                for(int j = 0; j < batch.Data.Count - i - 1; j++)
                {
                    var item = batch.Data[i];
                    //int age = item.GetType().GetProperty("Age").GetValue(item, null);
                }
            }
        }
    }
}
