using IMGZ_Editor;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Patch_Image_Tool
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length >= 1 && !string.IsNullOrEmpty(args[0]))
			{
				if (args[0] == "extract")
				{
					Console.WriteLine("Opening {0}...", args[1]);
					byte[] array = File.ReadAllBytes(args[1]);
					byte[] candidate = new byte[]
					{
						95,
						68,
						77,
						89,
						0,
						0,
						0,
						0,
						84,
						69,
						88,
						65
					};
					int num = 0;
					int[] array2 = array.Locate(candidate);
					for (int i = 0; i < array2.Length; i++)
					{
						uint num2 = (uint)array2[i];
						uint num3 = num2 + 8u;
						while (true)
						{
							Console.WriteLine("TEXA offset: {0}", num3);
							if (array[(int)((uint)((UIntPtr)num3))] == 84 && array[(int)((uint)((UIntPtr)(num3 + 1u)))] == 69 && array[(int)((uint)((UIntPtr)(num3 + 2u)))] == 88 && array[(int)((uint)((UIntPtr)(num3 + 3u)))] == 65)
							{
								break;
							}
							Console.Write("\tTEXA header not found ({0}{1}{2}{3}); ", new object[]
							{
								(char)array[(int)((uint)((UIntPtr)num3))],
								(char)array[(int)((uint)((UIntPtr)(num3 + 1u)))],
								(char)array[(int)((uint)((UIntPtr)(num3 + 2u)))],
								(char)array[(int)((uint)((UIntPtr)(num3 + 3u)))]
							});
						}
						uint num4 = num3 + 8u;
						num3 = num4 + 2u;
						ushort num5 = (ushort)((int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						Console.WriteLine("\tPatch target: {0}", num5);
						num3 += 10u;
						ushort num6 = (ushort)((int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						Console.WriteLine("\tPatch count: {0}", num6);
						Console.WriteLine("\tParent X offset: {0}", (int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						Console.WriteLine("\tParent Y offset: {0}", (int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						ushort num7 = (ushort)((int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						ushort num8 = (ushort)((int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8);
						Console.WriteLine("\tPatch images: {0} x {1}", num7, num8);
						num3 += 8u;
						uint num9 = (uint)((int)array[(int)((uint)((UIntPtr)(num3++)))] | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 8 | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 16 | (int)array[(int)((uint)((UIntPtr)(num3++)))] << 24);
						Console.WriteLine("\tData offset: {0}", num9);
						num8 *= num6;
						byte[] array3 = new byte[(int)(num7 * num8)];
						byte[] array4 = new byte[1024];
						for (int j = 0; j < 256; j++)
						{
							array4[j] = (array4[j + 1] = (array4[j + 2] = (array4[j + 3] = (byte)j)));
						}
						Buffer.BlockCopy(array, (int)(num4 + num9), array3, 0, array3.Length);
						Bitmap bitmap = new Bitmap((int)num7, (int)num8, (System.Drawing.Imaging.PixelFormat)198659);
						BitmapData bitmapData = bitmap.LockBits(Rectangle.FromLTRB(0, 0, (int)num7, (int)num8), (System.Drawing.Imaging.ImageLockMode)2, (System.Drawing.Imaging.PixelFormat)198659);
						try
						{
							Marshal.Copy(array3, 0, bitmapData.Scan0, Math.Min(array3.Length, bitmapData.Stride * (int)num8));
						}
						finally
						{
							bitmap.UnlockBits(bitmapData);
						}
						using (FileStream fileStream = File.OpenRead(args[1]))
						{
							MDLX mDLX = new MDLX(fileStream);
							mDLX.parse();
							if (num5 >= 0 && (int)num5 < mDLX.imageCount)
							{
								Bitmap bMP = mDLX.getBMP((int)num5);
								bitmap.Palette = bMP.Palette;
							}
							else
							{
								Console.WriteLine("Couldn't find proper palette, using generic black-and-white palette.");
								ColorPalette palette = bitmap.Palette;
								new Random();
								for (int k = 0; k < palette.Entries.Length; k++)
								{
									palette.Entries[k] = Color.FromArgb(k, k, k);
								}
								bitmap.Palette = palette;
							}
						}
						bitmap.Save(string.Concat(new object[]
						{
							args[1],
							"_",
							num,
							".png"
						}), ImageFormat.Png);
						num++;
					}
				}
				else if (args[0] == "import")
				{
					Console.WriteLine("Not implemented!");
					Console.WriteLine("Opening {0}...", args[1]);
					byte[] array = File.ReadAllBytes(args[1]);
					byte[] candidate = new byte[]
					{
						95,
						68,
						77,
						89,
						0,
						0,
						0,
						0,
						84,
						69,
						88,
						65
					};
					int num = 0;
					int[] array2 = array.Locate(candidate);
					for (int i = 0; i < array2.Length; i++)
					{
						uint num2 = (uint)array2[i];
						uint num10 = num2 + 8u;
						byte[] array5 = File.ReadAllBytes(string.Concat(new object[]
						{
							args[1],
							"_",
							num,
							".png"
						}));
						while (true)
						{
							Console.Write("TEXA offset: {0}", num10);
							if (array[(int)((uint)((UIntPtr)num10))] == 84 && array[(int)((uint)((UIntPtr)(num10 + 1u)))] == 69 && array[(int)((uint)((UIntPtr)(num10 + 2u)))] == 88 && array[(int)((uint)((UIntPtr)(num10 + 3u)))] == 65)
							{
								break;
							}
							Console.Write("\tTEXA header not found ({0}{1}{2}{3}); ", new object[]
							{
								(char)array[(int)((uint)((UIntPtr)num10))],
								(char)array[(int)((uint)((UIntPtr)(num10 + 1u)))],
								(char)array[(int)((uint)((UIntPtr)(num10 + 2u)))],
								(char)array[(int)((uint)((UIntPtr)(num10 + 3u)))]
							});
						}
						uint num4 = num10 + 8u;
						num10 = num4 + 2u;
						ushort num5 = (ushort)((int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						Console.WriteLine("\tPatch target: {0}", num5);
						num10 += 10u;
						ushort num6 = (ushort)((int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						Console.WriteLine("\tPatch count: {0}", num6);
						Console.WriteLine("\tParent X offset: {0}", (int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						Console.WriteLine("\tParent Y offset: {0}", (int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						ushort num7 = (ushort)((int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						ushort num8 = (ushort)((int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8);
						Console.WriteLine("\tPatch images: {0} x {1}", num7, num8);
						num10 += 8u;
						uint num9 = (uint)((int)array[(int)((uint)((UIntPtr)(num10++)))] | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 8 | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 16 | (int)array[(int)((uint)((UIntPtr)(num10++)))] << 24);
						Console.WriteLine("\tData offset: {0}", num9);
						num8 *= num6;
						byte[] array3 = new byte[(int)(num7 * num8)];
						byte[] array4 = new byte[1024];
						for (int j = 0; j < 256; j++)
						{
							array4[j] = (array4[j + 1] = (array4[j + 2] = (array4[j + 3] = (byte)j)));
						}
						Bitmap bitmap = new Bitmap((int)num7, (int)num8, (System.Drawing.Imaging.PixelFormat)198659);
						BitmapData bitmapData = bitmap.LockBits(Rectangle.FromLTRB(0, 0, (int)num7, (int)num8), (System.Drawing.Imaging.ImageLockMode)2, (System.Drawing.Imaging.PixelFormat)198659);
						try
						{
							Marshal.Copy(array5, 0, bitmapData.Scan0, Math.Min(array5.Length, bitmapData.Stride * (int)num8));
						}
						finally
						{
							bitmap.UnlockBits(bitmapData);
						}
					}
				}
				else
				{
					Console.WriteLine("Usage: extract|import *.mdlx");
				}
			}
		}
	}
}
