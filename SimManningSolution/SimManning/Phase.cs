using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;

namespace SimManning
{
	/// <summary>
	/// A period, part of a larger <see cref="Scenario"/>,
	/// during which certain tasks are active while some others are not.
	/// </summary>
	public abstract class Phase
	{
		int id;

		public int Id
		{
			get { return this.id; }
			internal set { this.id = value; }
		}

		readonly string name;

		/// <summary>
		/// The name is used as an unique identifier.
		/// </summary>
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

		int phaseType;

		public int PhaseType
		{
			get { return this.phaseType; }
			set { this.phaseType = value; }
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

		public static readonly SimulationTime ArbitraryMinDuration = new SimulationTime(TimeUnit.Minutes, 6);	//TODO: Make ArbitraryMinDuration a parameter

		public static readonly SimulationTime ArbitraryMaxDuration = new SimulationTime(TimeUnit.Days, 61);	//TODO: Make ArbitraryMaxDuration a parameter

		/// <summary>
		/// Expected duration of the phase expressed as a statistical distribution.
		/// A phase can last longer if there are some obligatory tasks that take time to complete.
		/// </summary>
		/// <remarks>Do not use a property for structures, otherwise the TimeDistribution struct would not be updatable</remarks>
		public TimeDistribution Duration;

		TaskDictionary tasks;

		/// <summary>
		/// Tasks specific to the phase.
		/// </summary>
		public TaskDictionary Tasks
		{
			get { return this.tasks; }
			protected set { this.tasks = value; }
		}

		readonly Phase refPhase;

		/// <summary>
		/// Other phase that was used as a reference to construct this phase.
		/// A phase and its reference phase share their task list (<see cref="Tasks"/>).
		/// </summary>
		internal Phase RefPhase
		{
			get { return this.refPhase; }
		}

		/// <summary>
		/// New empty phase
		/// </summary>
		protected Phase(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Construct a new phase based on an existing reference phase.
		/// A phase and its reference phase share their task list (<see cref="Tasks"/>).
		/// </summary>
		/// <seealso cref="RefPhase"/>
		/// <param name="refPhase">Existing phase used as a reference</param>
		protected Phase(Phase refPhase)
		{
			//if (refPhase == null) return;
			this.name = refPhase.name;
			//this.refPhase = refPhase.RootPhase;
			//if (this.refPhase == null) return;
			this.refPhase = refPhase;
			this.id = refPhase.id;
			this.description = refPhase.description;
			this.Duration = refPhase.Duration;
			this.phaseType = refPhase.phaseType;
			this.tasks = refPhase.tasks;
		}

		public virtual bool Identical(Phase phase2)
		{
			return (phase2 != null) &&
				(this.id == phase2.id) &&	//Do not compare description
				((this.name == phase2.name) || (phase2.name == String.Format(CultureInfo.InvariantCulture, @"p{0:D3}_{1}", this.id, this.name)) || (this.name == String.Format(CultureInfo.InvariantCulture, @"p{0:D3}_{1}", phase2.id, phase2.name))) &&
				(this.phaseType == phase2.phaseType) && (this.Duration == phase2.Duration) &&
				this.tasks.Identical(phase2.tasks);
		}

		public virtual string ErrorMessage
		{
			get
			{
				if (this.phaseType <= 0)
					return "The type of this phase is not set!";
				if (this.Duration.Distribution == ProbabilityDistribution.Exponential)
					return "A phase must not have a duration defined as an exponential distribution (0, mean, 0)!";
				if (this.Duration.MinPossible < Phase.ArbitraryMinDuration)
					return String.Format(CultureInfo.InvariantCulture, "This phase must have a minimum duration of at least {0} minutes!", Phase.ArbitraryMinDuration.TotalMinutes);
				if (this.Duration.MaxPossible > Phase.ArbitraryMaxDuration)
					return String.Format(CultureInfo.InvariantCulture, "The duration of this phase must not exceed {0} days!", Phase.ArbitraryMaxDuration.TotalDays);
				return String.Empty;
			}
		}

		/*public Phase RootPhase
		{
			get
			{
				Phase rootPhase = this;
				Phase myPhaseRef = this.refPhase;
				while (myPhaseRef != null)
				{
					rootPhase = myPhaseRef;
					myPhaseRef = rootPhase.refPhase;
				}
				return rootPhase;
			}
		}*/

		public override string ToString()
		{
			return String.Concat(this.id, '.', this.name);
		}

		#region IO
		protected internal virtual void LoadFromXml(XElement element, TaskDictionary taskList)
		{
			this.description = element.Element("description").Value;
			XAttribute attr;
			this.id = (attr = element.Attribute("phaseId")) == null ? 0 : attr.Value.ParseInteger();
			this.phaseType = (attr = element.Attribute("phaseType")) == null ? 0 : attr.Value.ParseInteger();
			this.Duration = XmlIO.ParseTimeDistribution(element.Attribute("phaseDurationUnit"),
				element.Attribute("phaseDurationMin"), element.Attribute("phaseDurationMean"), element.Attribute("phaseDurationMax"));
			foreach (var xmlTaskRef in element.Elements("TaskRef"))
			{
				var taskRef = taskList.LoadTaskRefFromXml(xmlTaskRef);
				if (taskRef != null)
				{
					taskRef.Validate();
					this.tasks.Add(taskRef.Id, taskRef);
				}
			}
		}

		protected internal virtual void SaveToXml(XmlWriter xmlWriter)
		{
			var needsDeclaration = xmlWriter.WriteState == WriteState.Start;
			xmlWriter.WriteStartElement("Phase");
			if (needsDeclaration)
			{
				xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
				xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			}
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteAttributeString("phaseId", this.id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("phaseType", this.phaseType.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("phaseDurationUnit", this.Duration.Unit.ToString());
			xmlWriter.WriteAttributeString("phaseDurationMin", this.Duration.Min.ToStringRaw());
			xmlWriter.WriteAttributeString("phaseDurationMean", this.Duration.Mode.ToStringRaw());
			xmlWriter.WriteAttributeString("phaseDurationMax", this.Duration.Max.ToStringRaw());
			xmlWriter.WriteElementString("description", this.description);
			foreach (var task in this.tasks.Values)
			{
				task.Validate();
				task.SaveRefToXml(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		}

		protected internal virtual void SaveRefToXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("PhaseRef");
			xmlWriter.WriteAttributeString("refName", this.name);
			//TODO: Override values
			xmlWriter.WriteEndElement();
		}
		#endregion

		#region Simulation
		public SimulationTime simulationTimeBegin;
		#endregion
	}
}
