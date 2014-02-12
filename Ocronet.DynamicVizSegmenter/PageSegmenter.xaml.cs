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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Drawing;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Binarize;
using Ocronet.Dynamic.Segmentation.Line;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.ImgLib;

namespace DynamicVizSegmenter
{
    /// <summary>
    /// Interaction logic for PageSegmenter.xaml
    /// </summary>
    public partial class PageSegmenter : Page
    {
        private BookLine currBookLine;                  // ссылка на текущий елемент коллекции bookstoreSource
        private LineSource lineSource;                  // предоставляет доступ к файлам коллекции
        private BookLineList bookstoreSource;           // коллекция BookLine
        private CollectionViewSource listingDataView;   // представление коллекции BooksLine
        private int currSegmentsCount = 1;              // количество сегментов в текущем charseg
        private bool isCurrDeletedSegment = false;      // удалялся ли сегмент изображения
        private string bookStorePaths;                  // пути в базам картинок, разделенные ;

        public string BookStorePaths
        {
            get { return bookStorePaths; }
            set 
            {
                bookStorePaths = value;
                InitBookstoreSource(bookStorePaths);
            }
        }


        public PageSegmenter()
        {
            InitializeComponent();

            bookstoreSource = new BookLineList();
            listingDataView = new CollectionViewSource();
            listingDataView.Source = bookstoreSource;
            this.DataContext = listingDataView; // предоставим доступ к коллекции из языка разметки XAML
            listingDataView.View.CurrentChanged += new EventHandler(BookLine_CurrentChanged);

            lineSource = new LineSource();
            lineSource.PSet("randomize", 0);    // отключим случайное упорядочивание

            // Init BookstoreSource
            tbBookPath.Text = DynamicVizSegmenter.Properties.Settings.Default.BookPath;
            BookStorePaths = tbBookPath.Text;
        }

        ~PageSegmenter()
        {
            Properties.Settings.Default.BookPath = BookStorePaths;
            Properties.Settings.Default.Save();
        }

        public BookLineList BookstoreSource
        {
            get { return bookstoreSource; }
            set { bookstoreSource = value; }
        }

        public int CurrSegmentsCount
        {
            get { return currSegmentsCount; }
            set 
            {
                currSegmentsCount = value;
                numUpDnStart.MaxValue = value;
                numUpDnEnd.MaxValue = value;
            }
        }


        private void InitBookstoreSource(string bookPaths)
        {
            try
            {
                string[] books = bookPaths.Split(';');
                BookstoreSource.Clear();
                lineSource.Init(bookPaths);
                BookstoreSource.FromLineSource(lineSource);
                if (bookstoreSource.Count > 0)
                    listBooksLine.SelectedIndex = 0;
            }
            catch { }
        }

        private void SetListBoxFocus()
        {
            var index = listBooksLine.SelectedIndex;
            if (index < 0) index = 0;

            var item = listBooksLine.ItemContainerGenerator
                           .ContainerFromIndex(index) as ListBoxItem;
            item.Focus();
        }


        /// <summary>
        /// Изменился текущий BookLine
        /// </summary>
        private void BookLine_CurrentChanged(object sender, EventArgs e)
        {
            if (listingDataView.View.CurrentItem == null)
                return;

            // сбросим кэш предыдущего елемента
            if (currBookLine != null)
            {
                currBookLine.Image = null;
                currBookLine.ImageBytearray = null;
                currBookLine.CharsegIntarray = null;
            }
            // текущий елемент
            BookLine bline = listingDataView.View.CurrentItem as BookLine;
            currBookLine = bline;
            // сбросим некоторые признаки
            isCurrDeletedSegment = false;

            // показать Charseg
            if (bline != null && bline.HaveCharseg)
            {
                // загрузим с файла
                Intarray charseg = bline.CharsegIntarray;
                // занесем в кэш
                bline.CharsegIntarray = charseg;
                // номер максимального сегмента
                CurrSegmentsCount = NarrayUtil.Max(charseg);
                // отобразим картинку
                ShowCharsegImage(charseg, bline.Transcript);
                // активируем кнопки действий
                EnableCharsegCmdButtons();
            }
            else
            {
                imgCharSeg.Source = null;
                DisableCharsegCmdButtons();
            }

            // показать Transcript
            if (bline != null && bline.HaveTranscript)
            {
                ShowTranscript(bline.Transcript);
            }
            else
                ShowTranscript("");
            // сбросим к началу номер начальног осегмента
            numUpDnStart.Value = 1;
            numUpDnEnd.Value = 2;
        }

