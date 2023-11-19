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

    public Connection(FileInfo csvFile)
    {
        CsvFile = csvFile;
        try
        {
            Reader = new CsvReader(new StreamReader(CsvFile.FullName), CultureInfo.InvariantCulture);
            
            if (!Reader.Read())
                throw new ArgumentException("No record in file.");
            
            CsvType = GetType(Reader);
            Status = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not read file {csvFile.FullName}. {ex.Message}");
        }
    }

    ~Connection()
    {
        if(Reader is not null)
            Reader.Dispose();
    }
    
    public Batch<object> ReadBatch(CsvReader csvReader, int batchSize, int counter)
    {
        var batch = new Batch<object>
        {
            FullPath = CsvFile.FullName + counter,
            RecordType = CsvType
        };

        for (var i = 0; i < batchSize - 1; i++)
        {
            var record = ReadRecord();
            
            if (record is null)
                break;
            
            batch.Data.Add(record);
        }

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
}