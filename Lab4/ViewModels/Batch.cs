using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Lab4.Models;

namespace Lab4.ViewModels
{
    internal class Batch<T> : ObservableObject
    {
        public ObservableCollection<T> Data { get; set; }
        public ObservableCollection<Record> Log { get; set; }
        public double Delay { get; set; }

        public Batch()
        {
            Data = new ObservableCollection<T>();
            Log = new ObservableCollection<Record>();
        }

        public bool IsBigger(int i1, int i2, string property, Type type, out Record record)
        {
            IComparable property1 = null;
            IComparable property2 = null;
            try
            {
                property1 = (IComparable)GetProperty(i1, property, type);
                property2 = (IComparable)GetProperty(i2, property, type);
            } catch (Exception ex)
            {
                MessageBox.Show("Check selected column of csv file. Wrong type.");
            }

            record = new Record() { RowIndex1 = i1, RowIndex2 = i2 };
            Log.Add(record);

            if (property1.CompareTo(property2) == 1)              
                return true;
            
            return false;
        }

        public void Swap(int i1, int i2, Record record)
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

        public void Clear()
        {
            Data.Clear();
            Log.Clear();
        }

        public static bool IsBigger(object obj1, object obj2, string property, Type type)
        {
            IComparable property1 = null;
            IComparable property2 = null;
            try
            {
                property1 = (IComparable)GetProperty(obj1, property, type);
                property2 = (IComparable)GetProperty(obj2, property, type);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Check selected column of csv file. Wrong type.");
            }

            if (property1.CompareTo(property2) == 1)
                return true;

            return false;
        }

        public static object? GetProperty(object obj, string property, Type type)
        {
            return Convert.ChangeType(
                obj.GetType()
                .GetProperty(property)
                .GetValue(obj, null), type);
        }
    }
}
