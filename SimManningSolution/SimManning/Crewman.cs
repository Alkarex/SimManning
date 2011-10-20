using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;
using SimManning.Simulation;

namespace SimManning
{
	public abstract class Crewman : IComparable<Crewman>
	{
		int id;

		public int Id
		{
			get { return this.id; }
			//internal set { this.id = value; }
		}

		string name;

		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		string description = String.Empty;

		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		int crewmanType;

		public int CrewmanType
		{
			get { return this.crewmanType; }
			set { this.crewmanType = value; }
		}

		readonly Dictionary<int, byte> qualifications = new Dictionary<int, byte>();

		/// <summary>
		/// {Task ID, Percentage of qualification/assigment from 0 to 100}
		/// </summary>
		public IDictionary<int, byte> Qualifications
		{
			get { return this.qualifications; }
		}

		protected Crewman(int id)
		{
			this.id = id;
		}

		public int CompareTo(Crewman other)
		{
			return this.id - other.id;
		}

		public virtual bool Identical(Crewman other)
		{
			if ((other == null) || (this.id != other.id) || (this.name != other.name) ||	//Do not compare description
				(this.crewmanType != other.crewmanType) || (this.qualifications.Count != other.qualifications.Count))
				return false;
			byte percentage2;
			foreach (var qualification in this.qualifications)
				if (!(other.Qualifications.TryGetValue(qualification.Key, out percentage2) && (qualification.Value == percentage2)))
					return false;
			/*var regexSourceTaskId = new Regex(@"[|]\s*(\d{1,5})-", RegexOptions.Compiled | RegexOptions.CultureInvariant);	//To remove the ID of the parent task, which might vary after export/import
			foreach (var qualification in this.qualifications)
				if (!crewMember2.qualifications.Any(qualification2 => (regexSourceTaskId.Replace(qualification.Key.name, String.Empty) == regexSourceTaskId.Replace(qualification2.Key.name, String.Empty))
					&& (qualification.Value == qualification2.Value)))	//Use Task.name instead of Task.id to be robust about different task IDs after export/import
					return false;*/
			return true;
		}

		public override string ToString()
		{
			return String.Concat(this.id, '.', this.name);
		}

		#region IO
		protected internal virtual bool LoadFromXml(TaskDictionary taskDictionary, XElement element)
		{
			XAttribute attr;
			this.name = (attr = element.Attribute("name")) == null ? this.id.ToString(CultureInfo.InvariantCulture) : attr.Value;
			this.crewmanType = element.Attribute("crewMemberType").ParseInteger();
			XElement elem;
			this.description = (elem = element.Element("description")) == null ? String.Empty : elem.Value;
			foreach (var taskRef in element.Elements("TaskRef"))
			{
				var taskId = taskRef.Attribute("refId").ParseInteger();
				SimulationTask task;
				if (taskDictionary.TryGetValue(taskId, out task))
					this.qualifications.Add(taskId, taskRef.Attribute("percent").Value.ParseByte());
			}
			return true;
		}

