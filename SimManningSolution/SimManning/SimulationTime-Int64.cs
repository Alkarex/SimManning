using System;
using System.Globalization;
using System.Text;
using SimManning.IO;

namespace SimManning
{
	/// <summary>
	/// Simulation time suited for simulation, based on seconds encoded in Int64.
	/// When converted to calendar information, the zero is at 00:00 (midnight) of the first day of the year,
	/// which is also the fist days of the month, and first day of the week (e.g. depending on conventions "Monday 1st January 2007 or 2018").
	/// </summary>
	/// <remarks>
	/// Alternative to TimeSpan because TimeSpan.MaxValue is too small for simulation (~30K years).
	/// </remarks>
	public struct SimulationTime : IComparable<SimulationTime>, IEquatable<SimulationTime>
	{//TODO: Implement IConvertible, Serializable?

		#region Constants

		/// <summary>
		/// Julian year (365.25 days), as recommended by the International Astronomical Union (IAU).
		/// </summary>
		public const double DaysPerYear = 365.25;

		/// <summary>
		/// Strictly positive integer number of ticks per second, for the internal representation of time.
		/// </summary>
		/// <remarks>This is the central parameter controlling the time accuracy. Can be tuned.</remarks>
		public const int TicksPerSecond = 1000;

		/// <summary>
		/// Number of ticks per minute, 60 seconds per minute.
		/// </summary>
		public const long TicksPerMinute = 60 * TicksPerSecond;

		/// <summary>
		/// Number of ticks per hour, 60 minutes per hour.
		/// </summary>
		public const long TicksPerHour = 60 * TicksPerMinute;

		/// <summary>
		/// Number of ticks per day, 24 hours per day.
		/// </summary>
		public const long TicksPerDay = 24 * TicksPerHour;

		/// <summary>
		/// Average ticks per week, based on weeks of 7 days in agreement with ISO 8601.
		/// </summary>
		public const long TicksPerWeek = 7 * TicksPerDay;

		/// <summary>
		/// Average ticks per month, based on TicksPerDay and DaysPerYear, 12 months per year.
		/// </summary>
		public const long TicksPerMonth = (long)(TicksPerDay * (DaysPerYear / 12.0));

		/// <summary>
		/// Julian year, as recommended by the International Astronomical Union (IAU).
		/// </summary>
		public const long TicksPerYear = (long)(TicksPerDay * DaysPerYear);

		/// <summary>
		/// Used when we need to define a duration of something that should not complete by itself,
		/// such as a task that will be killed by another event.
		/// </summary>
		public static readonly SimulationTime ArbitraryLargeDuration = new SimulationTime(TimeUnit.Years, 3);

		#endregion

		/// <summary>
		/// Prefered time unit.
		/// </summary>
		TimeUnit timeUnit;

		public TimeUnit Unit
		{
			get { return this.timeUnit; }
			set { this.timeUnit = value; }
		}
		
		/// <summary>
		/// This is the native internal storage of the information.
		/// </summary>
		public long internalTime;

		SimulationTime(long internalTicks)
		{
			this.timeUnit = TimeUnit.Seconds;
			this.internalTime = internalTicks;
		}

		public SimulationTime(TimeUnit timeUnit, long value)
		{//TODO: Check boudaries
			this.timeUnit = timeUnit;
			switch (timeUnit)
			{
				case TimeUnit.Ticks: this.internalTime = value; break;
				case TimeUnit.Seconds: this.internalTime = value * TicksPerSecond; break;
				case TimeUnit.Minutes: this.internalTime = value * TicksPerMinute; break;
				case TimeUnit.Hours: this.internalTime = value * TicksPerHour; break;
				case TimeUnit.Days: this.internalTime = value * TicksPerDay; break;
				case TimeUnit.Weeks: this.internalTime = value * TicksPerWeek; break;
				case TimeUnit.Years: this.internalTime = value * TicksPerYear; break;
				default: this.internalTime = 0; break;
			}
		}

		public SimulationTime(TimeUnit timeUnit, double value)
		{
			this.timeUnit = timeUnit;
			switch (timeUnit)
			{//Math.Round() is needed to reduce decimal to binary conversion problems (example: try with 16.9 hours)
				case TimeUnit.Ticks: this.internalTime = (long)Math.Round(value); break;
				case TimeUnit.Seconds: this.internalTime = (long)Math.Round(value * TicksPerSecond); break;
				case TimeUnit.Minutes: this.internalTime = (long)Math.Round(value * TicksPerMinute); break;
				case TimeUnit.Hours: this.internalTime = (long)Math.Round(value * TicksPerHour); break;
				case TimeUnit.Days: this.internalTime = (long)Math.Round(value * TicksPerDay); break;
				case TimeUnit.Weeks: this.internalTime = (long)Math.Round(value * TicksPerWeek); break;
				case TimeUnit.Years: this.internalTime = (long)Math.Round(value * TicksPerYear); break;
				default: this.internalTime = 0; break;
			}
		}

