using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DynamicVizSegmenter
{
    /// <summary>
    /// Логика взаимодействия для BooksInfoWindow.xaml
    /// </summary>
    public partial class BooksInfoWindow : Window
    {

        public BooksInfoWindow()
        {
            InitializeComponent();
        }

        public BooksInfoWindow(BooksInfo info)
            : this()
        {
            SetBooksInfo(info);
        }

        public void SetBooksInfo(BooksInfo info)
        {
            tbBooksCount.Text = info.BooksCount.ToString();
            tbLinesCount.Text = info.TotalLinesCount.ToString();
            tbChars.Text = info.Chars;
        }
    }
}
