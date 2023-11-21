using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using CsvHelper;
using Lab4.ViewModels;

namespace Lab4.Models;

public class Connection
{
    public FileInfo CsvFile { get; init; }
    public CsvReader Reader { get; init; }
    public bool Status { get; set; }
    public Type CsvType { get; set; }
    private int Counter { get; set; }
    private StreamReader Stream { get; set; }

    public Connection(FileInfo csvFile)
    {
        CsvFile = csvFile;
        Stream = new StreamReader(CsvFile.FullName);
        Reader = new CsvReader(Stream, CultureInfo.InvariantCulture);
        CsvType = SingleConnectionType.CsvType;
        Counter = 0;
        Status = true;
    }

    ~Connection()
    {
        Stream.Dispose();
        Reader.Dispose();
    }
    
    public Batch<object> ReadBatch(int batchSize)
    {
        var batch = new Batch<object>
        {
            FullPath = CsvFile.FullName + Counter,
            RecordType = CsvType
        };

        var counter = 0;
        while(counter < batchSize)
        {
            var record = ReadRecord();
            
            if (record is null)
                break;
            
            batch.Data.Add(record);
            counter++;
        }

        Counter++;
        return batch;
    }

    public object? ReadRecord()
    {
        if (Status == false || (Status = Reader.Read()) == false)
            return null;

        return CreateObjectRecord(Reader.GetRecord<dynamic>());
    }

    private object? CreateObjectRecord(dynamic record)
    {
        object dynamicObject = Activator.CreateInstance(CsvType);

        foreach(var data in record)
            dynamicObject.GetType().GetProperty(data.Key).SetValue(dynamicObject, data.Value);
        
        return dynamicObject;
    }
    
    // For debug only
    public void CheckTypes()
    {
        object? obj = null;
        while((obj = ReadRecord()) != null)
            if (obj.GetType() != CsvType)
                throw new Exception("Wrong type");
    }
}