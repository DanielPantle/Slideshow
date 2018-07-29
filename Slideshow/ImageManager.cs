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
        private Dictionary<string, Image> images = new Dictionary<string, Image>();

        public BitmapImage Get(string path)
        {
            return Load(path);
        }

        private BitmapImage Load(string path)
        {
            lock (images)
            {
                if(!images.ContainsKey(path) && File.Exists(path))
                {
                }
                //return RotateImage(images[path]);
                //return new BitmapImage(new Uri(path));
                return RotateImage(path);
            }
        }

        private BitmapImage RotateImage(string path)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);

            string _orientationQuery = "System.Photo.Orientation";
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                BitmapMetadata bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;
                
                if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(_orientationQuery)))
                {
                    object orientation = bitmapMetadata.GetQuery(_orientationQuery);

                    if (orientation != null)
                    {
                        switch ((ushort) orientation)
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
            }

            image.EndInit();
            return image;
        }
        
        public void Preload(params string[] paths)
        {
            foreach(string path in paths)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (s, e) => { Load(path); };
                bw.RunWorkerAsync();
            }
        }

        public void Dispose(string path)
        {
            images.Remove(path);
        }
    }
}
