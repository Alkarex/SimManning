using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;

namespace SimManning
{
	/// <summary>
	/// A storyline consisting of a sequence of <see cref="Phase"/>.
	/// </summary>
	public abstract class Scenario
	{
		readonly string name;

		public string Name
		{
			get { return this.name; }
		}

		string description = String.Empty;

		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		string tag;

		/// <summary>
		/// Generic tag that can be used by implementations to store custom data.
		/// Not used in the simulation itself, not used for testing equality, and not imported/exported to e.g. XML.
		/// </summary>
		public string Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		readonly List<Phase> phases = new List<Phase>();

		public List<Phase> Phases
		{
			get { return this.phases; }
		}

		/*/// <summary>
		/// Enumeration of all the tasks of all the specific phases used in phases of this scenario.
		/// </summary>
		public IEnumerable<SimulationTask> AllSpecificTasks
		{
			get
			{
				foreach (var phase in this.phases)
					foreach (var task in phase.Tasks.Values)
						yield return task;
			}
		}*/

		/// <summary>
		/// Foreign function to load a PhaseRef from is name
		/// </summary>
		internal Func<string, Phase> loadPhase;

		protected Scenario(string name, Func<string, Phase> loadPhase)
		{
			this.name = name;
			this.loadPhase = loadPhase;
			if (loadPhase == null)
				loadPhase = phaseRefName => this.phases.FirstOrDefault(p => p.Name == phaseRefName);
				//loadPhase = phaseRefName => null;
		}

		/// <summary>
		/// Construct a new phase based on an existing reference phase.
		/// </summary>
		/// <param name="refPhase">A phase used as a reference.</param>
		/// <returns>A new phase based on an existing reference phase</returns>
		protected abstract Phase CreatePhaseRef(Phase refPhase);

		/// <summary>
		/// Append the phases of another scenario to the current scenario.
		/// </summary>
		/// <param name="subScenario">Another scenario</param>
		public void Append(Scenario subScenario)
		{
			if (subScenario == null) return;
			foreach (var phaseRef in subScenario.phases)
				if (!phaseRef.Name.StartsWith("!", StringComparison.Ordinal))	//Skip system phases such as "!General".
					this.phases.Add(CreatePhaseRef(phaseRef));
		}

		/// <summary>
		/// Append a <see cref="Phase"/> to the scenario, loading the phase given its name.
		/// </summary>
		/// <param name="phaseName">The name of the new Phase to be added</param>
		/// <returns>The Phase that has just been added</returns>
		public Phase AddPhase(string phaseName)
		{//The number of phases in a scenario is not expected to be large, so a sequential search is not a problem and we can avoid yet another dictionary in memory
			if (String.IsNullOrEmpty(phaseName)) return null;
			var phaseRefCache = this.phases.FirstOrDefault(p => p.Name == phaseName);
			var phaseCache = phaseRefCache == null ? loadPhase(phaseName) : phaseRefCache.RefPhase;
			var phaseRef = phaseCache == null ? null : CreatePhaseRef(phaseCache);
			if (phaseRef != null) this.phases.Add(phaseRef);
			return phaseRef;
		}

		/// <summary>
		/// Remove a PhaseRef at the specified index "phaseRefIndex", but only if the name of the PhaseRef is matching the parameter "phaseRefName".
		/// </summary>
		/// <param name="phaseRefIndex">The index of the PhaseRef to be deleted in the scenario (base 0)</param>
		/// <param name="phaseRefName">The name of the PhaseRef to be deleted in the scenario</param>
		/// <returns>True if the operation succeeded, false otherwise</returns>
		public bool RemovePhase(int phaseRefIndex, string phaseRefName)
		{
			if ((phaseRefIndex < 0) || (phaseRefIndex >= this.phases.Count)) return false;
			var phaseRef = this.phases[phaseRefIndex];
			if ((phaseRef == null) || (phaseRef.Name != phaseRefName)) return false;
			this.phases.RemoveAt(phaseRefIndex);
			return true;
		}

