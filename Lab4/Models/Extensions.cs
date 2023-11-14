using System.Collections.ObjectModel;

namespace Lab4.Models
{
    internal static class Extensions
    {
        public static void AddRange<T>(this ObservableCollection<T> destination, 
            ObservableCollection<T> source)
        {
            for(var i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }
    }
}
