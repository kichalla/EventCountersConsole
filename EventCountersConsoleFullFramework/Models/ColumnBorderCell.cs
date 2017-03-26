using System;

namespace EventCountersConsole
{
    public class ColumnBorderCell : Cell
    {
        public ColumnBorderCell(string data) : base(data, width: 1)
        {
        }

        protected override void Draw()
        {
            WriteData(Data);
        }

        private void WriteData(string data)
        {
            Console.SetCursorPosition(CursorColumnIndex, CursorRowIndex);
            Console.Write(data);
        }
    }
}
