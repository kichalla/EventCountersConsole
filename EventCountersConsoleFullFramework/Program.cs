﻿using Microsoft.Diagnostics.Tracing;
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
        private static EventCounterConfiguration _config = null;

        // A map of the event counter to the row it represents to give fast access when trying to update
        // the cells when a trace event gets fired.
        private static Dictionary<string, List<DataCell>> _eventCounters = null;

        static void Main(string[] args)
        {
            GetConfiguration();

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
                    _config = jsonserializer.Deserialize<EventCounterConfiguration>(jsonTextReader);
                }
            }

            _config.Columns = _config.Columns
                .Where(column => column.Show)
                .ToList();

            _config.EventSources.Sort();

            foreach (var eventSource in _config.EventSources)
            {
                eventSource.EventCounters.Sort();
            }
        }

        static void DrawTable()
        {
            var table = new Table();
            AddTableHeader(table);
            AddTableRows(table);
            DrawTable(table);
        }

        static void AddTableHeader(Table table)
        {
            // Example:
            // ============================
            // |Name   |Age   |Occupation |
            // ============================
            table.Rows.Add(CreateBorderRow('=', '='));
            var headerDataRow = new List<Cell>();
            for (var i = 0; i < _config.Columns.Count; i++)
            {
                if (i == 0)
                {
                    headerDataRow.Add(new BorderCell("|"));
                }
                headerDataRow.Add(new DataCell(_config.Columns[i].Name, _config.Columns[i].Width));
                headerDataRow.Add(new BorderCell("|"));
            }
            table.Rows.Add(headerDataRow);
            table.Rows.Add(CreateBorderRow('=', '='));
        }

        static void AddTableRows(Table table)
        {
            _eventCounters = new Dictionary<string, List<DataCell>>();

            foreach (var eventSource in _config.EventSources)
            {
                // add one row for each counter we want to monitor
                foreach (var eventCounter in eventSource.EventCounters)
                {
                    var dataCells = new List<DataCell>(); // for fast access without border cells
                    var allsCells = new List<Cell>();

                    for (var i = 0; i < _config.Columns.Count; i++)
                    {
                        if (i == 0)
                        {
                            allsCells.Add(new BorderCell("|"));
                        }

                        var column = _config.Columns[i];

                        DataCell dataCell;
                        if (column.Name == "Name")
                        {
                            dataCell = new DataCell(eventCounter, column.Width);
                        }
                        else
                        {
                            dataCell = new DataCell(string.Empty, column.Width);
                        }

                        dataCells.Add(dataCell);
                        allsCells.Add(dataCell);
                        allsCells.Add(new BorderCell("|"));
                    }
                    table.Rows.Add(allsCells);
                    table.Rows.Add(CreateBorderRow());

                    var key = $"{eventSource.Name}-{eventCounter}";
                    _eventCounters.Add(key, dataCells);
                }
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

        static List<Cell> CreateBorderRow(char rowCharacter = '-', char columnCharacter = '|')
        {
            string columnString = null;
            string rowString = null;
            var borderRow = new List<Cell>();
            for (var i = 0; i < _config.Columns.Count; i++)
            {
                if (i == 0)
                {
                    rowString = new string(rowCharacter, _config.Columns[i].Width);
                    columnString = columnCharacter.ToString();
                    borderRow.Add(new BorderCell(columnString));
                }
                borderRow.Add(new BorderCell(rowString));
                borderRow.Add(new BorderCell(columnCharacter.ToString()));
            }
            return borderRow;
        }

        static void RegisterEventListener(TraceEventSession userSession)
        {
            var options = new TraceEventProviderOptions();
            options.AddArgument("EventCounterIntervalSec", _config.EventCounterIntervalInSeconds.ToString());

            foreach (var eventSource in _config.EventSources)
            {
                userSession.EnableProvider(eventSource.Name, TraceEventLevel.Always, (ulong)EventKeywords.None, options);

                // Create a stream of the 'EventCounters' event source events
                IObservable<TraceEvent> eventStream = userSession.Source.Dynamic.Observe(eventSource.Name, "EventCounters");
                eventStream.Subscribe(onNext: traceEvent =>
                {
                    var payload = traceEvent.PayloadValue(0) as IDictionary<string, object>;
                    if (payload != null)
                    {
                        var key = $"{traceEvent.ProviderName}-{payload["Name"]}";
                        var eventCounterRow = _eventCounters[key];

                        for (var i = 0; i < _config.Columns.Count; i++)
                        {
                            var rowDataCell = eventCounterRow[i];
                            rowDataCell.UpdateData(payload[_config.Columns[i].Name]?.ToString());
                        }
                    }
                });
            }

            // OK we are all set up, time to listen for events and pass them to the observers.  
            userSession.Source.Process();
        }
    }
}