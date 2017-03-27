using System;

namespace EventCountersConsole
{
    public class BorderCell : Cell
    {
        public BorderCell(string data) : base(data, data.Length)
        {
        }

        protected override void Draw()
        {
            Console.SetCursorPosition(CursorColumnIndex, CursorRowIndex);
            Console.Write(Data);
        }
    }
}