		public SimulationTime(TimeUnit timeUnit, string value) :
			this(timeUnit, value.ParseReal()) {}

		public double TotalSeconds
		{
			get { return this.internalTime / (double)TicksPerSecond; }
			set
			{
				this.timeUnit = TimeUnit.Seconds;
				this.internalTime = (long)Math.Round(value * TicksPerSecond);
			}
		}

		public double TotalMinutes
		{
			get { return this.internalTime / (double)TicksPerMinute; }
		}

		public double TotalHours
		{
			get { return this.internalTime / (double)TicksPerHour; }
		}

		public double TotalDays
		{
			get { return this.internalTime / (double)TicksPerDay; }
		}

		public long Ticks
		{
			get { return this.internalTime; }
			set
			{
				this.timeUnit = TimeUnit.Seconds;
				this.internalTime = value;
			}
		}

		public bool IsSunday
		{
			get { return this.internalTime % TicksPerWeek >= TicksPerDay * 6; } 
		}

		public static SimulationTime FromTimeSpan(TimeSpan value)
		{
			return new SimulationTime(value.Ticks / (TimeSpan.TicksPerSecond / TicksPerSecond));
		}

		public TimeSpan ToTimeSpan()
		{
			return new TimeSpan(this.internalTime * (TimeSpan.TicksPerSecond / TicksPerSecond));
		}

		public double CustomTime
		{
			get
			{
				switch (this.timeUnit)
				{
					case TimeUnit.Ticks: return this.internalTime;
					case TimeUnit.Seconds: return this.internalTime / (double)TicksPerSecond;
					case TimeUnit.Minutes: return this.internalTime / (double)TicksPerMinute;
					case TimeUnit.Hours: return this.internalTime / (double)TicksPerHour;
					case TimeUnit.Days: return this.internalTime / (double)TicksPerDay;
					case TimeUnit.Weeks: return this.internalTime / (double)TicksPerWeek;
					case TimeUnit.Months: return this.internalTime / (double)TicksPerMonth;
					case TimeUnit.Years: return this.internalTime / (double)TicksPerYear;
					default: return 0.0;
				}
			}
		}

		/// <summary>
		/// Return the time of the current day, since the last midnight 00:00.
		/// </summary>
		public SimulationTime DayTime
		{
			get { return new SimulationTime(this.internalTime % TicksPerDay); }
		}

		/// <summary>
		/// Return the time of the current week, since the first midnight 00:00 of the week.
		/// </summary>
		public SimulationTime WeekTime
		{
			get { return new SimulationTime(this.internalTime % TicksPerWeek); }
		}

		/// <summary>
		/// Return the time of the current month, since the first midnight 00:00 of the month.
		/// </summary>
		public SimulationTime MonthTime
		{//TODO: Modify to get an integer number of days since the beginning of the month
			get { return new SimulationTime(this.internalTime % TicksPerMonth); }
		}

		/// <summary>
		/// Return the time of the current year, since the first midnight 00:00 of the year.
		/// </summary>
		public SimulationTime YearTime
		{//TODO: Modify to get an integer number of days since the beginning of the year
			get { return new SimulationTime(this.internalTime % TicksPerYear); }
		}

		/// <summary>
		/// Return true if the current SimulationTime is negative or zero, false otherwise.
		/// </summary>
		public bool NegativeOrZero
		{
			get { return this.internalTime <= 0; }
		}

		/// <summary>
		/// Return true if the current SimulationTime is strictly negative, false otherwise.
		/// </summary>
		public bool Negative
		{
			get { return this.internalTime < 0; }
		}

		/// <summary>
		/// Return true if the current SimulationTime is positive or zero, false otherwise.
		/// </summary>
		public bool PositiveOrZero
		{
			get { return this.internalTime >= 0; }
		}

		/// <summary>
		/// Return true if the current SimulationTime is strictly positive, false otherwise.
		/// </summary>
		public bool Positive
		{
			get { return this.internalTime > 0; }
		}

