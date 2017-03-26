using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;

namespace EventCountersConsole
{
    class Program
    {
        private static Table _table;
        private static EventCounterConfiguration _configuration = null;

        static void Main(string[] args)
        {
            GetConfiguration();

            CreateTableSkeleton();

            DrawTable();

            TraceEventSession userSession = null;
            try
            {
                userSession = new TraceEventSession("ObserveHostingEventSource");
                RegisterEventListener(userSession);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            finally
            {
                if (userSession != null)
                {
                    userSession.Dispose();
                }
            }
        }

        static void GetConfiguration()
        {
            using (var streamReader = new StreamReader(File.OpenRead("config.json")))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonserializer = new JsonSerializer();
                    _configuration = jsonserializer.Deserialize<EventCounterConfiguration>(jsonTextReader);
                }
            }

            _configuration.Columns = _configuration.Columns
                .Where(column => column.Show)
                .ToList();
            _configuration.EventSources.Sort();
            foreach (var eventSource in _configuration.EventSources)
            {
                eventSource.EventCounters.Sort();
            }
        }

        static void CreateTableSkeleton()
        {
            _table = new Table();

            foreach (var eventSource in _configuration.EventSources)
            {
                // add one row for each counter we want to monitor
                foreach (var eventCounter in eventSource.EventCounters)
                {
                    var eventCounterRow = new List<RowDataCell>();
                    foreach (var column in _configuration.Columns)
                    {
                        RowDataCell dataCell;
                        if (column.Name == "Name")
                        {
                            dataCell = new RowDataCell(eventCounter, column.Width);
                        }
                        else
                        {
                            dataCell = new RowDataCell(string.Empty, column.Width);
                        }
                        eventCounterRow.Add(dataCell);
                    }
                    _table.Rows.Add(eventCounterRow);
                }
            }
        }

        static void DrawTable()
        {
            var tbl = new Table();
            AddTableHeader(tbl);
            AddTableDataRows(tbl);
            DrawTable(tbl);
        }

        static void AddTableHeader(Table tbl)
        {
            // Example:
            // ============================
            // |Name   |Age   |Occupation |
            // ============================
            tbl.Rows.Add(CreateBorderRow("=", "="));
            var headerDataRow = new List<Cell>();
            for (var i = 0; i < _configuration.Columns.Count; i++)
            {
                if (i == 0)
                {
                    headerDataRow.Add(new ColumnBorderCell("|"));
                }
                headerDataRow.Add(new HeaderDataCell(_configuration.Columns[i].Name, _configuration.Columns[i].Width));
                headerDataRow.Add(new ColumnBorderCell("|"));
            }
            tbl.Rows.Add(headerDataRow);
            tbl.Rows.Add(CreateBorderRow("=", "="));
        }

        static List<Cell> CreateBorderRow(string rowCharacter = "-", string columnCharacter = "|")
        {
            var borderRow = new List<Cell>();
            for (var i = 0; i < _configuration.Columns.Count; i++)
            {
                if (i == 0)
                {
                    borderRow.Add(new ColumnBorderCell(columnCharacter));
                }

                borderRow.Add(new RowBorderCell(rowCharacter, _configuration.Columns[i].Width));
                borderRow.Add(new ColumnBorderCell(columnCharacter));
            }
            return borderRow;
        }

        static void AddTableDataRows(Table tbl)
        {
            foreach (var rowCells in _table.Rows)
            {
                var dataRow = new List<Cell>();
                for (var i = 0; i < rowCells.Count(); i++)
                {
                    if (i == 0)
                    {
                        dataRow.Add(new ColumnBorderCell("|"));
                    }
                    dataRow.Add((RowDataCell)rowCells.ElementAt(i));
                    dataRow.Add(new ColumnBorderCell("|"));
                }
                tbl.Rows.Add(dataRow);
                tbl.Rows.Add(CreateBorderRow());
            }
        }

        static void DrawTable(Table table)
        {
            foreach (var row in table.Rows)
            {
                foreach (var cell in row)
                {
                    cell.Draw(Console.CursorTop, Console.CursorLeft);
                }

                // Move cursor to next line for drawing
                Console.WriteLine();
            }
        }

        static void RegisterEventListener(TraceEventSession userSession)
        {
            var options = new TraceEventProviderOptions();
            options.AddArgument("EventCounterIntervalSec", _configuration.EventCounterIntervalInSeconds.ToString());

            foreach (var eventSource in _configuration.EventSources)
            {
                userSession.EnableProvider(eventSource.Name, TraceEventLevel.Always, (ulong)EventKeywords.None, options);

                // Create a stream of the 'EventCounters' event source events
                IObservable<TraceEvent> eventStream = userSession.Source.Dynamic.Observe(eventSource.Name, "EventCounters");
                eventStream.Subscribe(onNext: traceEvent =>
                {
                    var payload = traceEvent.PayloadValue(0) as IDictionary<string, object>;
                    if (payload != null)
                    {
                        var eventCounterIndex = eventSource.EventCounters.FindIndex(n => n == payload["Name"].ToString());
                        var eventCounter = _table.Rows[eventCounterIndex] as List<RowDataCell>;

                        for (var i = 0; i < _configuration.Columns.Count; i++)
                        {
                            var rowDataCell = eventCounter[i];
                            rowDataCell.UpdateData(payload[_configuration.Columns[i].Name]?.ToString());
                        }
                    }
                });
            }

            // OK we are all set up, time to listen for events and pass them to the observers.  
            userSession.Source.Process();
        }
    }
}