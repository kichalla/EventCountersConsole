using System;

namespace EventCountersConsole
{
    public class DataCell : Cell
    {
        public DataCell(string data, int width) : base(data, width)
        {
        }

        protected override void Draw()
        {
            WriteData(Data);
        }

        public void UpdateData(string data)
        {
            WriteData(data);
        }

        private void WriteData(string data)
        {
            if (data.Length < Width)
            {
                data += new string(' ', Width - data.Length);
            }
            Console.SetCursorPosition(CursorColumnIndex, CursorRowIndex);
            Console.Write(data);
        }
    }
}
