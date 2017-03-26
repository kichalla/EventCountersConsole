using System.Collections.Generic;

namespace EventCountersConsole
{
    public class EventCounterConfiguration
    {
        public int EventCounterIntervalInSeconds { get; set; }
        public List<EventSource> EventSources { get; set; }
        public List<Column> Columns { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public bool Show { get; set; }
        public int Width { get; set; }
    }

    public class EventSource
    {
        public string Name { get; set; }
        public List<string> EventCounters { get; set; }
    }
}
