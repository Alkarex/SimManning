using System;
using System.Globalization;

namespace SimManning
{
	public enum ProbabilityDistribution
	{
		Undefined,
		Constant,
		Exponential,
		Triangular
	}

	/// <summary>
	/// Constant, triangular, or exponential distribution.
	/// </summary>
	public struct TimeDistribution : IComparable<TimeDistribution>, IEquatable<TimeDistribution>
	{//TODO: Optimise with helper variables. Make read-ony.
		//TODO: This structure is large. Check if it is faster with a sealed class
		
		public TimeUnit Unit
		{
			get { return this.mode.Unit; }
			set
			{
				this.mode.Unit = value;
				this.min.Unit = value;
				this.max.Unit = value;
			}
		}

		ProbabilityDistribution probabilityDistribution;

		public ProbabilityDistribution Distribution
		{
			get { return this.probabilityDistribution; }
		}

		/// <summary>
		/// Minimum value for a triangular distribution, or undefined for an exponential distribution.
		/// </summary>
		SimulationTime min;

		public SimulationTime Min
		{
			get { return this.min; }
		}

		/// <summary>
		/// Mode of a triangular distribution, or mean of the exponential distribution (if max ≤ 0).
		/// </summary>
		SimulationTime mode;

		public SimulationTime Mode
		{
			get { return this.mode; }
		}

		/// <summary>
		/// Maximum value for a triangular distribution, or ≤ 0 for an exponential distribution.
		/// </summary>
		SimulationTime max;

		public SimulationTime Max
		{
			get { return this.max; }
		}

		SimulationTime xValue;

		/// <summary>
		/// Concrete value (specified, or generated from the statistical distribution).
		/// </summary>
		public SimulationTime XValue
		{
			get
			{
				return this.xValue;
			}
		}

		public TimeDistribution(TimeUnit timeUnit, SimulationTime min, SimulationTime mode, SimulationTime max)
		{
			this.min = min;
			this.mode = mode;
			this.max = max;
			if (this.mode <= SimulationTime.Zero)
			{
				this.probabilityDistribution = ProbabilityDistribution.Constant;
				this.min = this.max = this.mode = SimulationTime.Zero;
			}
			else if (this.max <= SimulationTime.Zero)
			{
				this.probabilityDistribution = ProbabilityDistribution.Exponential;
				this.min = this.max = SimulationTime.Zero;
			}
			else
			{
				if (this.min > this.mode) this.min = this.mode;
				else if (this.min < SimulationTime.Zero) this.min = SimulationTime.Zero;
				if (this.max < this.mode) this.max = this.mode;
				if ((this.min < this.mode) || (this.mode < this.max))
					this.probabilityDistribution = ProbabilityDistribution.Triangular;
				else this.probabilityDistribution = ProbabilityDistribution.Constant;
			}
			this.xValue = this.mode;
			this.Unit = timeUnit;
		}

		public TimeDistribution(SimulationTime min, SimulationTime mode, SimulationTime max) :
			this(mode.Unit, min, mode, max) {}

		public TimeDistribution(TimeUnit timeUnit, SimulationTime constantValue) :
			this(timeUnit, constantValue, constantValue, constantValue) {}

		public TimeDistribution(SimulationTime constantValue) :
			this(constantValue.Unit, constantValue, constantValue, constantValue) {}

		public TimeDistribution(TimeUnit timeUnit, long min, long mode, long max) :
			this(timeUnit, new SimulationTime(timeUnit, min), new SimulationTime(timeUnit, mode), new SimulationTime(timeUnit, max)) {}

		public TimeDistribution(TimeUnit timeUnit, double min, double mode, double max) :
			this(timeUnit, new SimulationTime(timeUnit, min), new SimulationTime(timeUnit, mode), new SimulationTime(timeUnit, max)) {}

		public TimeDistribution(TimeUnit timeUnit, long constantValue) :
			this(timeUnit, constantValue, constantValue, constantValue) { }

		public TimeDistribution(TimeUnit timeUnit, double constantValue) :
			this(timeUnit, constantValue, constantValue, constantValue) {}

		public TimeDistribution(TimeUnit timeUnit, string min, string mode, string max) :
			this(timeUnit, new SimulationTime(timeUnit, min), new SimulationTime(timeUnit, mode), new SimulationTime(timeUnit, max)) {}

		public TimeDistribution(string timeUnit, string min, string mode, string max) :
			this(timeUnit.ParseTimeUnit(), min, mode, max) {}

		public static readonly TimeDistribution Zero = new TimeDistribution(SimulationTime.Zero);

		public SimulationTime MinPossible
		{
			get
			{
				switch (this.probabilityDistribution)
				{
					case ProbabilityDistribution.Constant:
						return this.mode;
					case ProbabilityDistribution.Exponential:
						return SimulationTime.Epsilon;
					case ProbabilityDistribution.Triangular:
						return this.min;
					default:
						return SimulationTime.Zero;
				}
			}
		}

		public SimulationTime Average
		{
			get
			{
				return this.probabilityDistribution == ProbabilityDistribution.Triangular ?
					new SimulationTime(TimeUnit.Seconds, (this.min.TotalSeconds + this.mode.TotalSeconds + this.max.TotalSeconds) / 3.0) :	//Compromise between speed, accuracy, and risk of overload
					this.mode;
			}
		}

