
namespace DynamicVizSegmenter
{
    /// <summary>
    /// Содержит сводную информацию о BookStore`s
    /// </summary>
    public class BooksInfo
    {
        private int _booksCount;
        private int _linesCount;
        private string _chars;
        private int[] _charCounts;

        /// <summary>
        /// Количество под-баз
        /// </summary>
        public int BooksCount
        {
            get { return _booksCount; }
            set { _booksCount = value; }
        }

        /// <summary>
        /// Общее количество елементов базы
        /// </summary>
        public int TotalLinesCount
        {
            get { return _linesCount; }
            set { _linesCount = value; }
        }

        /// <summary>
        /// Набор символов из базы
        /// </summary>
        public string Chars
        {
            get { return _chars; }
            set { _chars = value; }
        }

        public int[] CharCounts
        {
            get { return _charCounts; }
            set { _charCounts = value; }
        }
    }
}
