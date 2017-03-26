using System;

namespace EventCountersConsole
{
    public class RowBorderCell : Cell
    {
        public RowBorderCell(string data, int width) : base(data, width)
        {
        }

        protected override void Draw()
        {
            Console.SetCursorPosition(CursorColumnIndex, CursorRowIndex);
            Console.Write(new string(Data[0], Width));
        }
    }
}