		/// <remarks>
		/// In the case of an exponential distribution, returns the maximum value in practice (i.e. ~744.4 × mode),
		/// instead of the theoretical Double.PositiveInfinity or even Double.MaxValue (~1.8E+308).
		/// </remarks>
		public SimulationTime MaxPossible
		{
			get
			{
				return this.probabilityDistribution == ProbabilityDistribution.Exponential ?
					this.mode * (-SimManningCommon.LogMinValue) :	//TODO: Check if we want that or e.g. Double.MaxValue
					this.max;
			}
		}

		public void Validate()
		{
			if ((this.Unit == TimeUnit.Undefined) || (this.mode < SimulationTime.Zero))
				this.mode = SimulationTime.Zero;
			if (this.min > this.mode)
				this.min = this.mode;
			else if (this.min < SimulationTime.Zero)
				this.min = SimulationTime.Zero;
			if ((this.max != SimulationTime.Zero) && (this.max < this.mode))
				this.max = this.mode;
		}

		/// <summary>
		/// Generates the concrete value based on its statistical distribution and a random number,
		/// and assign it to <see cref="XValue"/>.
		/// </summary>
		/// <param name="rand">A random number generator.</param>
		/// <returns>The concrete value generated, ≥ 0</returns>
		public SimulationTime NextValue(Random rand)
		{
			switch (this.probabilityDistribution)
			{
				case ProbabilityDistribution.Exponential:
					this.xValue = -Math.Log(1.0 - rand.NextDouble()) * mode;	//mode == mean == 1/lambda (where lamda is the rate parameter)
					return this.xValue;
				case ProbabilityDistribution.Triangular:
					//TODO:Optimise (e.g. helper variables)	//Try to implement with integers?
					var u = rand.NextDouble();
					var minS = this.min.TotalSeconds;
					var modeS = this.mode.TotalSeconds;
					var maxS = this.max.TotalSeconds;
					if (u <= ((modeS - minS) / (maxS - minS)))
						this.xValue = new SimulationTime(TimeUnit.Seconds, minS + Math.Sqrt((u * (maxS - minS) * (modeS - minS))));
					else this.xValue = new SimulationTime(TimeUnit.Seconds, maxS - Math.Sqrt(((1.0 - u) * (maxS - minS) * (maxS - modeS))));
					this.xValue.Unit = this.Unit;
					return this.xValue;
				case ProbabilityDistribution.Constant:	//Nothing to do
				default:
					return this.xValue;
			}
		}

		public override string ToString()
		{
			switch (this.probabilityDistribution)
			{
				case ProbabilityDistribution.Constant:
					return this.mode.ToStringUIFixedUnit(showUnit: true);
				case ProbabilityDistribution.Exponential:
					return String.Format(CultureInfo.InvariantCulture, "≈{0}", this.mode.ToStringUIFixedUnit(showUnit: true));
				case ProbabilityDistribution.Triangular:
					return String.Format(CultureInfo.InvariantCulture, "{0} ≤ {1} ≥ {2}", this.min.ToStringUIFixedUnit(showUnit: false),
					this.mode.ToStringUIFixedUnit(showUnit: true), this.max.ToStringUIFixedUnit(showUnit: false));
				default:
					return "0";
			}
		}

		public int CompareTo(TimeDistribution other)
		{
			return this.Average.CompareTo(other.Average);
		}

		public override int GetHashCode()
		{
			return this.min.GetHashCode() ^ this.mode.GetHashCode() ^ this.max.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var t2 = obj as TimeDistribution?;
			return t2.HasValue && (this == t2.Value);
		}

		public bool Equals(TimeDistribution other)
		{
			return this == other;
		}

		#region Operators
		public static bool operator ==(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return (!((timeDistribution1.Unit == TimeUnit.Undefined) ^ (timeDistribution2.Unit == TimeUnit.Undefined))) && ((timeDistribution1.mode == timeDistribution2.mode) && (timeDistribution1.min == timeDistribution2.min) && (timeDistribution1.max == timeDistribution2.max));
		}

		public static bool operator !=(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return ((timeDistribution1.Unit == TimeUnit.Undefined) ^ (timeDistribution2.Unit == TimeUnit.Undefined)) || (timeDistribution1.mode != timeDistribution2.mode) || (timeDistribution1.min != timeDistribution2.min) || (timeDistribution1.max != timeDistribution2.max);
		}

		public static bool operator <(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return timeDistribution1.Average < timeDistribution2.Average;
		}

		public static bool operator <=(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return timeDistribution1.Average <= timeDistribution2.Average;
		}

		public static bool operator >(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return timeDistribution1.Average > timeDistribution2.Average;
		}

		public static bool operator >=(TimeDistribution timeDistribution1, TimeDistribution timeDistribution2)
		{
			return timeDistribution1.Average >= timeDistribution2.Average;
		}

		public static explicit operator TimeDistribution(SimulationTime simulationTime)
		{
			return new TimeDistribution(simulationTime);
		}
		#endregion
	}
}
