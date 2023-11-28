using System;
using System.Collections.Generic;
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

        public static object? GetProperty(this object? record, string propertyName, Type propertyType)
        {
            return Convert.ChangeType(
                record.GetType()
                    .GetProperty(propertyName)
                    .GetValue(record, null), propertyType);
        }

        public static int CompareRecord(this object? record, object? other, string propertyName, Type propertyType)
        {
            if (record is null && other is null)
                return 0;
            if(record is null)
                return 1;
            if (other is null)
                return -1;

            var recordProperty = (IComparable)record.GetProperty(propertyName, propertyType);
            var otherProperty = (IComparable)other.GetProperty(propertyName, propertyType);

            return recordProperty.CompareTo(otherProperty);
        }

        public static object? Min(this object? record, object? other, string propertyName, Type propertyType)
        {
            var result = record.CompareRecord(other, propertyName, propertyType);

            if (result is 0 or -1)
                return record;
            return other;
        }

        public static void RemoveRangeAt<T>(this List<T> list, ICollection<int> indexes)
        {
            foreach (var index in indexes)
            {
                if(index > 0 && index < list.Count)
                    list.RemoveAt(index);
            }
        }
    }
}
