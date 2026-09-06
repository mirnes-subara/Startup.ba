using Startupba.Model;

namespace Startupba.Services.Helpers
{
    /// <summary>
    /// Validates image bytes by file signature (magic bytes), not by client MIME or extension.
    /// JPEG: FF D8 FF. PNG: 89 50 4E 47 0D 0A 1A 0A.
    /// </summary>
    public static class ImageMagicBytes
    {
        private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static bool IsJpeg(byte[] data)
            => StartsWith(data, JpegSignature);

        public static bool IsPng(byte[] data)
            => StartsWith(data, PngSignature);

        public static bool IsJpegOrPng(byte[] data)
            => IsJpeg(data) || IsPng(data);

        public static string ContentType(byte[] data)
            => IsPng(data) ? "image/png" : "image/jpeg";

        public static void EnsureJpegOrPng(byte[]? data, string message = "Only JPEG and PNG images are allowed.")
        {
            if (data == null || data.Length == 0 || !IsJpegOrPng(data))
                throw new UserException(message);
        }

        private static bool StartsWith(byte[] data, byte[] signature)
        {
            if (data == null || data.Length < signature.Length)
                return false;

            for (var i = 0; i < signature.Length; i++)
            {
                if (data[i] != signature[i])
                    return false;
            }

            return true;
        }
    }
}
