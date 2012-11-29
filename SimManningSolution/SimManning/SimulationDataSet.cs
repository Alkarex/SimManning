using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SimManning.IO;
using System.Diagnostics;

namespace SimManning
{
	/// <summary>
	/// The simulation data-set is the central class for all simulation data input.
	/// </summary>
	public abstract class SimulationDataSet
	{
		Workplace workplace;

		public Workplace Workplace
		{
			get { return this.workplace; }
			protected set { this.workplace = value; }
		}

		TaskDictionary taskDictionary;

		public TaskDictionary TaskDictionary
		{
			get { return this.taskDictionary; }
			protected set { this.taskDictionary = value; }
		}

		Scenario scenario;

		public Scenario Scenario
		{
			get { return this.scenario; }
			protected set { this.scenario = value; }
		}

		Crew crew;

		public Crew Crew
		{
			get { return this.crew; }
			protected set { this.crew = value; }
		}

		TaskDictionary taskDictionaryExpanded;

		/// <summary>
		/// Task list after the expansion of automatic tasks by <see cref="AutoExpandTasks"/> (e.g. one task per crew-member).
		/// </summary>
		public TaskDictionary TaskDictionaryExpanded	//TODO: Make virtual ?	//TODO: Find better name
		{
			get { return this.taskDictionaryExpanded; }
			protected set { this.taskDictionaryExpanded = value; }
		}

		readonly List<string> assertions;

		public List<string> Assertions
		{
			get { return this.assertions; }
		}

		/// <summary>
		/// Constructor of a dataSet from some existing data.
		/// </summary>
		/// <param name="workplace">The workplace.</param>
		/// <param name="taskDictionary">The taskDictionary for the workplace. If this parameter is null, it will be taken from crew.taskDictionary</param>
		/// <param name="crew">The crew.</param>
		/// <param name="scenario">The scenario. If the callBack function scenario.loadPhase is null, one function will be generated to use this.phases as the source of phases</param>
		protected SimulationDataSet(Workplace workplace, TaskDictionary taskDictionary, Scenario scenario, Crew crew)
		{//Whould be better to make a deep copy of everything when creating the dataSet?
			this.workplace = workplace;
			this.taskDictionary = taskDictionary ?? (crew == null ? null : crew.TaskDictionary);
			this.scenario = scenario;
			this.crew = crew;
			this.assertions = new List<string>();
		}

		/// <summary>
		/// Generate <see cref="TaskDictionaryExpanded"/> by expanding the tasks (i.e.creating one instance per <see cref="Crewman"/>)
		/// that use <see cref="SimulationTask.AutoExpandToAllCrewmen"/>.
		/// </summary>
		/// <remarks>
		/// No copy of the taskDictionary is made, so only one SimulationDataSet can be used at a time with the same taskDictionary object.
		/// If the taskDictionary is to be used again, remember to call SimulationDataSet.Clean() when this SimulationDataSet is not needed anymore.
		/// </remarks>
		public void AutoExpandTasks()
		{
			this.taskDictionaryExpanded.Clear();
			var taskRelationsTemp = new List<TaskDictionary.TaskRelationTemp>();
			var autoId = 5000;
			Func<int, SimulationTask, TaskLinkingType, SimulationTask> createTask = this.taskDictionaryExpanded.CreateTask;	//Cache frequent virtual method call
			foreach (var task in this.taskDictionary.Values)
				if (task.AutoExpandToAllCrewmen)
				{
					foreach (var crewman in this.crew.Values)
					{
						while (this.taskDictionaryExpanded.ContainsKey(autoId) || this.taskDictionary.ContainsKey(autoId))
							autoId++;
						var autoTask = createTask(autoId, task, TaskLinkingType.Clear);
						autoTask.AutoExpandToAllCrewmen = false;
						autoTask.Name = String.Format(CultureInfo.InvariantCulture, "{0} | {1}-{2:00}.{3}", autoTask.Name, task.Id, crewman.Id, crewman.Name);
						/*foreach (var taskRelation in task.Relations)
							taskRelationsTemp.Add(new TaskDictionary.TaskRelationTemp(autoTask, taskRelation.Relation,
								task.Id == taskRelation.Task1.Id ? taskRelation.Task2.Id : taskRelation.Task1.Id));*/
						/*autoTask.MasterTasks.Clear();
						autoTask.ParallelTasks.Clear();
						autoTask.SlaveTasks.Clear();*/
						foreach (var task2 in task.ParallelTasks.Values)
							taskRelationsTemp.Add(new TaskDictionary.TaskRelationTemp(autoTask, TaskRelation.RelationType.Parallel, task2.Id));
						foreach (var task2 in task.SlaveTasks.Values)
							taskRelationsTemp.Add(new TaskDictionary.TaskRelationTemp(autoTask, TaskRelation.RelationType.Slave, task2.Id));
						this.taskDictionaryExpanded.Add(autoTask.Id, autoTask);
						crewman.Qualifications.Add(autoTask.Id, 100);
						SimulationTask taskRef;
						foreach (var phase in this.scenario.Phases)	//Update all TaskRef in phases
							if (phase.Tasks.TryGetValue(task.Id, out taskRef) && (!phase.Tasks.ContainsKey(autoTask.Id)))
								phase.Tasks.Add(autoTask.Id, this.taskDictionaryExpanded.CreateTask(autoTask.Id, autoTask, TaskLinkingType.Linked));
					}
					autoId = (int)Math.Ceiling(autoId / 50.0) * 50;	//Try to group autoExpanded tasks by groups of 50 Ids
				}
				else this.taskDictionaryExpanded.Add(task.Id, task);
			this.taskDictionaryExpanded.AddTaskRelations(taskRelationsTemp);
			taskRelationsTemp.Clear();
			//We currently do not make a copy of taskDictionary and its tasks, and we do not want to alter taskDictionary.
			//Tasks and task relations that are autoAssignToAllCrewmen will be filtered out during export.
		}

