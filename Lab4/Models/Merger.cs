using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace Lab4.Models;

public class Merger
{
    private List<CsvReader> _streams;
    private bool[] _streamStatuses;
    private ObservableCollection<object?> _values;

    public Merger(List<string> pathes)
    {
        _streams = OpenStreams(pathes);
        
        _streamStatuses = new bool[_streams.Count];
        for (var i = 0; i < _streamStatuses.Length; i++)
            _streamStatuses[i] = true;
        
        _values = new(new object?[_streams.Count]);
    }

    public void Merge()
    {
        
    }

    private void ReadValues()
    {
        for (var i = 0; i < _streams.Count; i++)
        {
            if (_streamStatuses[i] = _streams[i].Read())
                _values[i] = _streams[i].GetRecord<dynamic>();
        }
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
}