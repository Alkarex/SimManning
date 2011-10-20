using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimManning
{
	public sealed class PhaseType
	{//TODO: Make abstract?
		readonly int id;

		public int Id
		{
			get { return this.id; }
		}

		string name;

		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		readonly Dictionary<double, int> durationHistogram = new Dictionary<double, int>();

		/// <summary>
		/// Histograms of durations rounded in a given time unit: {x time unit, number of occurences}
		/// </summary>
		public Dictionary<double, int> DurationHistogram
		{
			get { return this.durationHistogram; }
		}

		readonly Dictionary<int, int> taskErrors = new Dictionary<int, int>();

		/// <summary>
		/// {task ID, number of occurences }
		/// </summary>
		public Dictionary<int, int> TaskErrors
		{
			get { return this.taskErrors; }
		}

		SimulationTime totalDuration;

		public SimulationTime TotalDuration
		{
			get { return this.totalDuration; }
			set { this.totalDuration = value; }
		}

		int occurrences;

		public int Occurrences
		{
			get { return this.occurrences; }
			set { this.occurrences = value; }
		}

		public PhaseType(int id)
		{
			this.id = id;
			this.name = String.Format(CultureInfo.InvariantCulture, "PhaseType{0}", id);
		}

		public SimulationTime DurationMean
		{
			get
			{
				return this.occurrences > 0 ? (this.totalDuration / this.occurrences) : SimulationTime.Zero;
			}
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "({0}){1}", this.id, this.name);
		}
	}
}
