using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VGMenu
{
    public class ShapeController
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct OVERSCAN
        {
            public int paddingLeft;
            public int paddingTop;
            public int paddingRight;
            public int paddingBottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COLOR
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct SHAPE_RECT
        {
            public int id;
             [MarshalAs(UnmanagedType.U1)]
            public bool visible;
            public float x;
            public float y;
            public float width;
            public float height;
            public COLOR backColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHAPE_TEXT
        {
            public int id;
             [MarshalAs(UnmanagedType.U1)]
            public bool visible;

            [MarshalAs(UnmanagedType.LPStr)]
            public string text;

            public float x;
            public float y;

            public float width;
            public float height;

            public float size;
            public COLOR color;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHAPE_IMAGE
        {
            public int id;
             [MarshalAs(UnmanagedType.U1)]
            public bool visible;

            [MarshalAs(UnmanagedType.LPStr)]
            public string filename;

            public float x;
            public float y;
            public float width;
            public float height;
        }

        [DllImport("./openvgrenderer.lib")]
        public static extern void Initialize(OVERSCAN overscan);

        [DllImport("./openvgrenderer.lib")]
        public static extern void SetRect(SHAPE_RECT r);

        [DllImport("./openvgrenderer.lib")]
        public static extern void SetText(SHAPE_TEXT t);

        [DllImport("./openvgrenderer.lib")]
        public static extern void SetImage(SHAPE_IMAGE i);


        [DllImport("./openvgrenderer.lib")]
        public static extern void OpenContext();

        [DllImport("./openvgrenderer.lib")]
        public static extern void CloseContext();


        [DllImport("./openvgrenderer.lib")]
        public static extern void Clear();

        [DllImport("./openvgrenderer.lib")]
        public static extern void Remove(int id);

        [DllImport("./openvgrenderer.lib")]
        public static extern void Draw();

        [DllImport("./openvgrenderer.lib")]
        public static extern void Destroy();


        private static int idCounter;
        public static int NewId()
        {
            return ++idCounter;
        }
    }
}