		/// <summary>
		/// Returns the next nearest time in the future.
		/// </summary>
		public SimulationTime NextUp()
		{
			return new SimulationTime(this.internalTime + 1);
		}

		/// <summary>
		/// Returns the next nearest time in the past.
		/// </summary>
		public SimulationTime NextDown()
		{
			return new SimulationTime(this.internalTime - 1);
		}

		public static readonly SimulationTime Zero = new SimulationTime(0);

		public static readonly SimulationTime Epsilon = new SimulationTime(1);

		public static readonly SimulationTime OneSecond = new SimulationTime(TicksPerSecond);

		public static readonly SimulationTime OneMinute = new SimulationTime(TicksPerMinute);

		public static readonly SimulationTime OneHour = new SimulationTime(TicksPerHour);

		public static readonly SimulationTime OneDay = new SimulationTime(TicksPerDay);

		public static readonly SimulationTime OneWeek = new SimulationTime(TicksPerWeek);

		public static readonly SimulationTime MaxValue = new SimulationTime(long.MaxValue);

		public static readonly SimulationTime MinValue = new SimulationTime(long.MinValue);

		void Explode(out bool negativeSign, out long days, out long hours, out long minutes, out double seconds)
		{
			var myTime = this.internalTime;
			if (myTime < 0)
			{
				myTime *= -1;
				negativeSign = true;
			}
			else negativeSign = false;
			//Do not use Math.DivRem(), because it is much slower, and not available on Silverlight 4
			days = myTime / TicksPerDay;
			myTime -= days * TicksPerDay;
			hours = myTime / TicksPerHour;
			myTime -= hours * TicksPerHour;
			minutes = myTime / TicksPerMinute;
			myTime -= minutes * TicksPerMinute;
			seconds = Math.Round(myTime / (double)TicksPerSecond, 3);
			if (seconds >= 60)
			{
				seconds = 0;
				minutes++;
				if (minutes >= 60)
				{
					minutes = 0;
					hours++;
					if (hours >= 24)
					{
						hours = 0;
						days++;
					}
				}
			}
		}