        private void ShowCharsegImage(Intarray charseg, string transcript)
        {
            Bitmap bitmap = UiHelper.ConvertCharsegToBitmapRecolor(charseg, transcript);
            imgCharSeg.Source = bitmap.ToImageSource();
        }

        private void ShowTranscript(string transcript)
        {
            tbTranscript.Text = transcript;
        }


        #region TextBox BookPath helper event handlers
        private void ReloadBookPath(object sender, RoutedEventArgs e)
        {
            TextBox tb = tbBookPath;
            if (tb.Text.Length > 0)
            {
                BookStorePaths = tb.Text;
            }
        }

        private void tbBookPath_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                TextBox tb = sender as TextBox;
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                Keyboard.ClearFocus();
            }
        }
        #endregion // TextBox BookPath helper event handlers

        #region ListBox of BooksLine toolbar event handlers
        private void GroupByHaveCharseg_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tbutton = sender as ToggleButton;
            if (tbutton.IsChecked == true)
            {
                PropertyGroupDescription groupDescription = new PropertyGroupDescription();
                groupDescription.PropertyName = "HaveCharseg";
                listingDataView.GroupDescriptions.Add(groupDescription);
            }
            else
            {
                listingDataView.GroupDescriptions.Clear();
            }
        }

        private void AddItemsToBase_Click(object sender, RoutedEventArgs e)
        {
            if (tbBookPath.Text.Length == 0)
                return;

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Image"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "PNG images (.png)|*.png|JPEG images (.jpg)|*.jpg"; // Filter files by extension
            dlg.Multiselect = true;
            dlg.Title = "Выберите один или несколько файлов";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                var filenames = dlg.FileNames.OrderBy(name => name);
                // check method (0 - default, 1 - with text)
                int method = 0;
                Button btn = sender as Button;
                if (btn.Tag != null)
                {
                    method = int.Parse(btn.Tag.ToString());
                }

                foreach (string fname in filenames)
                {
                    Bitmap bitmap = ImgIo.LoadBitmapFromFile(fname);
                    int lineid = lineSource.PutNewImage(bitmap);
                    if (method == 1)
                    {
                        string str = System.IO.Path.GetFileNameWithoutExtension(fname);
                        lineSource.PutTranscript(str, lineSource.CurrentBook, lineSource.CurrentPage, lineid);
                    }
                }
                InitBookstoreSource(tbBookPath.Text);
            }

        }

        private void ShowBooksInfo_Click(object sender, RoutedEventArgs e)
        {
            // собираем сводную информацию
            BooksInfo info = new BooksInfo();
            info.BooksCount = lineSource.BooksCount;
            info.TotalLinesCount = lineSource.Length;
            int[] charCounts;
            info.Chars = lineSource.GetCharset(out charCounts);
            info.CharCounts = charCounts;
            // создаем окно для вывода информации
            BooksInfoWindow infowin = new BooksInfoWindow(info);
            // отображаем окно
            infowin.ShowDialog();
        }
        #endregion // ListBox of BooksLine toolbar event handlers

        private void EnableCharsegCmdButtons()
        {
            btSaveCharseg.IsEnabled = true;
            btMergeSegments.IsEnabled = true;
            btDeleteSegment.IsEnabled = true;
        }
        private void DisableCharsegCmdButtons()
        {
            btMergeSegments.IsEnabled = false;
            btSaveCharseg.IsEnabled = false;
            btDeleteSegment.IsEnabled = false;
        }

        private void PutTranscriptToBookLine(object sender, RoutedEventArgs e)
        {
            string trans = tbTranscript.Text;
            if (!String.IsNullOrWhiteSpace(trans) && currBookLine != null)
            {
                currBookLine.Transcript = trans;
                lineSource.PutTranscript(trans,
                    currBookLine.Bookno, currBookLine.Pageno, currBookLine.Lineno);
                SetListBoxFocus();
            }
        }

        private void PutCharsegToBookLine(object sender, RoutedEventArgs e)
        {
            if (currBookLine.CharsegIntarray != null && currBookLine != null)
            {
                lineSource.PutCharSegmentation(currBookLine.CharsegIntarray,
                    currBookLine.Bookno, currBookLine.Pageno, currBookLine.Lineno);
                // если удалялся сегмент, нужно пересохранить оригинал также
                if (isCurrDeletedSegment)
                {
                    lineSource.PutImage(currBookLine.Image, currBookLine.Bookno, currBookLine.Pageno, currBookLine.Lineno);
                }
                SetListBoxFocus();
            }
        }

        private void ProcessSegmentationMethod(object sender, RoutedEventArgs e)
        {
            string strSermenterName = (sender as Control).Tag.ToString();
            ISegmentLine segmenter = ComponentCreator.MakeComponent<ISegmentLine>(strSermenterName);
            if (segmenter == null || currBookLine == null)
                return;

            // приведем к чернобелому
            Bytearray image = currBookLine.ImageBytearray;
            //IBinarize binarizer = new BinarizeByOtsu();
            //binarizer.Binarize(image, image);
            OcrRoutine.binarize_simple(image, image);
            // сегментация
            Intarray charseg = new Intarray();
            segmenter.Charseg(ref charseg, image);
            // фон равен 0
            SegmRoutine.make_line_segmentation_black(charseg);
            // удалим маленькие сегменты
            SegmRoutine.remove_small_components(charseg, 3, 3);
            ImgLabels.renumber_labels(charseg, 1);

            currBookLine.CharsegIntarray = charseg;
            CurrSegmentsCount = NarrayUtil.Max(charseg);
            // Show segmented image
            ShowCharsegImage(charseg,
                (currBookLine.HaveTranscript && CurrSegmentsCount == currBookLine.Transcript.Length) ?
                    currBookLine.Transcript : "");
            // to enable save button
            EnableCharsegCmdButtons();
        }

        private void ProcessMergeSegments(object sender, RoutedEventArgs e)
        {
            int start = numUpDnStart.Value ?? 1;
            int end = numUpDnEnd.Value ?? 1;
            start = Math.Min(start, CurrSegmentsCount);
            end = Math.Min(end, CurrSegmentsCount);

            SegmRoutine.rseg_to_cseg(currBookLine.CharsegIntarray, currBookLine.CharsegIntarray, start, end);
            
            // пересчитаем число сегментов
            CurrSegmentsCount = NarrayUtil.Max(currBookLine.CharsegIntarray);
            // Show segmented image
            ShowCharsegImage(currBookLine.CharsegIntarray,
                (currBookLine.HaveTranscript && CurrSegmentsCount == currBookLine.Transcript.Length) ?
                    currBookLine.Transcript : "");
        }

        private void ProcessDeleteSegment(object sender, RoutedEventArgs e)
        {
            int start = numUpDnStart.Value ?? 1;
            start = Math.Min(start, CurrSegmentsCount);

            Bytearray outgray = new Bytearray();
            SegmRoutine.rseg_to_cseg_remove(
                currBookLine.CharsegIntarray, currBookLine.CharsegIntarray,
                outgray, currBookLine.ImageBytearray, start, start);

            // пересчитаем число сегментов
            CurrSegmentsCount = NarrayUtil.Max(currBookLine.CharsegIntarray);
            // Show segmented image
            ShowCharsegImage(currBookLine.CharsegIntarray,
                (currBookLine.HaveTranscript && CurrSegmentsCount == currBookLine.Transcript.Length) ?
                    currBookLine.Transcript : "");

            currBookLine.ImageBytearray = outgray;
            currBookLine.Image = ImgRoutine.NarrayToRgbBitmap(outgray);
            isCurrDeletedSegment = true;
        }


        private void numUpDnStart_ValueChanged(object sender, RoutedEventArgs e)
        {
            int start = numUpDnStart.Value ?? 1;
            if (start > CurrSegmentsCount)
                numUpDnStart.Value = start = CurrSegmentsCount;

            if (numUpDnEnd == null)
                return;
            int end = numUpDnEnd.Value ?? 1;
            if (end <= start && end < numUpDnEnd.MaxValue)
                numUpDnEnd.Value = start + 1;
        }

        private void numUpDnEnd_ValueChanged(object sender, RoutedEventArgs e)
        {
            int end = numUpDnEnd.Value ?? 1;
            if (end > CurrSegmentsCount)
                numUpDnEnd.Value = end = CurrSegmentsCount;

            if (numUpDnStart == null)
                return;
            int start = numUpDnStart.Value ?? 1;
            if (start >= end && start > numUpDnStart.MinValue)
                numUpDnStart.Value = Math.Max(end - 1, 1);
        }

    }
}