		/// <summary>
		/// Re-order the phases based on an ordered dictionary {old index, phase name}.
		/// The phase name is redundant but used for double-checking.
		/// Used for graphical user interfaces.
		/// </summary>
		/// <param name="dictionary">An ordered dictionary {old index, phase name}</param>
		/// <returns>true if the processed succeeded, false otherwise</returns>
		public bool SortPhases(IDictionary<int, string> dictionary)
		{
			if (dictionary == null) return false;
			var newList = new Queue<Phase>(dictionary.Count);
			lock (this.phases)
			{
				foreach (var phaseRefEntry in dictionary)
				{
					var phaseRefIndex = phaseRefEntry.Key;
					var phaseRefName = phaseRefEntry.Value;
					if ((phaseRefIndex < 0) || (phaseRefIndex >= this.phases.Count)) continue;
					var phaseRef = this.phases[phaseRefIndex];
					if ((phaseRef == null) || (phaseRef.Name != phaseRefName)) return false;	//A synchronisation error has occured
					newList.Enqueue(phaseRef);
				}
				var systemPhases = new List<Phase>();
				foreach (var phaseRef in this.phases)
					if (phaseRef.Name.StartsWith("!", StringComparison.Ordinal))	//System phases such as "!General".
						systemPhases.Add(phaseRef);
				this.phases.Clear();
				this.phases.AddRange(systemPhases);	//Keep system phases
				this.phases.AddRange(newList);
			}
			newList.Clear();
			return true;
		}

		public virtual bool Identical(Scenario scenario2)
		{
			if ((scenario2 == null) || (this.name != scenario2.name) || //Do not compare description
				(this.phases.Count != scenario2.phases.Count))
				return false;
			for (var i = this.phases.Count - 1; i >= 0; i--)
				if (!this.phases[i].Identical(scenario2.phases[i]))
					return false;
			return true;
		}

		/*protected internal bool ContainsSpecificTask(int taskId)
		{
			foreach (var phase in this.phases)
				if (phase.Tasks.ContainsKey(taskId)) return true;
			return false;
		}*/

		/// <summary>
		/// Maximum number of phases allowed by scenario.
		/// Governed indirectly by <see cref="Phase.ArbitraryMaxDuration"/>.
		/// </summary>
		public static readonly long MaxNumberOfPhases = SimulationTime.MaxValue.internalTime / Phase.ArbitraryMaxDuration.internalTime;

		public virtual string ErrorMessage
		{
			get
			{
				if (this.phases.Count <= 0)
					return "A scenario must have at least one phase!";
				if (this.phases.Count > MaxNumberOfPhases)
					return String.Format(CultureInfo.InvariantCulture, "A scenario must not contain more than {0} phases!", MaxNumberOfPhases);
				foreach (var phase in this.phases)
				{
					var result = phase.ErrorMessage;
					if (!String.IsNullOrEmpty(result))
						return String.Format(CultureInfo.InvariantCulture, "Invalid phase “{0}”: {1}", phase, result);
				}
				return String.Empty;
			}
		}

		/// <summary>
		/// Estimated scenario duration, taking into account Phase.ArbitraryMaxDuration and Phase.ArbitraryMinDuration.
		/// </summary>
		public TimeDistribution Duration
		{
			get
			{
				if (this.phases.Count <= 0) return TimeDistribution.Zero;
				var min = SimulationTime.Zero;
				var mode = SimulationTime.Zero;
				var max = SimulationTime.Zero;
				foreach (var phase in this.phases)
				{
					var local = phase.Duration.MinPossible;
					if (local > Phase.ArbitraryMaxDuration) local = Phase.ArbitraryMaxDuration;
					else if (local < Phase.ArbitraryMinDuration) local = Phase.ArbitraryMinDuration;
					min += local;

					local = phase.Duration.Mode;
					if (local > Phase.ArbitraryMaxDuration) local = Phase.ArbitraryMaxDuration;
					else if (local < Phase.ArbitraryMinDuration) local = Phase.ArbitraryMinDuration;
					mode += local;

					local = phase.Duration.MaxPossible;
					if (local > Phase.ArbitraryMaxDuration) local = Phase.ArbitraryMaxDuration;
					else if (local < Phase.ArbitraryMinDuration) local = Phase.ArbitraryMinDuration;
					max += local;
				}
				return new TimeDistribution(this.phases[0].Duration.Unit, min, mode, max);
			}
		}

		#region IO
		public virtual void LoadFromXml(XElement element)
		{
			this.description = element.Element("description").Value;
			foreach (var xmlPhaseRef in element.Elements("PhaseRef"))
			{
				XAttribute attr;
				var phaseRefName = (attr = xmlPhaseRef.Attribute("refName")) == null ? String.Empty : attr.Value;
				var phaseRef = this.AddPhase(phaseRefName);
				//TODO: Override phase values with values from the scenario (duration)
			}
		}

		public virtual void SaveToXml(XmlWriter xmlWriter)
		{
			var needsDeclaration = xmlWriter.WriteState == WriteState.Start;
			xmlWriter.WriteStartElement("Scenario");
			if (needsDeclaration)
			{
				xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
				xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			}
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteElementString("description", this.description);
			foreach (var myPhaseRef in this.phases)
				myPhaseRef.SaveRefToXml(xmlWriter);
			xmlWriter.WriteEndElement();
		}
		#endregion
	}
}
