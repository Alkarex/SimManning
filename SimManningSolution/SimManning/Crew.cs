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
	/// A group of <see cref="Crewman"/> to perform the tasks.
	/// </summary>
	public abstract class Crew : Dictionary<int, Crewman>
	{
		readonly string name;

		/// <summary>
		/// Name of the crew.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		string description = String.Empty;

		/// <summary>
		/// Description of the crew.
		/// Does not influence the simulation.
		/// </summary>
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

		readonly TaskDictionary taskDictionary;

		/// <summary>
		/// Reference to the list of all tasks at workplace level.
		/// </summary>
		public TaskDictionary TaskDictionary
		{
			get { return this.taskDictionary; }
		}

		/// <summary>
		/// New empty crew
		/// </summary>
		/// <param name="name">Name of the crew</param>
		/// <param name="taskDictionary">Reference to the list of all tasks at workplace level.</param>
		protected Crew(string name, TaskDictionary taskDictionary)
		{
			this.name = name;
			this.taskDictionary = taskDictionary;
		}

		public abstract Crewman CreateCrewman(int id);

		/*public IEnumerable<Task> AssignedTasks
		{
			get
			{
				var myTasks = new List<Task>();
				foreach (var crewMember in this.Values.Where(cm => cm.Id > 0))
					foreach (var myTask in crewMember.Qualifications.Where(p => p.Value > 0))
						if (!myTasks.Contains(myTask.Key))
							myTasks.Add(myTask.Key);
				return myTasks.AsReadOnly();
			}
		}*/

		/// <summary>
		/// Provides an error message that is empty if no error is found,
		/// or which contains some information about the first task not correctly assigned.
		/// </summary>
		public virtual string ErrorMessage
		{
			get
			{
				foreach (var task in this.taskDictionary.Values)
					if (task.EnforceAssignment && (!task.IsAssigned(this)))
						return String.Format(CultureInfo.InvariantCulture, "Task “{0}” is not correctly assigned!", task);
				return String.Empty;
			}
		}

		/// <summary>
		/// Next available crewman ID.
		/// </summary>
		public virtual int NextCrewmanId
		{
			get { return (base.Count > 0) ? base.Keys.Max() + 1 : 1; }
		}

		/// <summary>
		/// Tells if two crews are logically identical.
		/// </summary>
		/// <param name="other">Another crew to compare</param>
		/// <returns></returns>
		public virtual bool Identical(Crew other)
		{
			if ((other == null) || (this.name != other.name) ||
				(base.Values.Where(cm => cm.Id > 0).Count() != other.Values.Where(cm => cm.Id > 0).Count()))
				return false;
			Crewman crewman2;
			foreach (var crewman in base.Values.Where(cm => cm.Id > 0))
				if (!(other.TryGetValue(crewman.Id, out crewman2) && crewman.Identical(crewman2)))
					return false;
			return true;
		}

		#region IO
		/// <summary>
		/// Load a crew from an XML serialisation.
		/// </summary>
		/// <param name="element">An XML element representing a crew</param>
		public virtual void LoadFromXml(XElement element)
		{
			this.description = element.Element("description").Value;
			foreach (var xmlCrewman in element.Elements("CrewMember"))
			{
				var crewman = this.CreateCrewman(xmlCrewman.Attribute("id").ParseInteger());
				crewman.LoadFromXml(this.taskDictionary, xmlCrewman);
				base.Add(crewman.Id, crewman);
			}
		}

		/// <summary>
		/// Save a crew to an XML serialisation.
		/// </summary>
		/// <param name="xmlWriter">An XML writer</param>
		public virtual void SaveToXml(XmlWriter xmlWriter)
		{
			var needsDeclaration = xmlWriter.WriteState == WriteState.Start;
			xmlWriter.WriteStartElement("Crew");
			if (needsDeclaration)
			{
				xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
				xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			}
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteElementString("description", this.description);
			foreach (var crewman in base.Values.Where(cm => cm.Id > 0))
				crewman.SaveToXml(this.TaskDictionary, xmlWriter);
			xmlWriter.WriteEndElement();
		}
		#endregion
	}
}
