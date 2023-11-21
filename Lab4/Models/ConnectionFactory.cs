using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using CsvHelper;

namespace Lab4.Models;

public class ConnectionFactory
{
    public FileInfo File { get; private set; }
    
    public Connection? StartConnection(FileInfo? file)
    {
        if (!TestConnection(file))
            return null;
        
        return new Connection(file);
    }

    private bool TestConnection(FileInfo? file)
    {
        if (file is null || !file.Exists)
            return false;

        try
        {
            using var stream = new StreamReader(file.FullName);
            using var reader = new CsvReader(stream, CultureInfo.InvariantCulture);

            if (!reader.Read())
                return false;
            
            SingleConnectionType.CsvType = GetRecordType(reader);
        }
        catch (Exception ex)
        {
            MessageBox.Show("File has problems");
            return false;
        }

        return true;
    }
    
    private Type GetRecordType(CsvReader reader)
    {
        var record = reader.GetRecord<dynamic>();
        var typeDict = new Dictionary<string, object>();

        foreach (var data in record)
            typeDict.TryAdd(data.Key, data.Value);


        return DataCreator.CreateType(typeDict);
    }
}