		/// <summary>
		/// Time period as per ISO 8601 (e.g. "P11DT18H56M32.288S");
		/// </summary>
		/// <returns>This time period as a string compliant with ISO 8601</returns>
		public override string ToString()
		{
			bool negativeSign;
			long days, hours, minutes;
			double seconds;
			Explode(out negativeSign, out days, out hours, out minutes, out seconds);
			var stringBuilder = new StringBuilder();
			if (negativeSign) stringBuilder.Append('-');
			stringBuilder.Append('P');
			if (days > 0) stringBuilder.Append(days).Append('D');
			if (hours + minutes + seconds > 0)
			{
				stringBuilder.Append('T');
				if (hours > 0) stringBuilder.Append(hours).Append('H');
				if (minutes > 0) stringBuilder.Append(minutes).Append('M');
				if (seconds > 0) stringBuilder.Append(seconds).Append('S');
			}
			else if (days <= 0) stringBuilder.Append("0S");
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Generates a string representation suitable for user interfaces.
		/// </summary>
		/// <returns>A string representation suitable for user interface</returns>
		public string ToStringUI()
		{
			bool negativeSign;
			long days, hours, minutes;
			double seconds;
			Explode(out negativeSign, out days, out hours, out minutes, out seconds);
			if (days > 0) return String.Format(CultureInfo.InvariantCulture, "{0}{1}d {2:00}:{3:00}:{4:00}", negativeSign ? "-" : String.Empty, days, hours, minutes, seconds);
			else return String.Format(CultureInfo.InvariantCulture, "{0}{1:00}:{2:00}:{3:00}", negativeSign ? "-" : String.Empty, hours, minutes, seconds);
		}

		/// <summary>
		/// Returns a string of the time expressed in the specified time unit, in full precision and without the unit symbol
		/// </summary>
		/// <returns>A string of the time expressed in the specified time unit. (Real number with dot '.' as decimal separator)</returns>
		public string ToStringRaw()
		{
			return this.CustomTime.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Return the time as a string for the GUI, expressed in the specified time unit, with 3-digit precision and possibly followed by the symbol (abbreviation) for this time unit.
		/// </summary>
		/// <param name="showUnit">Indicates whether the time unit should be displayed or not</param>
		/// <returns>A string with the specified time value expressed in the specified time unit, possibly followed by a time unit symbol (e.g. "min"). (Real number with dot '.' as decimal separator)</returns>
		public string ToStringUIFixedUnit(bool showUnit)
		{
			return String.Format(CultureInfo.InvariantCulture, "{0:#0.###}{1}", this.CustomTime, showUnit && (this.internalTime != 0) ? timeUnit.Symbol() : String.Empty);
		}

		/// <summary>
		/// Convert the time into a string for the GUI, expressed in hours with the format 23:59
		/// </summary>
		/// <returns>A string (for the GUI) of the specified time value, expressed in hours with the format 23:59</returns>
		public string ToStringHourUI()
		{
			var sign = String.Empty;
			var myTime = this.internalTime;
			if (myTime < 0)
			{
				myTime *= -1;
				sign = "-";
			}
			//Do not use Math.DivRem(), because it is much slower, and not available on Silverlight 4
			var hours = myTime / TicksPerHour;
			myTime -= hours * TicksPerHour;
			var minutes = Math.Round(myTime / (double)TicksPerMinute);
			if (minutes >= 60)
			{
				minutes = 0;
				hours++;
			}
			return String.Format(CultureInfo.InvariantCulture, "{0}{1:0#}:{2:00}", sign, hours, minutes);
		}

		/// <remarks>
		/// Automatically substracts one day to the parameter <paramref name="monthTime"/> in order to start on e.g. the 3rd at 00:00 when we say on day three, instead of the 3rd at 24:00
		/// </remarks>
		public SimulationTime NextMonthTime(SimulationTime monthTime, bool allowCurrentTime = true)
		{
			var myTime = allowCurrentTime ? this.internalTime : this.internalTime + 1;
			var currentMonthTimeTicks = myTime % TicksPerMonth;
			var monthTimeTicks = (monthTime.internalTime - TicksPerDay) % TicksPerMonth;
			if (monthTimeTicks >= currentMonthTimeTicks)
				return new SimulationTime((myTime - currentMonthTimeTicks) + monthTimeTicks);	//Current month
			else return new SimulationTime((myTime - currentMonthTimeTicks) + monthTimeTicks + TicksPerMonth);	//Next month
		}

		/// <remarks>
		/// Automatically substracts one day to the parameter <paramref name="weekTime"/> in order to start on e.g. Monday 00:00 when we say on day one, instead of Monday 24:00
		/// </remarks>
		public SimulationTime NextWeekTime(SimulationTime weekTime, bool allowCurrentTime = true)
		{
			var myTime = allowCurrentTime ? this.internalTime : this.internalTime + 1;
			var currentWeekTimeTicks = myTime % TicksPerWeek;
			var weekTimeTicks = (weekTime.internalTime - TicksPerDay) % TicksPerWeek;
			if (weekTimeTicks >= currentWeekTimeTicks)
				return new SimulationTime((myTime - currentWeekTimeTicks) + weekTimeTicks);	//Current week
			else return new SimulationTime((myTime - currentWeekTimeTicks) + weekTimeTicks + TicksPerWeek);	//Next week
		}

		public SimulationTime NextDayTime(SimulationTime dayTime, bool allowCurrentTime = true)
		{
			var myTime = allowCurrentTime ? this.internalTime : this.internalTime + 1;
			var currentDayTimeTicks = myTime % TicksPerDay;
			var dayTimeTicks = dayTime.internalTime % TicksPerDay;
			if (dayTimeTicks >= currentDayTimeTicks)
				return new SimulationTime((myTime - currentDayTimeTicks) + dayTimeTicks);	//Current day
			else return new SimulationTime((myTime - currentDayTimeTicks) + dayTimeTicks + TicksPerDay);	//Next day
		}

		/// <summary>
		/// Calculate the time duration between two hours of a day (like 2 hours from 23:00 to 02:00).
		/// </summary>
		/// <param name="fromTime">"Departure" time, like 1 day 23 hours. (Only the part "23 hours" will be used)</param>
		/// <param name="toTime">"Arrival" time, like 4 days 2 hours. (Only the part "2 hours" will be used)</param>
		/// <returns>The time duration from "fromTime" to "toTime". For instance, the are 3 hours from 23:00 to 02:00</returns>
		public static SimulationTime DayTimeOffset(SimulationTime fromTime, SimulationTime toTime)
		{
			var fromDayTime = fromTime.DayTime;
			var toDayTime = toTime.DayTime;
			return new SimulationTime(fromDayTime <= toDayTime ?
				toDayTime.internalTime - fromDayTime.internalTime :	//Same day
				TicksPerDay + toDayTime.internalTime - fromDayTime.internalTime);	//Next day
		}

		public int CompareTo(SimulationTime other)
		{//Hot path
			//return this.totalSeconds.CompareTo(other.totalSeconds);	//Manually inline below
			//return this.internalTime - other.internalTime;	//Could do that in Int32, but not Int64
			var t1 = this.internalTime;
			var t2 = other.internalTime;
			if (t1 < t2) return -1;
			else if (t1 > t2) return 1;
			else return 0;
		}

		public override int GetHashCode()
		{
			return (int)this.internalTime;
		}

		public override bool Equals(object obj)
		{
			var b = obj as SimulationTime?;
			return b.HasValue && (this == b.Value);
		}

		public bool Equals(SimulationTime other)
		{
			return this.internalTime == other.internalTime;
		}

		#region Operators
		public static bool operator ==(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime == simulationTime2.internalTime;
		}

		public static bool operator !=(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime != simulationTime2.internalTime;
		}

		public static bool operator <(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime < simulationTime2.internalTime;
		}

		public static bool operator <=(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime <= simulationTime2.internalTime;
		}

		public static bool operator >(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime > simulationTime2.internalTime;
		}

		public static bool operator >=(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return simulationTime1.internalTime >= simulationTime2.internalTime;
		}

		public static SimulationTime operator +(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			var result = new SimulationTime(simulationTime1.internalTime + simulationTime2.internalTime);
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator -(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			var result = new SimulationTime(simulationTime1.internalTime - simulationTime2.internalTime);
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator +(SimulationTime simulationTime1, TimeSpan simulationTime2)
		{
			var result = new SimulationTime(simulationTime1.internalTime + (simulationTime2.Ticks / (TimeSpan.TicksPerSecond / TicksPerSecond)));
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator -(SimulationTime simulationTime1, TimeSpan simulationTime2)
		{
			var result = new SimulationTime(simulationTime1.internalTime - (simulationTime2.Ticks / (TimeSpan.TicksPerSecond / TicksPerSecond)));
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator *(SimulationTime simulationTime1, long factor)
		{
			var result = new SimulationTime(simulationTime1.internalTime * factor);
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator *(SimulationTime simulationTime1, double factor)
		{
			var result = new SimulationTime((long)Math.Round(simulationTime1.internalTime * factor));
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator *(long factor, SimulationTime simulationTime1)
		{
			var result = new SimulationTime(simulationTime1.internalTime * factor);
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator *(double factor, SimulationTime simulationTime1)
		{
			var result = new SimulationTime((long)Math.Round(simulationTime1.internalTime * factor));
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator /(SimulationTime simulationTime1, long factor)
		{
			var result = new SimulationTime(simulationTime1.internalTime / factor);
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		public static SimulationTime operator /(SimulationTime simulationTime1, double factor)
		{
			var result = new SimulationTime((long)Math.Round(simulationTime1.internalTime / factor));
			result.timeUnit = simulationTime1.timeUnit;
			return result;
		}

		/*public static SimulationTime operator %(SimulationTime simulationTime1, SimulationTime simulationTime2)
		{
			return new SimulationTime(simulationTime1.totalSeconds % simulationTime2.totalSeconds);
		}*/

		/*public static SimulationTime operator %(SimulationTime simulationTime1, double factor)
		{
			return new SimulationTime(simulationTime1.internalTime % factor);
		}*/

		public static explicit operator SimulationTime(TimeDistribution timeDistribution)
		{
			return timeDistribution.Average;
		}
		#endregion

		public bool InInterval(SimulationTime min, SimulationTime max)
		{
			return (this.internalTime >= min.internalTime) && (this.internalTime <= max.internalTime);
		}

		public bool InInterval(TimeUnit timeUnit, long min, long max)
		{
			return (this.internalTime >= (new SimulationTime(timeUnit, min)).internalTime) &&
				(this.internalTime <= (new SimulationTime(timeUnit, max)).internalTime);
		}

		public bool InDayTimeInterval(SimulationTime minDayTime, SimulationTime maxDayTime)
		{
			var myDayTime = this.internalTime % TicksPerDay;
			var myMinDayTime = minDayTime.internalTime % TicksPerDay;
			var myMaxDayTime = maxDayTime.internalTime % TicksPerDay;
			if (myMinDayTime >= myMaxDayTime) return (myDayTime >= myMinDayTime) || (myDayTime <= myMaxDayTime);
			else return (myDayTime >= myMinDayTime) && (myDayTime <= myMaxDayTime);
		}
	}
}
