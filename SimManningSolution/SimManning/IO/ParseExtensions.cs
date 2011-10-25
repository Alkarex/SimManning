using System;
using System.Globalization;

namespace SimManning.IO
{
	public static class ParseExtensions
	{
		public static bool ParseBoolean(this string text)
		{//Would be better with a class static extension when C# supports it
			if (String.IsNullOrEmpty(text)) return false;
			switch (text.ToUpperInvariant())
			{
				//case "0":
				//case "FALSE": return false;
				case "1":
				case "TRUE": return true;
				default: return false;
			}
		}

		public static byte ParseByte(this string text)
		{
			if (String.IsNullOrEmpty(text)) return 0;
			byte result;
			return byte.TryParse(text, out result) ? result : (byte)0;
		}

		public static int ParseInteger(this string text)
		{
			if (String.IsNullOrEmpty(text)) return 0;
			int result;
			return Int32.TryParse(text, out result) ? result : 0;
		}

		public static double ParseReal(this string text)
		{
			double result;
			if (String.IsNullOrEmpty(text) ||
				(!Double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result)) ||
				Double.IsNaN(result)) return 0.0;
			return result;
		}

		/// <summary>
		/// Parse a double and is resitant for null, empty, and comma for decimal separator.
		/// </summary>
		/// <param name="text">The string to parse as a double</param>
		/// <param name="result">The double</param>
		/// <returns>True if the parsing was successful, false otherwise</returns>
		public static bool TryParseReal(this string text, out double result)
		{
			if (String.IsNullOrEmpty(text))
			{
				result = 0.0;
				return false;
			}
			return Double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}
	}
}
