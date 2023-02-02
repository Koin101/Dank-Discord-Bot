using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using AnimatedGif;

namespace Discord_Bot
{
    internal class GifCreator
    {
        public int count = 0;
        public bool del = true;
        public MemoryStream memStream = new MemoryStream();
        public MemoryStream gifStream = new MemoryStream();
        static void Main(string[] args)
        {

        }

        public void CreateGifFromImg(Bitmap img)
        {
            Bitmap overlay = new Bitmap(Image.FromFile("Images/speechbubble.png"));
            var imgToGif = RemovePixels(overlay, img);
            imgToGif.SaveAsGif(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height / 3);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap RemovePixels(Bitmap overlay, Bitmap background)
        {
            var resizedOverlay = ResizeImage(overlay, background.Width, background.Height);

            for (int i = 0; i < resizedOverlay.Width; i++)
            {
                for (int j = 0; j < resizedOverlay.Height; j++)
                {
                    Color c = resizedOverlay.GetPixel(i, j);

                    if (c.Name == "ff04f404") background.SetPixel(i, j, Color.Transparent);
                }
            }

            return background;
        }

        public void AnimatedGifCreator(Image[] frames)
        {
            Bitmap overlay = new Bitmap(Image.FromFile("Images/speechbubble.png"));
            var width = frames[0].Width;
            var height = frames[0].Height;
            var resizedOverlay = ResizeImage(overlay, width, height);
            Image[] editedframes = new Image[frames.Length];
            var creator = new AnimatedGifCreator(gifStream);
            int i = 0;
            foreach (var frame in frames)
            {
                Bitmap frameBitmap = new(frame);
                var editedFrame = RemovePixels(overlay, frameBitmap);

                editedframes[i] = editedFrame;

                i++;
            }

            editedframes.SaveAsAnimatedGif(gifStream);
            gifStream.Seek(0, SeekOrigin.Begin);

        }

        public Image[] getFrames(Image originalImg)
        {
            FrameDimension dimension = new(originalImg.FrameDimensionsList[0]);
            int numberOfFrames = originalImg.GetFrameCount(dimension);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(dimension, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            return frames;
        }
    }
}
