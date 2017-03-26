using System.Collections.Generic;

namespace EventCountersConsole
{
    public class Table
    {
        public List<IEnumerable<Cell>> Rows { get; } = new List<IEnumerable<Cell>>();
    }
}
