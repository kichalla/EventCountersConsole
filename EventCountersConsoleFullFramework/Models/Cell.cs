namespace EventCountersConsole
{
    public abstract class Cell
    {
        public Cell(string data, int width)
        {
            Data = data;
            Width = width;
        }

        protected string Data { get; }

        protected int Width { get; private set; }

        protected int CursorRowIndex { get; private set; }

        protected int CursorColumnIndex { get; private set; }

        public void Draw(int cursorRowIndex, int cursorColumnIndex)
        {
            CursorRowIndex = cursorRowIndex;
            CursorColumnIndex = cursorColumnIndex;

            Draw();
        }

        protected abstract void Draw();
    }
}
