using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimatedGif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

        public void CreateGifFromImg(Image<Rgba32> img)
        {
            Image<Rgba32> overlay = Image.Load<Rgba32>("Images/speechbubble.png");
            var imgToGif = RemovePixels(overlay, img);
            imgToGif.SaveAsGif(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
        }

        public static Image<Rgba32> ResizeImage(Image<Rgba32> image, int width, int height)
        {
            image.Mutate(x => { x.Resize(width, height, KnownResamplers.NearestNeighbor); });

            return image;
        }

        public static Image<Rgba32> RemovePixels(Image<Rgba32> overlay, Image<Rgba32> background)
        {
            var width = background.Width;
            var height = background.Height;

            using Image<Rgba32> resizedOverlay = ResizeImage(overlay, width, height / 3);

            for (int i = 0; i < resizedOverlay.Width; i++)
            {
                for (int j = 0; j < resizedOverlay.Height; j++)
                {
                    var pixel = resizedOverlay[i, j];
                    if (pixel.ToHex() == "04F404FF")
                    {
                        background[i, j] = Color.Transparent;
                    }
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
