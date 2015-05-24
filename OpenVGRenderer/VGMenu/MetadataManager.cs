using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;

namespace VGMenu
{
    public abstract class MetadataManager
    {


        public void SaveImage(Bitmap bm, string path)
        {
            if (ConfigurationManager.AppSettings["useRaw"] == "true")
                SaveToRaw(bm, path);
            else
                SaveToRaw(bm, path);
        }

        private void SaveToJpeg(Bitmap bm, string path)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }

            EncoderParameters ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

            using (MemoryStream ms = new MemoryStream())
            {
                bm.Save(ms, ici, ep);
                ms.Position = 0;
                System.IO.File.WriteAllBytes(path, ms.ToArray());
            }
        }

        private void SaveToRaw(Bitmap bmp, string path)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            using (var fs = File.OpenWrite(path))
            {
                fs.Write(BitConverter.GetBytes(w), 0, 4);
                fs.Write(BitConverter.GetBytes(h), 0, 4);

                for (int j = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        var color = bmp.GetPixel(i, (h - 1) - j);
                        fs.WriteByte(color.R);
                        fs.WriteByte(color.G);
                        fs.WriteByte(color.B);
                        fs.WriteByte(color.A);
                    }
                }
                fs.Flush();
                fs.Close();
            }
        }

        public string GetCleanKeywords(string name)
        {
            name = name.Replace('.', ' ');

            string[] parts = name.Split(' ');
            int yearIdx = -1;
            for (int i = 0; i < parts.Length; i++)
            {

                int val;
                if (int.TryParse(parts[i].Replace("(", "").Replace(")", ""), out val) && val >= 1900 && val < 3000)
                {
                    yearIdx = i;
                    break;
                }
            }
            if (yearIdx > 0)
                name = string.Join(" ", parts.Take(yearIdx + 1));

            if (name.Contains(")"))
                name = name.Substring(0, name.IndexOf(")") + 1);

            return name;
        }

    }
}
