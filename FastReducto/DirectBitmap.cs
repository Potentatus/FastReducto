using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FastReducto
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public DirectBitmap(Bitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    SetPixel(i, j, bitmap.GetPixel(i, j));
        }

        public void LightnessGray()
        {
            Parallel.For(0, Bits.Length, index => {
                Color c = Color.FromArgb(Bits[index]);
                int y = (Math.Max((int)c.R, Math.Max(c.G, c.B)) + Math.Min((int)c.R, Math.Min(c.G, c.B))) / 2;
                Bits[index] = Color.FromArgb(y,y,y).ToArgb();
            });
        }

        public void AverageGray()
        {
            Parallel.For(0, Bits.Length, index => {
                Color c = Color.FromArgb(Bits[index]);
                int y = (c.R + c.G + c.B) / 3;
                Bits[index] = Color.FromArgb(y, y, y).ToArgb();
            });
        }

        public void LuminosityGray(double r = 0.21, double g = 0.72, double b = 0.07)
        {
            Parallel.For(0, Bits.Length, index => {
                Color c = Color.FromArgb(Bits[index]);
                int y = (int)(r * c.R + g * c.G + b * c.B);
                Bits[index] = Color.FromArgb(y, y, y).ToArgb();
            });
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
