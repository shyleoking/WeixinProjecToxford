using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ProjecToxfordApi.Controllers
{
    public static class FileHelper
    {
        private static string photofolder = System.Configuration.ConfigurationManager.AppSettings["ProjecToxfordPhotos"];

        public static async Task<byte[]> ReadAsync(string fileName)
        {
            var filePath = Path.Combine(photofolder, fileName);
            var fs = File.OpenRead(filePath);
            int filelength = (int)fs.Length;
            var image = new Byte[filelength];
            await fs.ReadAsync(image, 0, filelength);
            fs.Close();
            return image;
        }

        public static async Task<byte[]> ReadAsync(Stream stream)
        {
            var result = new byte[(int)stream.Length];
            await stream.ReadAsync(result, 0, (int)stream.Length);
            stream.Close();
            return result;
        }

        public static void SaveFile(byte[] content, string fileName)
        {
            var filePath = Path.Combine(photofolder, fileName);

            FileStream fs = new FileStream(filePath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(content);
            bw.Close();
            fs.Close();
        }

        public static void Delete(string fileName)
        {
            var filePath = Path.Combine(photofolder, fileName);
            File.Delete(filePath);
        }

        public static void Rename(string oldfile, string newfile)
        {
            var filePath1 = Path.Combine(photofolder, oldfile);
            var filePath2 = Path.Combine(photofolder, newfile);
            File.Move(filePath1, filePath2);
        }

        public static bool Exists(string fileName)
        {
            var filePath = Path.Combine(photofolder, fileName);
            return File.Exists(filePath);
        }


    }
}