		/// <summary>
		/// Clean the additional tasks resulting from the auto-expansion of all tasks with attribute  <see cref="SimulationTask.AutoExpandToAllCrewmen"/> set to true.
		/// </summary>
		/// <remarks>
		/// It is important to call this method before using the rest of the data (e.g. taskDictionary, crew, phases) in another context.
		/// Basically, it should be called as soon as this SimulationDataSet object is not needed anymore.
		/// </remarks>
		public void CleanAutoExpandedTasks()
		{//TODO: Proof check: risk of memory leak here.
			foreach (var autoTask in this.taskDictionaryExpanded.Values.Where(t => t.IsAutoExpanded))
			{
				autoTask.Name = String.Format(CultureInfo.InvariantCulture, "!Invalid reference! {0}", autoTask.Name);
				autoTask.PhaseInterruptionPolicy = PhaseInterruptionPolicies.Undefined;	//Make invalid (just in case we would have forgotten a reference to it)
				/*foreach (var autoTaskRelation in autoTask.Relations)
				{
					var task2 = autoTaskRelation.Task1.Id == autoTask.Id ? autoTaskRelation.Task2 : autoTaskRelation.Task1;
					task2.RemoveRelation(autoTask.Id);
				}*/
				foreach (var task2 in autoTask.ParallelTasks.Values)
					task2.ParallelTasks.Remove(autoTask.Id);
				foreach (var task2 in autoTask.SlaveTasks.Values)
					task2.SlaveTasks.Remove(autoTask.Id);
				foreach (var task2 in autoTask.MasterTasks.Values)
					task2.MasterTasks.Remove(autoTask.Id);
			}
			this.taskDictionaryExpanded.Clear();
			foreach (var crewman in this.crew.Values)
				foreach (var autoTaskId in crewman.Qualifications.Keys.Where(taskId =>
				{
					SimulationTask autoTask;
					return (!this.taskDictionaryExpanded.TryGetValue(taskId, out autoTask)) ||
						autoTask.IsAutoExpanded;
				}).ToList())	//.ToList to make a copy
					crewman.Qualifications.Remove(autoTaskId);
			foreach (var phase in this.scenario.Phases)
				foreach (var autoTask in phase.Tasks.Values.Where(t => t.IsAutoExpanded).ToList())	//.ToList to make a copy
					phase.Tasks.Remove(autoTask.Id);
		}

