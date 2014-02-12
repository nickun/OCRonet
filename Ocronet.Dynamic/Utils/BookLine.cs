using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Utils
{
    public class BookLine : INotifyPropertyChanged
    {
        private int _Bookno;
        private int _Pageno;
        private int _Lineno;
        private Bitmap _image;
        private Bytearray _imageBytearray;
        private Intarray _charsegIntarray;
        private string _transcript;
        private LineSource _LineSource;
        public event PropertyChangedEventHandler PropertyChanged;

        public BookLine(int bookno, int pageno, int lineno)
        {
            _Bookno = bookno;
            _Pageno = pageno;
            _Lineno = lineno;
        }

        #region Property Getters and Setters
        public int Bookno
        {
            get { return _Bookno; }
        }

        public int Pageno
        {
            get { return _Pageno; }
        }

        public int Lineno
        {
            get { return _Lineno; }
        }

        public string ImagePath
        {
            get
            {
                if (_LineSource == null)
                    return "";
                FileInfo fi = new FileInfo(_LineSource.GetPath(_Bookno, _Pageno, _Lineno) + ".png");
                return fi.FullName;
            }
        }

        public string Transcript
        {
            get
            {
                if (_LineSource == null)
                    return null;
                if (_transcript != null)
                    return _transcript;
                return _LineSource.GetTranscript(_Bookno, _Pageno, _Lineno);
            }
            set
            {
                _transcript = value;
                //OnPropertyChanged("HaveTranscript");
                OnPropertyChanged("Transcript");
            }
        }

        public Bitmap Image
        {
            get
            {
                if (_LineSource == null)
                    return null;
                if (_image != null)
                    return _image;
                return _LineSource.GetImage(_Bookno, _Pageno, _Lineno);
            }
            set
            {
                _image = value;
                OnPropertyChanged("HaveImage");
                OnPropertyChanged("Image");
            }
        }

        public Bytearray ImageBytearray
        {
            get
            {
                if (_LineSource == null)
                    return null;
                if (_imageBytearray != null)
                    return _imageBytearray;
                Bytearray resarray = new Bytearray();
                if (_LineSource.GetImage(resarray, _Bookno, _Pageno, _Lineno))
                    return resarray;
                else
                    return null;
            }
            set
            {
                _imageBytearray = value;
                OnPropertyChanged("ImageBytearray");
            }
        }

        /*public Bitmap CharsegImage
        {
            get
            {
                if (_LineSource == null)
                    return null;
                if (_charsegImage != null)
                    return _charsegImage;
                return _LineSource.GetCharSegmentation(_Bookno, _Pageno, _Lineno);
            }
            set
            {
                _charsegImage = value;
                OnPropertyChanged("HaveCharseg");
                OnPropertyChanged("CharsegImage");
            }
        }*/

        public int CharsegImageHeight
        {
            get
            {
                Intarray img = CharsegIntarray;
                if (img != null)
                    return img.Dim(1);
                else
                    return 0;
            }
        }

        public int CharsegImageWidth
        {
            get
            {
                Intarray img = CharsegIntarray;
                if (img != null)
                    return img.Dim(0);
                else
                    return 0;
            }
        }

        public Intarray CharsegIntarray
        {
            get
            {
                if (_LineSource == null)
                    return null;
                if (_charsegIntarray != null)
                    return _charsegIntarray;
                Intarray charseg = new Intarray();
                if (_LineSource.GetCharSegmentation(charseg, _Bookno, _Pageno, _Lineno))
                    return charseg;
                else
                    return null;
            }
            set
            {
                _charsegIntarray = value;
                //_charsegImage = ImgRoutine.NarrayToRgbBitmap(_charsegIntarray);
                //if (value != null)
                {
                    //OnPropertyChanged("HaveCharseg");
                    OnPropertyChanged("CharsegIntarray");
                    OnPropertyChanged("HaveCharseg");
                    OnPropertyChanged("CharsegImageHeight");
                    OnPropertyChanged("CharsegImageWidth");
                }
            }
        }

        public bool HaveImage
        {
            get
            {
                if (_LineSource == null)
                    return false;
                if (_image != null)
                    return true;
                return _LineSource.HaveImage(_Bookno, _Pageno, _Lineno);
            }
        }

        public bool HaveCharseg
        {
            get
            {
                if (_LineSource == null)
                    return false;
                if (_charsegIntarray != null)
                    return true;
                return _LineSource.HaveCharSegmentation(_Bookno, _Pageno, _Lineno);
            }
        }

        public bool HaveTranscript
        {
            get
            {
                if (_LineSource == null)
                    return false;
                if (_transcript != null)
                    return true;
                return _LineSource.HaveTranscript(_Bookno, _Pageno, _Lineno);
            }
        }

        #endregion

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        public void SetLineSource(LineSource lineSource)
        {
            _LineSource = lineSource;
        }

        public override string ToString()
        {
            return String.Format("B{0}, P{1}, L{2}", _Bookno, _Pageno, _Lineno);
        }

    }
}
