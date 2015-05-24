using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.IO;

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
            public bool visible;

            [MarshalAs(UnmanagedType.LPStr)]
            public string filename;

            public float x;
            public float y;
            public float width;
            public float height;
        }


        private static Form frm;
        public static void Initialize(OVERSCAN overscan)
        {
            Thread t = new Thread(() =>
            {
                frm = new Form();
                frm.Width = 960;
                frm.Height = 540;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.ShowDialog();

            });
            t.Start();

            while (frm == null || !frm.IsHandleCreated)
                System.Threading.Thread.Sleep(100);

            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Initialize");
        }



        private static string Dump(object o)
        {
            return "[" + o.GetType().FullName + "]" + Environment.NewLine + string.Join(Environment.NewLine, o.GetType().GetFields().Select(f => "   " + f.Name + " = " + f.GetValue(o)));
        }


        public static void SetRect(SHAPE_RECT r)
        {
            frm.BeginInvoke(new Action(() =>
            {
                int oldIdx = -1;
                var oldC = frm.Controls.Cast<Control>().Where(c => (int)c.Tag == r.id).FirstOrDefault();
                if (oldC != null)
                {
                    oldIdx = frm.Controls.IndexOf(oldC);
                    frm.Controls.Remove(oldC);

                }
                Panel p = new Panel();
                p.Tag = r.id;
                p.Location = new System.Drawing.Point((int)(r.x * frm.Width), (int)(r.y * frm.Height));
                p.Size = new System.Drawing.Size((int)(r.width * frm.Width), (int)(r.height * frm.Height));
                p.BackColor = Color.FromArgb(r.backColor.R, r.backColor.G, r.backColor.B);

                p.Visible = r.visible;

                frm.Controls.Add(p);
                //p.BringToFront();
                if (oldC != null)
                    frm.Controls.SetChildIndex(p, oldIdx);
                else
                    p.BringToFront();
            }));

            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Set " + Dump(r));

        }

        public static void SetText(SHAPE_TEXT r)
        {
            frm.BeginInvoke(new Action(() =>
            {
                int oldIdx = -1;
                var oldC = frm.Controls.Cast<Control>().Where(c => (int)c.Tag == r.id).FirstOrDefault();
                if (oldC != null)
                {
                    oldIdx = frm.Controls.IndexOf(oldC);
                    frm.Controls.Remove(oldC);

                }

                Label p = new TransparentLabel();
                p.Font = new Font("Tahoma", r.size * frm.Height, GraphicsUnit.Pixel);
                p.Tag = r.id;
                p.Location = new System.Drawing.Point((int)(r.x * frm.Width), (int)(r.y * frm.Height));
                p.Size = new Size((int)(r.width * frm.Width), (int)(r.height * frm.Height));
                p.ForeColor = Color.FromArgb(r.color.R, r.color.G, r.color.B);
                p.Text = r.text;

                p.Visible = r.visible;

                frm.Controls.Add(p);
                
                p.BackColor = Color.Transparent;
                //p.BringToFront();
                if (oldC != null)
                    frm.Controls.SetChildIndex(p, oldIdx);
                else
                    p.BringToFront();
            }));

            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Set " + Dump(r));
        }

        public class TransparentLabel : Label
        {
            public TransparentLabel()
            {
                this.SetStyle(ControlStyles.Opaque, true);
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            }
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams parms = base.CreateParams;
                    parms.ExStyle |= 0x20;  // Turn on WS_EX_TRANSPARENT
                    return parms;
                }
            }
        }

        public static void SetImage(SHAPE_IMAGE img)
        {
            frm.BeginInvoke(new Action(() =>
            {
                int oldIdx = -1;
                var oldC = frm.Controls.Cast<Control>().Where(c => (int)c.Tag == img.id).FirstOrDefault();
                if (oldC != null)
                {
                    oldIdx = frm.Controls.IndexOf(oldC);
                    frm.Controls.Remove(oldC);
                }

                Panel p = new Panel();
                p.Font = new Font("Tahoma", 20f);
                p.Tag = img.id;
                p.Location = new System.Drawing.Point((int)(img.x * frm.Width), (int)(img.y * frm.Height));
                p.Size = new System.Drawing.Size((int)(img.width * frm.Width), (int)(img.height * frm.Height));
                
                frm.Controls.Add(p);
                //p.BringToFront();
                p.BackgroundImageLayout = ImageLayout.Stretch;


                Image bmp;
                if (!imageStorage.TryGetValue(img.filename, out bmp))
                {
                    if (!img.filename.EndsWith(".raw"))
                    {
                        if (!string.IsNullOrEmpty(img.filename))
                            imageStorage[img.filename] = bmp = Image.FromFile(img.filename.Replace("/", "\\"));
                    }
                    else
                    {


                        using (var fs = File.OpenRead(img.filename.Replace("/", "\\")))
                        {
                            byte[] bytes = new byte[4];
                            fs.Read(bytes, 0, 4);
                            int w = BitConverter.ToInt32(bytes, 0);
                            fs.Read(bytes, 0, 4);
                            int h = BitConverter.ToInt32(bytes, 0);

                            bmp = new Bitmap(w, h);
                            for (int j = 0; j < h; j++)
                            {
                                for (int i = 0; i < w; i++)
                                {
                                    byte r = (byte)fs.ReadByte(); byte g = (byte)fs.ReadByte(); byte b = (byte)fs.ReadByte(); byte a = (byte)fs.ReadByte();

                                    ((Bitmap)bmp).SetPixel(i, (h - 1) - j, Color.FromArgb(a, r, g, b));

                                }
                            }
                            fs.Flush();
                            fs.Close();
                        }
                        imageStorage[img.filename] = bmp;
                    }
                }
                p.BackgroundImage = bmp;
                p.Visible = img.visible && p.BackgroundImage != null;

                if (oldC != null)
                    frm.Controls.SetChildIndex(p, oldIdx);
                else
                    p.BringToFront();
            }));

            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Set " + Dump(img));
        }

        private static Dictionary<string, Image> imageStorage = new Dictionary<string, Image>();


        public static void Clear()
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Clear");
            frm.BeginInvoke(new Action(() =>
            {
                frm.Controls.Clear();
            }));
        }

        public static void OpenContext()
        {

        }

        public static void CloseContext()
        {

        }


        public static void Remove(int id)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Remove " + id);

            frm.BeginInvoke(new Action(() =>
            {
                var ctl = frm.Controls.Cast<Control>().Where(c => (int)c.Tag == id).FirstOrDefault();
                frm.Controls.Remove(ctl);
            }));
        }


        public static void Draw()
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Draw");
        }



        public static void Destroy()
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Destroy");
        }


        private static int idCounter;
        public static int NewId()
        {
            return ++idCounter;
        }
    }
}
