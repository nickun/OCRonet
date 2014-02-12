using System;
using System.Collections.Generic;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    enum MAGIC_BYTES{
        //8 - unsigned byte, 03 - 3 dimensions
        MAGIC_BYTES_IMAGES   = 0x0803,
        MAGIC_BYTES_LABELS   = 0x0801,
        // standard lush magic numbers
        MAGIC_BYTE_MATRIX    = 0x1e3d4c55,
        MAGIC_SHORT_MATRIX   = 0x1e3d4c56,
        MAGIC_INTEGER_MATRIX = 0x1e3d4c54,
        // pascal vincent's magic numbers
        MAGIC_UBYTE_VINCENT  = 0x0800,
        MAGIC_SHORT_VINCENT  = 0x0B00,
        MAGIC_INT_VINCENT    = 0x0C00
    };

    public class MnistDatasource
    {
        public static string MnistPostfixImages = "-images-idx3-ubyte";
        public static string MnistPostfixImagesShort = "-idx3-ubyte";
        public static string MnistPostfixLabels = "-labels-idx1-ubyte";
        public static string MnistPostfixLabelsShort = "-idx1-ubyte";
        int _count;
        int _height;
        int _width;
        readonly List<byte[]> _imagesData;
        List<byte> _labels;

        public List<byte[]> ImagesData
        {
            get { return _imagesData; }
        }
        
        public List<byte> Labels
        {
            get { return _labels; }
        }

        public int ImgHeight
        {
            get { return _height; }
            set { _height = value; }
        }

        public int ImgWidth
        {
            get { return _width; }
            set { _width = value; }
        }

        #region constructors
        public MnistDatasource()
        {
            _count = 0;
            _imagesData = new List<byte[]>();
            _labels = new List<byte>();
        }
        public MnistDatasource(int height, int width)
            : this()
        {
            _height = height;
            _width = width;
        }
        #endregion

        public virtual int NSamples()
        {
            return _count;
        }

        public void AddImage(byte[] imgData, byte label)
        {
            _imagesData.Add(imgData);
            _labels.Add(label);
            _count++;
        }

        public static bool CheckForMnistImagesFileName(string fileName)
        {
            // проверка на наличие постфикса в имени файла
            int idx1 = fileName.LastIndexOf(MnistPostfixImages);
            bool isMnistImageFile = (idx1 >= 0 && (idx1 == (fileName.Length - MnistPostfixImages.Length)));
            if (!isMnistImageFile)
            {
                idx1 = fileName.LastIndexOf(MnistPostfixImagesShort);
                isMnistImageFile = (idx1 >= 0 && (idx1 == (fileName.Length - MnistPostfixImagesShort.Length)));
            }
            return isMnistImageFile;
        }
        public static bool CheckForMnistLabelsFileName(string fileName)
        {
            int idx1 = fileName.LastIndexOf(MnistPostfixLabels);
            // проверка на наличие постфикса в имени файла
            return (idx1 >= 0 && (idx1 == (fileName.Length - MnistPostfixLabels.Length)));
        }

        public static string GetImagesFullFileName(string prefix)
        {
            // проверка на наличие постфикса в имени файла
            int idx1 = prefix.LastIndexOf(MnistPostfixImages);
            bool isMnistImageFile = (idx1 >= 0 && (idx1 == (prefix.Length - MnistPostfixImages.Length)));
            if (isMnistImageFile)
                return prefix;
            // проверим короткое именование
            idx1 = prefix.LastIndexOf(MnistPostfixImagesShort);
            isMnistImageFile = (idx1 >= 0 && (idx1 == (prefix.Length - MnistPostfixImagesShort.Length)));
            if (isMnistImageFile)
                return prefix.Replace(MnistPostfixImagesShort, "") + MnistPostfixImages;
            return prefix + MnistPostfixImages;
        }

        public static string GetLabelsFullFileName(string prefix)
        {
            // уберем из имени файла постфикс IMAGES если он есть
            if (CheckForMnistImagesFileName(prefix))
                return (prefix.Replace(MnistPostfixImages, "").Replace(MnistPostfixImagesShort, "") + MnistPostfixLabels);

            int idx1 = prefix.LastIndexOf(MnistPostfixLabels);
            // проверка на наличие постфикса в имени файла
            if (idx1 >= 0 && (idx1 == (prefix.Length - MnistPostfixLabels.Length)))
                return prefix;
            return prefix + MnistPostfixLabels;
        }

        #region reading

        private static byte[] TransposeBytes(byte[] bytes)
        {
            byte[] result = new byte[bytes.Length];
            int iPos = bytes.Length - 1;
            for (int i = 0; i < bytes.Length; i++)
                result[i] = bytes[iPos--];
            return result;
        }

        public static int SwapInt32(int v)
        {
            return (int)(((SwapInt16((short)v) & 0xffff) << 0x10) |
                          (SwapInt16((short)(v >> 0x10)) & 0xffff));
        }

        public static short SwapInt16(short v)
        {
            return (short)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        private void ReadHeaders(BinaryReader imagesReader, BinaryReader labelsReader)
        {
            int ndimMinImg = 3; // std header requires at least 3 dims even empty ones.
            int ndimMinLbl = 3;
            // read magic number for Images
            byte[] rbytes = imagesReader.ReadBytes(4);
            int magicImg = BitConverter.ToInt32(rbytes, 0);
            int magicVincentImg = SwapInt32(magicImg);
            int ndimImg = magicVincentImg & 0xF;
            magicVincentImg &= ~0xF;
            if (magicImg != (int)MAGIC_BYTES.MAGIC_BYTE_MATRIX && magicVincentImg != (int)MAGIC_BYTES.MAGIC_UBYTE_VINCENT)
                throw new InvalidDataException("invalid magic bytes in the images file");

            // standard header
            if (magicImg == (int)MAGIC_BYTES.MAGIC_BYTE_MATRIX)
            {
                // read number of dimensions
                ndimImg = BitConverter.ToInt32(imagesReader.ReadBytes(4), 0);
            }
            else
            {
                ndimMinImg = ndimImg;
            }
            if (ndimImg != 3)
                throw new InvalidDataException("expected order of 3 but found " + ndimImg + " in dat file.");

            // read magic number for Labels
            rbytes = labelsReader.ReadBytes(4);
            int magicLbl = BitConverter.ToInt32(rbytes, 0);
            int magicVincentLbl = SwapInt32(magicLbl);
            int ndimLbl = magicVincentLbl & 0xF;
            magicVincentLbl &= ~0xF;
            if (magicLbl != (int)MAGIC_BYTES.MAGIC_BYTE_MATRIX && magicVincentLbl != (int)MAGIC_BYTES.MAGIC_UBYTE_VINCENT)
                throw new InvalidDataException("invalid magic bytes in the labels file");
            // standard header
            if (magicLbl == (int)MAGIC_BYTES.MAGIC_BYTE_MATRIX)
            {
                // read number of dimensions
                ndimLbl = BitConverter.ToInt32(labelsReader.ReadBytes(4), 0);
            }
            else
            {
                ndimMinLbl = ndimLbl;
            }
            if (ndimLbl != 1)
                throw new InvalidDataException("expected order of 1 but found " + ndimLbl + " in label file.");

            // header: read each dimension
            _count = BitConverter.ToInt32(imagesReader.ReadBytes(4), 0);
            int countLbl = BitConverter.ToInt32(labelsReader.ReadBytes(4), 0);
            if (_count != countLbl)
                throw new InvalidDataException("the number of images and labels mismatch");
            if (magicVincentImg == (int)MAGIC_BYTES.MAGIC_UBYTE_VINCENT)
            {
                _count = SwapInt32(_count);
                //countLbl = Endian.SwapInt32(countLbl);
            }

            _height = BitConverter.ToInt32(imagesReader.ReadBytes(4), 0);
            _width = BitConverter.ToInt32(imagesReader.ReadBytes(4), 0);
            if (magicLbl == (int)MAGIC_BYTES.MAGIC_BYTE_MATRIX)
            {
                // read reserved to 3 dimension
                labelsReader.ReadBytes(4);
                labelsReader.ReadBytes(4);
            }
            else if (magicVincentImg == (int)MAGIC_BYTES.MAGIC_UBYTE_VINCENT)
            {
                _height = SwapInt32(_height);
                _width = SwapInt32(_width);
            }
            
        }

        private void ReadData(BinaryReader imagesReader, BinaryReader labelsReader)
        {
            int index = 0;
            while (index < _count)
            {
                //read pixels of (index) image
                try
                {
                    byte[] imgData = imagesReader.ReadBytes(_height * _width);
                    _imagesData.Add(imgData);
                    index++;
                }
                catch {
                    break;
                }
            }
            //read labels
            _labels = new List<byte>(labelsReader.ReadBytes(_count));
        }

        public void LoadFromFile(string prefix)
        {
            // open images
            Stream inputImg = new FileStream(GetImagesFullFileName(prefix), FileMode.Open, FileAccess.Read, FileShare.Read);
            // open labels
            Stream inputLbl = new FileStream(GetLabelsFullFileName(prefix), FileMode.Open, FileAccess.Read, FileShare.Read);

            BinaryReader imagesReader = new BinaryReader(inputImg);
            BinaryReader labelsReader = new BinaryReader(inputLbl);
            try
            {
                ReadHeaders(imagesReader, labelsReader);
                ReadData(imagesReader, labelsReader);
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException("Load Mnist failed: " + ex.Message);
            }
            finally
            {
                inputImg.Close();
                inputLbl.Close();
            }
        }

        #endregion

        #region writing

        private void WriteHeaders(BinaryWriter imagesWriter, BinaryWriter labelsWriter)
        {
            imagesWriter.Write(TransposeBytes(BitConverter.GetBytes((Int32)MAGIC_BYTES.MAGIC_BYTES_IMAGES)));
            labelsWriter.Write(TransposeBytes(BitConverter.GetBytes((Int32)MAGIC_BYTES.MAGIC_BYTES_LABELS)));
            imagesWriter.Write(TransposeBytes(BitConverter.GetBytes(_imagesData.Count))); // number of samples
            labelsWriter.Write(TransposeBytes(BitConverter.GetBytes(_labels.Count)));     // number of samples
            imagesWriter.Write(TransposeBytes(BitConverter.GetBytes(_height))); // dimensions 1
            imagesWriter.Write(TransposeBytes(BitConverter.GetBytes(_width)));  // dimensions 2
        }

        private void WriteData(BinaryWriter imagesWriter, BinaryWriter labelsWriter)
        {
            foreach (byte[] imgData in _imagesData)
                imagesWriter.Write(imgData);
            labelsWriter.Write(_labels.ToArray());
        }


        public void SaveToFile(string prefix)
        {
            FileMode mode = FileMode.Create;

            if (File.Exists(GetImagesFullFileName(prefix)))
                mode = FileMode.Truncate;
            // open images for write
            Stream outputImg = new FileStream(GetImagesFullFileName(prefix), mode, FileAccess.Write, FileShare.None);

            mode = FileMode.Create;
            if (File.Exists(GetLabelsFullFileName(prefix)))
                mode = FileMode.Truncate;
            // open labels for write
            Stream outputLbl = new FileStream(GetLabelsFullFileName(prefix), mode, FileAccess.Write, FileShare.None);

            BinaryWriter imagesWriter = new BinaryWriter(outputImg);
            BinaryWriter labelsWriter = new BinaryWriter(outputLbl);
            try
            {
                WriteHeaders(imagesWriter, labelsWriter);
                WriteData(imagesWriter, labelsWriter);
            }
            finally
            {
                imagesWriter.Flush();
                labelsWriter.Flush();
                outputImg.Flush();
                outputImg.Close();
                outputLbl.Flush();
                outputLbl.Close();
            }
        }

        #endregion
    }

}
