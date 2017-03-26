using System;

namespace EventCountersConsole
{
    public class HeaderDataCell : Cell
    {
        public HeaderDataCell(string data, int width) : base(data, width)
        {
        }

        protected override void Draw()
        {
            var data = Data;
            if (Data.Length < Width)
            {
                data += new string(' ', Width - data.Length);
            }
            Console.SetCursorPosition(CursorColumnIndex, CursorRowIndex);
            Console.Write(data);
        }
    }
}
