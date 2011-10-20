using System;

namespace SimManning
{
	public enum TimeUnit
	{
		Undefined,
		/// <summary>
		/// Most precise internal representation of time.
		/// </summary>
		/// <remarks>
		/// Different from TimeSpan!
		/// </remarks>
		Ticks,
		//Milliseconds,
		Seconds,
		Minutes,
		Hours,
		Days,
		Weeks,
		Months,
		Years
	}

	public static class TimeUnitExtensions
	{
		public static TimeUnit ParseTimeUnit(this string value)
		{
			if (String.IsNullOrEmpty(value)) return TimeUnit.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (TimeUnit)Enum.Parse(typeof(TimeUnit), value, ignoreCase: true);
				return Enum.IsDefined(typeof(TimeUnit), result) ? result : TimeUnit.Undefined;
			}
			catch (ArgumentException) { return TimeUnit.Undefined; }
			catch (OverflowException) { return TimeUnit.Undefined; }
		}

		/// <summary>
		/// Returns the standard abbreviation for a given time unit (e.g. "min" for Minutes).
		/// </summary>
		/// <param name="timeUnit">A time unit</param>
		/// <returns>An abbreviation of a time unit</returns>
		/// <remarks>Unified Code for Units of Measure, http://aurora.regenstrief.org/~ucum/ucum.html#section-Derived-Unit-Atoms </remarks>
		public static string Symbol(this TimeUnit timeUnit)
		{
			switch (timeUnit)
			{
				case TimeUnit.Ticks: return "ticks";
				//case TimeUnit.Milliseconds: return "ms";
				case TimeUnit.Seconds: return "s";
				case TimeUnit.Minutes: return "min";
				case TimeUnit.Hours: return "h";
				case TimeUnit.Days: return "d";
				case TimeUnit.Weeks: return "wk";
				case TimeUnit.Months: return "mo";	//Mean Julian month
				case TimeUnit.Years: return "a";	//Julian year
				default: return String.Empty;
			}
		}
	}
}
