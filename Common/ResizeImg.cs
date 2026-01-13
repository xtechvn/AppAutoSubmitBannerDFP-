using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using System.Text;
using System.Linq;
//using dotless.Core.Abstractions;

namespace POLY.ResizeImg
{
    public class Resize_Img
    {
        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="imageFile">the byte array of the file</param>
        /// <param name="targetSize">the target size of the file (may affect width or height) 
        /// depends on orientation of file (landscape or portrait)</param>
        /// <returns>Byte array containing the resized file</returns>
        public static byte[] ResizeImageFile(byte[] imageFile, int targetSize,int Height)
        {
            using (System.Drawing.Image oldImage =
                System.Drawing.Image.FromStream(new MemoryStream(imageFile)))
            {
                //  Size newSize = CalculateDimensions(oldImage.Size, targetSize);
                
                int Width = targetSize;
                System.Drawing.Image newImage = ScaleImage(oldImage, Width, Height);

                var m = new MemoryStream();
                string imgFormat = GetImageFormat(imageFile).ToString().ToLower();
                if (imgFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg.ToString().ToLower()))
                {                   
                    newImage.Save(m, ImageFormat.Png); // Neu la duoi Jpg thi se chuyen thanh Png                        
                }
                else if (imgFormat.Equals(System.Drawing.Imaging.ImageFormat.Png.ToString().ToLower()))
                {
                    newImage.Save(m, ImageFormat.Png);
                }
                else if (imgFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif.ToString().ToLower()))
                {
                    newImage.Save(m, ImageFormat.Gif);
                }
                else
                {
                    newImage.Save(m, ImageFormat.Png); // Ngoai 3 dinh dang nay thi cho jpg
                }

                return m.ToArray();
            }
        }

        /// <summary>
        /// Calculates the new size of the image based on the target size
        /// </summary>
        /// <param name="oldSize">Is the size of the original file</param>
        /// <param name="targetSize">Is the target size of the resized file</param>
        /// <returns>The new size</returns>
        public static Size CalculateDimensions(Size oldSize, int targetSize)
        {
            Size newSize = new Size();
            if (oldSize.Height > oldSize.Width)
            {
                newSize.Width =
                    (int)(oldSize.Width * ((float)targetSize / (float)oldSize.Height));
                newSize.Height = targetSize;
            }
            else
            {
                newSize.Width = targetSize;
                newSize.Height =
                    (int)(oldSize.Height * ((float)targetSize / (float)oldSize.Width));
            }
            return newSize;
        }

        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.Bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.Gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.Png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.Tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.Tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.Jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.Jpeg;

            return null;
        }

        public static System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxWidth, int maxHeight)
        {            
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).SmoothingMode = SmoothingMode.AntiAlias;
            Graphics.FromImage(newImage).InterpolationMode = InterpolationMode.HighQualityBicubic;
            
            Graphics.FromImage(newImage).PixelOffsetMode = PixelOffsetMode.HighQuality;
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            newImage.SetResolution(300, 300);

            return newImage;
        }     

    }
}
