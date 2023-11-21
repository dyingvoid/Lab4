﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Lab4.ViewModels;

namespace Lab4.Models;

public class Merger
{
    public ObservableCollection<Connection> Connections { get; private set; }
    public ObservableCollection<object> Values { get; private init; }
    public ObservableCollection<Record> Records { get; set; }
    private List<bool> ValuesStatuses { get; set; }

    public Merger(string[] fileNames)
    {
        Connections = OpenConnections(fileNames);
        Values = new ObservableCollection<object>();
        Records = new ObservableCollection<Record>();
        ValuesStatuses = new List<bool>(new bool[Connections.Count]);
    }

    public async void MultiPathMerge(string propertyName, Type propertyType)
    {
        var batch = new Batch<object>()
        {
            FullPath = @"C:\Users\Dying\RiderProjects\Lab4\Lab4\Files\Sorted.csv",
            RecordType = SingleConnectionType.CsvType
        };

        while (true)
        {
            await ReadRecords();
            if (!Min(propertyName, propertyType, out var min))
                break;
            batch.Data.Add(min);
            await Task.Delay(100);
        }
        
        batch.ToFile();
        MessageBox.Show("End");
    }

    public async Task ReadRecords()
    {
        for (var i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].BufferedRecord is null)
            {
                var record = Connections[i].ReadRecord();
                if (record is not null)
                {
                    Connections[i].BufferedRecord = record;
                    Values.Add(record);
                }
            }

            await Task.Delay(200);
        }
    }

    private bool Min(string propertyName, Type propertyType, out object? min)
    {
        min = null;
        int minIndex = -1;
        for (int i = 0; i < Connections.Count; i++)
        {
            var temp = min;
            min = min.Min(Connections[i].BufferedRecord, propertyName, propertyType);

            // Check if min is updated
            if (temp != min)
                minIndex = i;
        }

        if (minIndex != -1)
        {
            Values.Remove(Connections[minIndex].BufferedRecord);
            Connections[minIndex].BufferedRecord = null;
        }

        return min is not null;
    }

    private ObservableCollection<Connection> OpenConnections(string[] fileNames)
    {
        var connections = new ObservableCollection<Connection>();

        foreach (var fileName in fileNames)
        {
            var connection = new Connection(new FileInfo(fileName));
            
            if(connection.Status)
                connections.Add(connection);
        }

        return connections;
    }
}