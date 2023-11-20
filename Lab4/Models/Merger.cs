using System.Collections.ObjectModel;
using System.IO;

namespace Lab4.Models;

public class Merger
{
    public ObservableCollection<Connection> Connections { get; private set; }
    public ObservableCollection<object?> Values { get; private init; }

    public Merger(string[] fileNames)
    {
        Connections = OpenConnections(fileNames);
        Values = new ObservableCollection<object?>(new object?[Connections.Count]);
    }

    public void ReadRecords()
    {
        for (var i = 0; i < Values.Count; i++)
        {
            if (Values[i] is null)
            {
                Values[i] = Connections[i].ReadRecord();
            }
        }
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