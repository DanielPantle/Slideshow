using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Slideshow
{
    class ImageManager
    {
        private Dictionary<string, BitmapImage> images = new Dictionary<string, BitmapImage>();


        private string preloadedImagePath;
        private BitmapImage preloadedImage;


        public BitmapImage Get(string path)
        {
            return Load(path);
        }

        private BitmapImage Load(string path)
        {
            lock (images)
            {
                if (preloadedImagePath == path && preloadedImage != null)
                {
                    return preloadedImage;
                }
                else
                {
                    BitmapImage image = new BitmapImage();
                    image = new BitmapImage();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.BeginInit();
                    image.UriSource = new Uri(path);
                    image.EndInit();
                    return RotateImage(image, path);
                }
            }
        }

        private BitmapImage RotateImage(BitmapImage image, string path)
        {

            //Bild anhand der EXIF-Daten drehen

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if ((BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None).Metadata is BitmapMetadata bitmapMetadata) && (bitmapMetadata.ContainsQuery("System.Photo.Orientation")) && (bitmapMetadata.GetQuery("System.Photo.Orientation") is ushort orientation))
                {
					Console.WriteLine("rotate: " + orientation);
                    switch (orientation)
                    {
                        case 6:
                            image.Rotation = Rotation.Rotate90;
                            break;
                        case 3:
                            image.Rotation = Rotation.Rotate180;
                            break;
                        case 8:
                            image.Rotation = Rotation.Rotate270;
                            break;
                    }
                }
            }

            return image;
        }
        
        public void Preload(params string[] paths)
        {
            foreach(string path in paths)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (s, e) =>
                {
                    preloadedImage = new BitmapImage();
                    preloadedImage.CacheOption = BitmapCacheOption.OnLoad;
                    preloadedImage.BeginInit();
                    preloadedImage.UriSource = new Uri(path);
					preloadedImage = RotateImage(preloadedImage, path);
					preloadedImage.EndInit();
                    preloadedImage.Freeze();
                    preloadedImagePath = path;
                };
                bw.RunWorkerAsync();
            }
        }

        public void Dispose(string path)
        {
            images.Remove(path);
        }
    }
}
