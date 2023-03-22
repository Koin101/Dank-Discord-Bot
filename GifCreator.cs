using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimatedGif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
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
            var imgToGif = RemovePixels(img);
            imgToGif.SaveAsGif(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
        }

        public static Image<Rgba32> ResizeImage(Image<Rgba32> image, int width, int height)
        {
            image.Mutate(x => { x.Resize(width, height, KnownResamplers.NearestNeighbor); });

            return image;
        }

        public static Image<Rgba32> RemovePixels( Image<Rgba32> background)
        {
            var width = background.Width;
            var height = background.Height;
            Image<Rgba32> overlay = Image.Load<Rgba32>("Images/speechbubble.png");

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

        public static ImageFrame<Rgba32> RemovePixels(ImageFrame<Rgba32> background)
        {
            var width = background.Width;
            var height = background.Height;
            Image<Rgba32> overlay = Image.Load<Rgba32>("Images/speechbubble.png");

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

        public void AnimatedGifCreator(ImageFrameCollection<Rgba32> frames, Image<Rgba32> originalGif)
        {
            try
            {
                var width = frames[0].Width;
                var height = frames[0].Height;
                using Image<Rgba32> newGif = new Image<Rgba32>(width, height);
                var originalGifMetaData = originalGif.Metadata.GetGifMetadata();
                var gifMetaData = newGif.Metadata.GetGifMetadata();

                gifMetaData.RepeatCount = originalGifMetaData.RepeatCount;
                var oldMetaData = originalGif.Frames.RootFrame.Metadata.GetGifMetadata();
                GifFrameMetadata metadata = newGif.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = oldMetaData.FrameDelay;
                for (int i = 0; i < frames.Count; i++)
                {

                    using ImageFrame<Rgba32> newFrame = RemovePixels(frames[i]);
                    Console.WriteLine("frame number:" + i + "----" + newFrame[100, 120].ToHex());
                    newGif.Frames.AddFrame(newFrame);
                }

                newGif.Frames.RemoveFrame(0);
                newGif.SaveAsGif(gifStream);
                gifStream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("--------------------");
                throw;
            }
           

        }


    }
}