		protected internal virtual void SaveToXml(TaskDictionary taskDictionary, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("CrewMember");
			xmlWriter.WriteAttributeString("id", this.id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteAttributeString("crewMemberType", this.crewmanType.ToString(CultureInfo.InvariantCulture));
			if (!String.IsNullOrWhiteSpace(this.description))
				xmlWriter.WriteElementString("description", this.description);
			foreach (var qualification in this.qualifications)
			{
				if (!taskDictionary.ContainsKey(qualification.Key)) continue;	//The task has been deleted from the central list of tasks
				xmlWriter.WriteStartElement("TaskRef");
				xmlWriter.WriteAttributeString("refId", qualification.Key.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("percent", qualification.Value.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
		}
		#endregion

		#region Simulation

		SimulationTime cumulatedWorkTime = SimulationTime.Zero;

		/// <summary>
		/// Cumulated work time for this crewman since the beginning of the replication.
		/// </summary>
		public SimulationTime CumulatedWorkTime
		{
			get { return this.cumulatedWorkTime; }
		}

		int currentLoad;

		/// <summary>
		/// Number of work tasks (not rest) right now.
		/// </summary>
		public int CurrentLoad
		{
			get { return this.currentLoad; }
		}

		readonly List<SimulationTask> tasksAssigned = new List<SimulationTask>();

		/// <summary>
		/// List of tasks currently active for the crewman.
		/// </summary>
		public List<SimulationTask> TasksAssigned
		{
			get { return this.tasksAssigned; }
		}

		SimulationTime lastRefreshTime = SimulationTime.MinValue;

		/// <summary>
		/// Last time <see cref="RefreshStatus"/> was called.
		/// </summary>
		public SimulationTime LastRefreshTime
		{
			get { return this.lastRefreshTime; }
		}

		/// <summary>
		/// Refresh all status parameters of the crewman such as cumulated work time.
		/// This is called automatically by <see cref="AssignTask"/>, <see cref="DismissTask"/>,
		/// at at the end of each phase transition by <see cref="Simulator"/>.
		/// Furthermore, this should be called by implementations of <see cref="DomainDispatcher"/>
		/// before deciding which crewmen should take a job.
		/// </summary>
		/// <remarks>Implementations of domains should override this function to include their domain-specific parameters</remarks>
		/// <param name="time">Current simulation time</param>
		/// <returns>true if a refresh was needed, false otherwise</returns>
		/// <seealso cref="SimulationTask.UpdateCrewmen"/>
		public virtual bool RefreshStatus(SimulationTime time)
		{//In the base class, we only have cumulatedWorkTime
			Debug.Assert(this.lastRefreshTime <= time, "Simulation cannot go back in time!");
			if (time == this.lastRefreshTime) return false;
			//Debug.WriteLine("?\t{0}\tRefreshStatus\t{1}", time, this);
			if (this.currentLoad > 0)
			{//The crewman is currently working
				var offset = time - this.lastRefreshTime;
				if (offset.Positive) this.cumulatedWorkTime += offset;
			}
			this.lastRefreshTime = time;
			return true;
		}

		/// <summary>
		/// Assign the crewman to a running task.
		/// This starts by calling <see cref="RefreshStatus"/>, increase <see cref="CurrentLoad"/>,
		/// and then add a new entry to <see cref="workHistory"/>.
		/// </summary>
		/// <param name="time">Current simulation time</param>
		/// <param name="phase">Current phase</param>
		/// <param name="task">New task</param>
		public virtual void AssignTask(SimulationTime time, Phase phase, SimulationTask task)
		{
			RefreshStatus(time);
			this.tasksAssigned.Add(task);
			if (task.IsWork) currentLoad++;
		}

		/// <summary>
		/// Remove a running or completed task from the crewman.
		/// This starts by calling <see cref="RefreshStatus"/>, decrease <see cref="CurrentLoad"/>,
		/// and then add a new entry to <see cref="workHistory"/>.
		/// </summary>
		/// <param name="time">Current simulation time</param>
		/// <param name="phase">Current phase</param>
		/// <param name="task">Task dismissed</param>
		public virtual void DismissTask(SimulationTime time, Phase phase, SimulationTask task)
		{
			RefreshStatus(time);
			this.tasksAssigned.Remove(task);
			if (task.IsWork)
			{
				this.currentLoad--;
				Debug.Assert(currentLoad >= 0, "Current load must be positive!");
				if (this.currentLoad < 0) this.currentLoad = 0;
			}
		}

		/// <summary>
		/// Call <see cref="DismissTask"/> for all tasks currently assigned to the crewman.
		/// </summary>
		/// <param name="time">Current simulation time</param>
		/// <param name="phase">Current phase</param>
		public void DismissAllTasks(SimulationTime time, Phase phase)
		{
			while (this.tasksAssigned.Count > 0)
				DismissTask(time, phase, this.tasksAssigned[0]);
		}

		/// <summary>
		/// To be called before each replication to reset some parameters.
		/// Automatically called by <see cref="SimulationDataSet.PrepareForNextReplication"/>
		/// </summary>
		protected internal virtual void PrepareForNextReplication()
		{
			this.lastRefreshTime = SimulationTime.MinValue;
			this.cumulatedWorkTime = SimulationTime.Zero;
			this.tasksAssigned.Clear();
		}
		#endregion
	}
}