		/// <summary>
		/// Prepare the dataset for being used for simulation.
		/// </summary>
		public virtual void PrepareForFirstSimulation()
		{
			var num = 1;
			foreach (var phase in this.scenario.Phases)
				phase.Id = num++;	//Assign a different ID sequentially to all the different phases used
			num = 1;
			foreach (var task in this.taskDictionaryExpanded.Values)
			{
				task.InternalId = num++;
				task.simulationCurrentQualifications.Clear();
			}
			foreach (var crewman in this.crew.Values)
				foreach (var qualification in crewman.Qualifications.Where(q => q.Value > 0))	//Cache some qualification information
				{
					SimulationTask task;
					if (this.TaskDictionaryExpanded.TryGetValue(qualification.Key, out task))
						task.simulationCurrentQualifications.Add(crewman, qualification.Value);
					else Debug.Assert(this.taskDictionary.TryGetValue(qualification.Key, out task),
						String.Format("✗ Invalid qualification reference!", "✗ Task qualification reference problem for task ID {0} and crewman {1}!",
						qualification.Key, qualification.Value));
				}
		}

		public virtual void PrepareForNextReplication()
		{
			foreach (var crewman in this.crew.Values)
				crewman.PrepareForNextReplication();
		}

		public virtual bool Identical(SimulationDataSet simulationDataSet2)
		{
			if (simulationDataSet2 == null) return false;
			return this.workplace.Identical(simulationDataSet2.workplace) && this.taskDictionary.Identical(simulationDataSet2.taskDictionary) &&
				this.scenario.Identical(simulationDataSet2.scenario) && this.crew.Identical(simulationDataSet2.crew);
			//this.phases and this.phaseTypes probably not important
		}

		/// <summary>
		/// Tells if a task is used at all or not.
		/// A task can be used generically for some phase type,
		/// as a master task for other tasks,
		/// or as a specific task for a phase.
		/// </summary>
		/// <param name="task">A task</param>
		/// <returns>true if the task is used, false otherwise</returns>
		public bool TaskIsUsed(SimulationTask task)
		{
			return (task.PhaseTypes.Count > 0) || task.MasterTasks.Any() || this.scenario.Phases.Any(p => p.Tasks.ContainsKey(task.Id));
		}

		public virtual string ErrorMessage
		{
			get
			{
				var stringBuilder = new StringBuilder();
				var result = this.TaskDictionaryExpanded.ErrorMessage;
				if (!String.IsNullOrEmpty(result))
					stringBuilder.Append("✗ Some of the tasks are invalid! First error: ").AppendLine(result);
				result = this.scenario.ErrorMessage;
				if (!String.IsNullOrEmpty(result))
					stringBuilder.Append("✗ The scenario is invalid! First error: ").AppendLine(result);
				result = this.Crew.ErrorMessage;
				if (!String.IsNullOrEmpty(result))
					stringBuilder.Append("✗ Some of the tasks are not assigned to enough crew-members! First error: ").AppendLine(result);
				return stringBuilder.ToString();
			}
		}

		public virtual string WarningMessage
		{
			get
			{
				var stringBuilder = new StringBuilder();
				foreach (var task in this.taskDictionary.Values)
					if (!TaskIsUsed(task))
						stringBuilder.Append(task).Append("; ");
				if (stringBuilder.Length > 0)
					return stringBuilder.Insert(0, "The following tasks are not used: ").ToString();
				else return String.Empty;
			}
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}, {1} {2}, {3}",
				this.workplace.Name, this.scenario.Name, this.scenario.Duration, this.crew.Name);
		}

		#region IO
		public void SaveToSingleXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("DataSet");
			xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
			xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			this.workplace.SaveToXml(xmlWriter);
			this.taskDictionary.SaveToXml(xmlWriter);
			foreach (var phase in this.scenario.Phases)
				phase.SaveToXml(xmlWriter);
			this.scenario.SaveToXml(xmlWriter);
			this.crew.SaveToXml(xmlWriter);
			xmlWriter.WriteStartElement("Assertions");
			foreach (var assertion in this.assertions)
				xmlWriter.WriteElementString("Assertion", assertion);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		public void SaveToSingleXml(TextWriter textWriter)
		{
			using (var writer = XmlWriter.Create(textWriter, XmlIO.SimManningXmlWriterSettings))
			{
				SaveToSingleXml(writer);
			}
		}
		#endregion
	}
}
