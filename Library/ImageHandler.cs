using ImageMagick;
using System.Drawing;
using System.IO;

namespace Thumbnail_Generator_Library
{
    class ImageHandler
    {
        private static readonly int setContentWidth = 224;
        private static readonly int setContentHeight = 75;
        private static readonly int setContentHeightShort = 115;

        public static void GenerateThumbnail(string[] fileArray, string filePath, bool shortCover = false)
        {
            using MagickImage bgImage = new(BitmapToArray(Properties.Resources.Background));

            int xOffset = 1;
            int yOffset = -20;

            MagickImage contents;
            MagickImage fgImage;

            if (shortCover)
            {
                yOffset = 0;
                contents = new(CompositeThumbnail(fileArray, setContentWidth, setContentHeightShort));
                fgImage = new(BitmapToArray(Properties.Resources.Foreground_Short));
                contents.Crop(setContentWidth, setContentHeightShort);
            } else
            {
                contents = new(CompositeThumbnail(fileArray, setContentWidth, setContentHeight));
                fgImage = new(BitmapToArray(Properties.Resources.Foreground));
                contents.Crop(setContentWidth, setContentHeight);
            }
            
            bgImage.Composite(contents, Gravity.Center, xOffset, yOffset, CompositeOperator.Over);
            bgImage.Composite(fgImage, Gravity.Center, CompositeOperator.Over);
            ExportImage(bgImage.ToByteArray(), filePath);

            contents.Dispose();
            fgImage.Dispose();
        }

        private static byte[] CompositeThumbnail(string[] fileArray, int contentWidth, int contentHeight)
        {
            using MagickImageCollection magickCollection = new();

            bool isFirst = true;
            foreach (string filePath in fileArray)
            {
                using Bitmap thumb = WindowsThumbnailProvider.GetThumbnail(
                    filePath, 1024, 1024, ThumbnailOptions.None);
                using MagickImage thumbModified = new(BitmapToArray(thumb));

                int calculatedHeight = thumbModified.Height * (contentWidth / thumbModified.Height);
                thumbModified.Scale(contentWidth, calculatedHeight);
                thumbModified.Extent(0, thumbModified.Height / 8, contentWidth, contentHeight / fileArray.Length);
                thumbModified.RePage();

                MagickImage compositeImage = new(ApplyGradient(thumbModified.ToByteArray()));

                if (isFirst)
                {
                    MagickImage roundedImage = new(RoundEdges(compositeImage.ToByteArray()));
                    magickCollection.Add(roundedImage);
                    isFirst = false;
                }
                else
                {
                    magickCollection.Add(compositeImage);
                }

                thumb.Dispose();
            }

            byte[] resultBytes = magickCollection.AppendVertically().ToByteArray();

            return resultBytes;
        }

        private static byte[] RoundEdges(byte[] sourceBytes, int radius = 10)
        {
            using MagickImage sourceImage = new MagickImage(sourceBytes);

            MagickImage maskImage = new(MagickColors.White, sourceImage.Width, sourceImage.Height);
            _ = new Drawables()
                .FillColor(MagickColors.Black)
                .StrokeColor(MagickColors.Black)
                .Polygon(new PointD(0, 0), new PointD(0, radius), new PointD(radius, 0))
                .Polygon(new PointD(maskImage.Width, 0), new PointD(maskImage.Width, radius), new PointD(maskImage.Width - radius, 0))
                .FillColor(MagickColors.White)
                .StrokeColor(MagickColors.White)
                .Circle(radius, radius, radius, 0)
                .Circle(maskImage.Width - radius, radius, maskImage.Width - radius, 0)
                .Draw(maskImage);

            // Copy Alpha
            using (IMagickImage<ushort> imageAlpha = sourceImage.Clone())
            {
                imageAlpha.Alpha(AlphaOption.Extract);
                imageAlpha.Opaque(MagickColors.White, MagickColors.None);
                maskImage.Composite(imageAlpha, CompositeOperator.Over);
            }

            maskImage.HasAlpha = false;
            sourceImage.HasAlpha = false;
            sourceImage.Composite(maskImage, CompositeOperator.CopyAlpha);

            return sourceImage.ToByteArray();
        }

        private static byte[] ApplyGradient(byte[] sourceBytes)
        {
            using MagickImage sourceImage = new(sourceBytes);

            MagickImage Gradient = new("gradient:none-black", sourceImage.Width, sourceImage.Height);
            Gradient.Alpha(AlphaOption.Set);
            Gradient.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 2);

            sourceImage.Composite(Gradient, CompositeOperator.Over);

            return sourceImage.ToByteArray();
        }

        private static byte[] BitmapToArray(Bitmap sourceBitmap)
        {
            using MemoryStream stream = new();
            sourceBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            return stream.ToArray();
        }

        private static void ExportImage(byte[] sourceImage, string filePath)
        {
            using MagickImage outputImage = new(sourceImage);
            try
            {
                outputImage.Write(filePath);
            }
            catch (MagickBlobErrorException)
            {
                return;
            }
        }
    }
}
