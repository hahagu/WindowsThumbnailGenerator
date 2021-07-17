using ImageMagick;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thumbnail_Generator
{
    class ImageHandler
    {
        private static readonly int contentWidth = 224;
        private static readonly int contentHeight = 75;

        public void generateThumbnail(string[] fileArray, string filePath)
        {
            MagickImage bgImage = new MagickImage(bitmapToArray(Properties.Resources.Background));
            MagickImage contents = new MagickImage(compositeThumbnail(fileArray));
            MagickImage fgImage = new MagickImage(bitmapToArray(Properties.Resources.Foreground));

            contents.Crop(contentWidth, contentHeight);

            bgImage.Composite(contents, Gravity.Center, 1, -20, CompositeOperator.Over);
            bgImage.Composite(fgImage, Gravity.Center, CompositeOperator.Over);
            exportImage(bgImage.ToByteArray(), filePath);
        }

        private byte[] compositeThumbnail(string[] fileArray)
        {
            using MagickImageCollection magickCollection = new();

            bool isFirst = true;
            foreach (string filePath in fileArray)
            {
                Bitmap thumb = extractThumbnail(filePath, new Size(1024, 1024), SIIGBF.SIIGBF_RESIZETOFIT);
                MagickImage image = new(bitmapToArray(thumb));

                int calcHeight = image.Height * (contentWidth / image.Height);
                image.Scale(contentWidth, calcHeight);
                image.Extent(0, image.Height / 8, contentWidth, contentHeight / fileArray.Length);
                image.RePage();

                MagickImage composite = new(applyGradient(image.ToByteArray()));

                if (isFirst)
                {
                    MagickImage roundedImage = new(roundEdges(composite.ToByteArray()));
                    magickCollection.Add(roundedImage);
                    isFirst = false;
                }
                else
                {
                    magickCollection.Add(composite);
                }
            }

            byte[] result_bytes = magickCollection.AppendVertically().ToByteArray();

            return result_bytes;
        }

        private byte[] roundEdges(byte[] srcBytes, int radius = 10)
        {
            using MagickImage srcImage = new MagickImage(srcBytes);

            MagickImage mask = new MagickImage(MagickColors.White, srcImage.Width, srcImage.Height);
            _ = new Drawables()
                .FillColor(MagickColors.Black)
                .StrokeColor(MagickColors.Black)
                .Polygon(new PointD(0, 0), new PointD(0, radius), new PointD(radius, 0))
                .Polygon(new PointD(mask.Width, 0), new PointD(mask.Width, radius), new PointD(mask.Width - radius, 0))
                //.Polygon(new PointD(0, mask.Height), new PointD(0, mask.Height - radius), new PointD(radius, mask.Height))
                //.Polygon(new PointD(mask.Width, mask.Height), new PointD(mask.Width, mask.Height - radius), new PointD(mask.Width - radius, mask.Height))
                .FillColor(MagickColors.White)
                .StrokeColor(MagickColors.White)
                .Circle(radius, radius, radius, 0)
                .Circle(mask.Width - radius, radius, mask.Width - radius, 0)
                //.Circle(radius, mask.Height - radius, 0, mask.Height - radius)
                //.Circle(mask.Width - radius, mask.Height - radius, mask.Width - radius, mask.Height)
                .Draw(mask);

            // Copy Alpha
            using (IMagickImage<ushort> imageAlpha = srcImage.Clone())
            {
                imageAlpha.Alpha(AlphaOption.Extract);
                imageAlpha.Opaque(MagickColors.White, MagickColors.None);
                mask.Composite(imageAlpha, CompositeOperator.Over);
            }

            mask.HasAlpha = false;
            srcImage.HasAlpha = false;
            srcImage.Composite(mask, CompositeOperator.CopyAlpha);

            return srcImage.ToByteArray();
        }

        private byte[] applyGradient(byte[] srcBytes)
        {
            using MagickImage srcImage = new MagickImage(srcBytes);

            MagickImage gradient = new("gradient:none-black", srcImage.Width, srcImage.Height);
            gradient.Alpha(AlphaOption.Set);
            gradient.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 2);

            srcImage.Composite(gradient, CompositeOperator.Over);

            return srcImage.ToByteArray();
        }

        private byte[] bitmapToArray(Bitmap src)
        {
            using MemoryStream stream = new();
            src.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        private void exportImage(byte[] srcImage, string filePath)
        {
            using MagickImage outImage = new(srcImage);
            try
            {
                outImage.Write(filePath);
            }
            catch (MagickBlobErrorException e)
            {
                MessageBox.Show("Error writing thumb.ico! Please check if the file is being used by another application!", "Write Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static Bitmap extractThumbnail(string filePath, Size size, SIIGBF flags)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            // TODO: you might want to cache the factory for different types of files
            // as this simple call may trigger some heavy-load underground operations
            IShellItemImageFactory factory;
            int hr = SHCreateItemFromParsingName(filePath, IntPtr.Zero, typeof(IShellItemImageFactory).GUID, out factory);
            if (hr != 0)
                throw new Win32Exception(hr);

            IntPtr bmp;
            hr = factory.GetImage(size, flags, out bmp);
            if (hr != 0)
                throw new Win32Exception(hr);

            return Image.FromHbitmap(bmp);
        }

        [Flags]
        public enum SIIGBF
        {
            SIIGBF_RESIZETOFIT = 0x00000000,
            SIIGBF_BIGGERSIZEOK = 0x00000001,
            SIIGBF_MEMORYONLY = 0x00000002,
            SIIGBF_ICONONLY = 0x00000004,
            SIIGBF_THUMBNAILONLY = 0x00000008,
            SIIGBF_INCACHEONLY = 0x00000010,
            SIIGBF_CROPTOSQUARE = 0x00000020,
            SIIGBF_WIDETHUMBNAILS = 0x00000040,
            SIIGBF_ICONBACKGROUND = 0x00000080,
            SIIGBF_SCALEUP = 0x00000100,
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(string path, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItemImageFactory factory);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(Size size, SIIGBF flags, out IntPtr phbm);
        }
    }
}
