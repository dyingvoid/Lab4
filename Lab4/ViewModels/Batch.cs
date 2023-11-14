using System;
using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class Batch<T> : ObservableObject
    {
        public ObservableCollection<T> Data { get; set; }
        public ObservableCollection<LogRecord> Log { get; set; }
        public double Delay { get; set; }

        public Batch()
        {
            Data = new ObservableCollection<T>();
            Log = new ObservableCollection<LogRecord>();
        }

        public void CompareSwapElements(int i1, int i2, string property, Type asType)
        {
            var property1 = (IComparable)GetProperty(i1, property, asType);
            var property2 = (IComparable)GetProperty(i2, property, asType);

            var logRecord = new LogRecord() { RowIndex1 = i1, RowIndex2 = i2 };

            if(property1.CompareTo(property2) == 1)
            {
                (Data[i2], Data[i1]) = (Data[i1], Data[i2]);
                logRecord.Swapped = true;
            }

            Log.Add(logRecord);
        }

        public bool IsBigger(int i1, int i2, string property, Type type, out LogRecord record)
        {
            var property1 = (IComparable)GetProperty(i1, property, type);
            var property2 = (IComparable)GetProperty(i2, property, type);

            record = new LogRecord() { RowIndex1 = i1, RowIndex2 = i2 };
            Log.Add(record);

            if (property1.CompareTo(property2) == 1)              
                return true;
            

            return false;
        }

        public void Swap(int i1, int i2, LogRecord record)
        {
            record.Swapped = true;
            (Data[i2], Data[i1]) = (Data[i1], Data[i2]);
        }

        public object? GetProperty(int index, string property, Type type)
        {
            var record = Data[index];
            return Convert.ChangeType(
                record.GetType()
                .GetProperty(property)
                .GetValue(record, null), type);
        }
    }
}
