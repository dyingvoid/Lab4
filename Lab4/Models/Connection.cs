using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using CsvHelper;
using Lab4.ViewModels;

namespace Lab4.Models;

public class Connection
{
    public FileInfo? CsvFile { get; init; }
    public CsvReader? Reader { get; init; }
    public bool Status { get; private set; }
    public Type? CsvType { get; private set; }
    public int Counter { get; private set; }

    private object? _last;
    public object? Last
    {
        get => _last;
        set => _last = value;
    }

    public Connection(FileInfo csvFile)
    {
        CsvFile = csvFile;
        try
        {
            Reader = new CsvReader(new StreamReader(CsvFile.FullName), CultureInfo.InvariantCulture);
            
            if (!Reader.Read())
                throw new ArgumentException("No record in file.");
            
            CsvType = GetType(Reader);
            Counter = 0;
            Status = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not read file {csvFile.FullName}.");
        }
    }

    ~Connection()
    {
        if(Reader is not null)
            Reader.Dispose();
    }
    
    public Batch<object> ReadBatch(int batchSize)
    {
        var batch = new Batch<object>
        {
            FullPath = CsvFile.FullName + Counter,
            RecordType = CsvType
        };

        for (var i = 0; i < batchSize - 1; i++)
        {
            var record = ReadRecord();
            
            if (record is null)
                break;
            
            batch.Data.Add(record);
        }

        Counter++;
        return batch;
    }

    public object? ReadRecord()
    {
        if (Reader is null)
        {
            Status = false;
            return null;
        }
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

    private Type GetType(CsvReader reader)
    {
        var record = reader.GetRecord<dynamic>();
        var typeDict = new Dictionary<string, object>();

        foreach (var data in record)
            typeDict.TryAdd(data.Key, data.Value);


        return DataCreator.CreateType(typeDict);
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