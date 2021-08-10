using ImageMagick;
using System.Drawing;
using System.IO;

namespace Thumbnail_Generator_Library
{
    class ImageHandler
    {
        private static readonly int SetContentWidth = 224;
        private static readonly int SetContentHeight = 75;
        private static readonly int SetContentHeightShort = 115;

        public static void GenerateThumbnail(string[] FileArray, string FilePath, bool ShortCover = false)
        {
            using MagickImage BackgroundImage = new(BitmapToArray(Properties.Resources.Background));

            int X_Offset = 1;
            int Y_Offset = -20;

            MagickImage Contents;
            MagickImage ForegroundImage;

            if (ShortCover)
            {
                Y_Offset = 0;
                Contents = new(CompositeThumbnail(FileArray, SetContentWidth, SetContentHeightShort));
                ForegroundImage = new(BitmapToArray(Properties.Resources.Foreground_Short));
                Contents.Crop(SetContentWidth, SetContentHeightShort);
            } else
            {
                Contents = new(CompositeThumbnail(FileArray, SetContentWidth, SetContentHeight));
                ForegroundImage = new(BitmapToArray(Properties.Resources.Foreground));
                Contents.Crop(SetContentWidth, SetContentHeight);
            }
            
            BackgroundImage.Composite(Contents, Gravity.Center, X_Offset, Y_Offset, CompositeOperator.Over);
            BackgroundImage.Composite(ForegroundImage, Gravity.Center, CompositeOperator.Over);
            ExportImage(BackgroundImage.ToByteArray(), FilePath);

            Contents.Dispose();
            ForegroundImage.Dispose();
        }

        private static byte[] CompositeThumbnail(string[] FileArray, int ContentWidth, int ContentHeight)
        {
            using MagickImageCollection MagickCollection = new();

            bool IsFirst = true;
            foreach (string FilePath in FileArray)
            {
                using Bitmap Thumb = WindowsThumbnailProvider.GetThumbnail(
                    FilePath, 1024, 1024, ThumbnailOptions.None);
                using MagickImage Image = new(BitmapToArray(Thumb));

                int CalculatedHeight = Image.Height * (ContentWidth / Image.Height);
                Image.Scale(ContentWidth, CalculatedHeight);
                Image.Extent(0, Image.Height / 8, ContentWidth, ContentHeight / FileArray.Length);
                Image.RePage();

                MagickImage CompositeImage = new(ApplyGradient(Image.ToByteArray()));

                if (IsFirst)
                {
                    MagickImage RoundedImage = new(RoundEdges(CompositeImage.ToByteArray()));
                    MagickCollection.Add(RoundedImage);
                    IsFirst = false;
                }
                else
                {
                    MagickCollection.Add(CompositeImage);
                }

                Thumb.Dispose();
            }

            byte[] ResultBytes = MagickCollection.AppendVertically().ToByteArray();

            return ResultBytes;
        }

        private static byte[] RoundEdges(byte[] SourceBytes, int Radius = 10)
        {
            using MagickImage SourceImage = new MagickImage(SourceBytes);

            MagickImage Mask = new(MagickColors.White, SourceImage.Width, SourceImage.Height);
            _ = new Drawables()
                .FillColor(MagickColors.Black)
                .StrokeColor(MagickColors.Black)
                .Polygon(new PointD(0, 0), new PointD(0, Radius), new PointD(Radius, 0))
                .Polygon(new PointD(Mask.Width, 0), new PointD(Mask.Width, Radius), new PointD(Mask.Width - Radius, 0))
                .FillColor(MagickColors.White)
                .StrokeColor(MagickColors.White)
                .Circle(Radius, Radius, Radius, 0)
                .Circle(Mask.Width - Radius, Radius, Mask.Width - Radius, 0)
                .Draw(Mask);

            // Copy Alpha
            using (IMagickImage<ushort> ImageAlpha = SourceImage.Clone())
            {
                ImageAlpha.Alpha(AlphaOption.Extract);
                ImageAlpha.Opaque(MagickColors.White, MagickColors.None);
                Mask.Composite(ImageAlpha, CompositeOperator.Over);
            }

            Mask.HasAlpha = false;
            SourceImage.HasAlpha = false;
            SourceImage.Composite(Mask, CompositeOperator.CopyAlpha);

            return SourceImage.ToByteArray();
        }

        private static byte[] ApplyGradient(byte[] SourceBytes)
        {
            using MagickImage SourceImage = new(SourceBytes);

            MagickImage Gradient = new("gradient:none-black", SourceImage.Width, SourceImage.Height);
            Gradient.Alpha(AlphaOption.Set);
            Gradient.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 2);

            SourceImage.Composite(Gradient, CompositeOperator.Over);

            return SourceImage.ToByteArray();
        }

        private static byte[] BitmapToArray(Bitmap SourceBitmap)
        {
            using MemoryStream Stream = new();
            SourceBitmap.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);

            return Stream.ToArray();
        }

        private static void ExportImage(byte[] SourceImage, string FilePath)
        {
            using MagickImage OutputImage = new(SourceImage);
            try
            {
                OutputImage.Write(FilePath);
            }
            catch (MagickBlobErrorException)
            {
                return;
            }
        }
    }
}
