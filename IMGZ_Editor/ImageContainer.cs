using System;
using System.Drawing;
using System.IO;

namespace IMGZ_Editor
{
    public abstract class ImageContainer : IDisposable
    {
        private static nQuant.WuQuantizer _quantizer;
        private static nQuant.WuQuantizer quantizer { get { if (_quantizer == null) { _quantizer = new nQuant.WuQuantizer(); } return _quantizer; } }
        /// <summary><para>Performa a lossy quantize.</para><para>This is seperated from the main function so that when the DLL is missing, a straight quantize can still be attempted.</para></summary>
        /// <param name="input">Input <c>Bitmap</c>.</param>
        /// <param name="MaxColor">Max colros in output image.</param>
        /// <returns>New image.</returns>
        private static Bitmap lossyQuantize(Bitmap input, int MaxColor) { return quantizer.QuantizeImage(input, 12, 1, MaxColor) as Bitmap; }

        public ImageContainer() { }

        /// <summary><para>Converts an image from ARGB to either 8-bit or 4-bit indexed</para><para>Not real quantization since this only copies colors, not reduces.</para></summary>
        /// <param name="input">Input image in RGBA format</param>
        /// <param name="target">Target <c>PixelFormat</c></param>
        /// <returns>Indexed <c>Bitmap</c> is successful, null on failure.</returns>
        private static Bitmap attemptStraightQuantize(Bitmap input, System.Drawing.Imaging.PixelFormat target, out int totalColors)
        {
            int width = input.Width,
                height = input.Height,
                stride, MaxColor;
            switch (target)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    MaxColor = 256;
                    stride = width;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    MaxColor = 16;
                    stride = width / 2;
                    break;
                default: throw new ArgumentException("Unsupported PixelFormat", "target");
            }
            byte[] iBuff = new byte[stride * height];
            System.Collections.Generic.Dictionary<int, byte> colors = new System.Collections.Generic.Dictionary<int, byte>(MaxColor);
            {
                int[] buffer = new int[width];
                bool overMax = false;
                for (int i = 0, offs = 0; i < height; ++i, offs += stride)
                {
                    System.Drawing.Imaging.BitmapData data = input.LockBits(Rectangle.FromLTRB(0, i, width, i + 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    try { System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, width); }
                    finally { input.UnlockBits(data); }
                    for (int j = 0; j < width; ++j)
                    {
                        int color = buffer[j];
                        //Treat all alpha == 0 the same
                        if ((color >> 24) == 0) { color = 0; }
                        byte key;
                        if (!colors.TryGetValue(color, out key))
                        {
                            colors.Add(color, key = (byte)colors.Count);
                            if (overMax) { continue; }
                            else if (colors.Count > MaxColor)
                            {
                                System.Diagnostics.Debug.WriteLine("attemptStraightQuantize: " + colors.Count + " > " + MaxColor);
                                overMax = true;
                                continue;
                            }
                        }
                        else if (overMax) { continue; }
                        switch (target)
                        {
                            case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                                iBuff[offs + j] = key;
                                break;
                            case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                                iBuff[offs + (j / 2)] |= (byte)((key & 0x0F) << (j % 2 == 0 ? 4 : 0));
                                break;
                        }
                    }
                }
                totalColors = colors.Count;
                if (overMax) { return null; }
            }
            {
                Bitmap bmp = new Bitmap(width, height, target);
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(Rectangle.FromLTRB(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, target);
                try { System.Runtime.InteropServices.Marshal.Copy(iBuff, 0, data.Scan0, iBuff.Length); }
                finally { bmp.UnlockBits(data); }
                var palette = bmp.Palette;
                foreach (System.Collections.Generic.KeyValuePair<int, byte> color in colors)
                {
                    palette.Entries[color.Value] = Color.FromArgb(color.Key);
                }
                bmp.Palette = palette;
                return bmp;
            }
        }
        /// <summary><para>Ask the user if thay want to allow quantization and apply it.</para><para>Throws on "Cancel", quantizes on "OK".</para></summary>
        /// <param name="input">User input <c>Bitmap</c></param>
        /// <param name="target">Target (original) image <c>PixelFormat</c></param>
        protected static void requestQuantize(ref Bitmap input, System.Drawing.Imaging.PixelFormat target)
        {
            const string tmsg = "Input image does not match the type of the target image. ";
            //Make sure target PixelFormat is supported
            int MaxColor, totalColor = 0;
            switch (target)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: MaxColor = 256; break;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed: MaxColor = 16; break;
                default: throw new NotSupportedException(string.Format(tmsg + "In addition, cannot quantize to {0}.", target));
            }
            //Make sure input image PixelFormat is supported (WuQuantizer limitations + my assumptions)
            int bitDepth = System.Drawing.Image.GetPixelFormatSize(input.PixelFormat);
            if (bitDepth != 32) { throw new NotSupportedException(string.Format(tmsg + "In addition, quantization can only be applied to 32-bit RGBA images; input image is {0}-bit.", bitDepth)); }

            Bitmap quant = null;
            try
            {
                quant = attemptStraightQuantize(input, target, out totalColor);
                System.Diagnostics.Debug.WriteLineIf(quant != null, "attemptStraightQuantize: Converted image successfully!");
            }
            catch (Exception e)
            {
                //Attempt to recover
                quant = null;
            }
            if (quant == null)
            {
                //Ask user if they want to attempt
                //Quantize (Alpha < 5% considered fully transparent; step alpha levels in values of 1)
                quant = lossyQuantize(input, MaxColor);
            }
            if (quant != null)
            {
                input.Dispose();
                input = quant;
            }
            else { throw new NullReferenceException("Failed to quantize image."); }
        }

        protected readonly System.Collections.Generic.List<Bitmap> bmps = new System.Collections.Generic.List<Bitmap>();
        public int imageCount { get { return this.bmps.Count; } }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.bmps.Count != 0)
                {
                    foreach (Bitmap bmp in this.bmps) { bmp.Dispose(); }
                    this.bmps.Clear();
                }
            }
        }
        protected virtual void setBMPInternal(int index, ref Bitmap bmp) { throw new NotImplementedException(); }
        public abstract void parse();

        public void Dispose() { Dispose(true); }
        public Bitmap getBMP(int index)
        {
            if (index >= this.bmps.Count) { throw new IndexOutOfRangeException(); }
            return this.bmps[index];
        }
        public void setBMP(int index, Bitmap bmp)
        {
            if (index >= this.bmps.Count) { throw new IndexOutOfRangeException(); }
            System.Diagnostics.Debug.Assert(bmp != null);
            System.Diagnostics.Debug.Assert(bmp.Width > 0);
            System.Diagnostics.Debug.Assert(bmp.Height > 0);
            try
            {
                setBMPInternal(index, ref bmp);
            }
            catch (Exception e)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(bmp != null);
            System.Diagnostics.Debug.Assert(bmp.Width > 0);
            System.Diagnostics.Debug.Assert(bmp.Height > 0);
            if (this.bmps[index] != null) { this.bmps[index].Dispose(); }
            this.bmps[index] = bmp;
        }
    }
}
