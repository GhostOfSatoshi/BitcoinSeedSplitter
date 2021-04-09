using System;
using System.Collections.Generic;
using System.Text;

	class clsGBitcoin
	{
		public static Encoding NoBOMUTF8 = new UTF8Encoding(false);

		public static byte[] Normalize(string str)
		{
			return NoBOMUTF8.GetBytes(NormalizeString(str));
		}
		internal static string NormalizeString(string word)
		{
			return word.Normalize(NormalizationForm.FormKD);
		}
		public static Byte[] Concat(Byte[] source1, Byte[] source2)
		{
			//Most efficient way to merge two arrays this according to http://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
			Byte[] buffer = new Byte[source1.Length + source2.Length];
			System.Buffer.BlockCopy(source1, 0, buffer, 0, source1.Length);
			System.Buffer.BlockCopy(source2, 0, buffer, source1.Length, source2.Length);

			return buffer;
		}
	}
