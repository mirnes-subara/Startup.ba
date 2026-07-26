using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Startupba.Services.Helpers
{
    public class ImageConversion
    {
        public static byte[] HexToByteArray(string hex)
        {
            hex = hex.Replace("0x", "");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }


        public static string ConvertImageToBase64String(string folder, string imageName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string imagePath = Path.Combine(currentDirectory, folder, imageName);




            try
            {
                if (File.Exists(imagePath))
                {
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    return Convert.ToBase64String(imageBytes);
                }
                else
                {
                    Console.WriteLine("Image file not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file: {ex.Message}");
                return null;
            }
        }



        public static byte[] ConvertImageToByteArray(string folder, string imageName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string imagePath = Path.Combine(currentDirectory, folder, imageName);


            try
            {
                if (File.Exists(imagePath))
                {
                    return File.ReadAllBytes(imagePath);
                }
                else
                {
                    Console.WriteLine($"Image file not found: {imageName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reads an image from the Assets folder, trying each name in order.
        /// Returns an empty byte array if none of the files exist, so seeding /
        /// migration generation never fails because of a missing picture.
        /// </summary>
        public static byte[] ConvertImageToByteArrayWithFallback(string folder, params string[] imageNames)
        {
            foreach (var name in imageNames)
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), folder, name);
                try
                {
                    if (File.Exists(imagePath))
                    {
                        return File.ReadAllBytes(imagePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading image file '{name}': {ex.Message}");
                }
            }

            Console.WriteLine($"None of the image files found: {string.Join(", ", imageNames)}. Seeding empty image.");
            return Array.Empty<byte>();
        }
    }
}
