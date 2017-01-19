using System;
using System.Collections.Generic;

namespace Patch_Image_Tool
{
	internal static class ByteArrayRocks
	{
		private static readonly int[] Empty = new int[0];

		public static int[] Locate(this byte[] self, byte[] candidate)
		{
			int[] result;
			if (ByteArrayRocks.IsEmptyLocate(self, candidate))
			{
				result = ByteArrayRocks.Empty;
			}
			else
			{
				List<int> list = new List<int>();
				for (int i = 0; i < self.Length; i++)
				{
					if (ByteArrayRocks.IsMatch(self, i, candidate))
					{
						list.Add(i);
					}
				}
				result = ((list.Count == 0) ? ByteArrayRocks.Empty : list.ToArray());
			}
			return result;
		}

		private static bool IsMatch(byte[] array, int position, byte[] candidate)
		{
			bool result;
			if (candidate.Length > array.Length - position)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < candidate.Length; i++)
				{
					if (array[position + i] != candidate[i])
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}

		private static bool IsEmptyLocate(byte[] array, byte[] candidate)
		{
			return array == null || candidate == null || array.Length == 0 || candidate.Length == 0 || candidate.Length > array.Length;
		}
	}
}
