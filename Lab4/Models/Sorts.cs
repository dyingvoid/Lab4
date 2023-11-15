using System.Threading.Tasks;
using Lab4.ViewModels;
using Lab4.Models;
using System.Windows.Documents;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Globalization;
using System;
using System.Diagnostics;

namespace Lab4.Models
{
    internal class Sorts
    {
        public static async Task<int> Merge(Batch<object> batch1, Batch<object> batch2, Batch<object> mergeBatch)
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
                await Task.Delay(200);
            }

            return 1;
        }

        public static void GreatMerge(List<string> pathes, Type type)
        {
            var readers = OpenStreams(pathes);
            var test = new List<object>();

            if (!TryRead(readers, type, out var batch))
                return;
            batch.RecordType = type;

            for(var i = 0; i < batch.Data.Count; i++)
            {
                int minIdx = batch.Min();
                
                test.Add(batch.Data[minIdx]);


                if (readers[minIdx].Read())
                {

                }
            }
           
        }

        private static bool TryRead(List<CsvReader> readers, Type type, out Batch<object> batch)
        {
            var counter = 0;
            batch = new Batch<object>();

            foreach (var reader in readers)
            {
                if (!reader.Read())
                    counter++;
                else
                    batch.Data.Add(CreateObjectRecord(type, reader.GetRecord<dynamic>()));
            }

            if (counter == readers.Count)
                return false;
            return true;
        }

        private static List<CsvReader> OpenStreams(List<string> pathes)
        {
            var readers = new List<CsvReader>();
            foreach (string path in pathes)
            {
                var reader = new StreamReader(path);
                readers.Add(new CsvReader(reader, CultureInfo.InvariantCulture));
            }

            return readers;
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
                    await Task.Delay(300);
                }
                await Task.Delay(200);
            }

            return 1;
        }

        private static object CreateObjectRecord(Type objectType, dynamic record)
        {
            object dynamicObject = Activator.CreateInstance(objectType);

            foreach (var data in record)
                dynamicObject.GetType().GetProperty(data.Key).SetValue(dynamicObject, data.Value);


            return dynamicObject;
        }
    }
}
