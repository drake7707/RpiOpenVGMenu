using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace VGMenu
{
    static class Extensions
    {
        public static VGMenu.ShapeController.COLOR ToShapeColor(this Color color)
        {
            return new ShapeController.COLOR()
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }

        public static IEnumerable<FileInfo> GetAllFilesInDirectory(this DirectoryInfo dir)
        {
            foreach (var f in dir.GetFiles())
                yield return f;

            foreach (var d in dir.GetDirectories())
            {
                foreach (var subf in GetAllFilesInDirectory(d))
                    yield return subf;
            }
        }
    }
}
