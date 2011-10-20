using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using SimManning.IO;
using System.Xml;
using System.Diagnostics;

namespace SimManning
{
	public abstract class TaskDictionary : Dictionary<int, SimulationTask>
	{
		//internal const int MaxTaskId = 9999;

		public abstract SimulationTask CreateTask(int id);

		public abstract SimulationTask CreateTask(SimulationTask refTask, SimulationTask.LinkingType linkingType);

		public abstract SimulationTask CreateTask(int id, SimulationTask refTask, SimulationTask.LinkingType linkingType);	//TODO: Make protected internal

		public bool Identical(IDictionary<int, SimulationTask> taskDictionary2)
		{
			if ((taskDictionary2 == null) || (this.Count != taskDictionary2.Count))
				return false;
			SimulationTask task2;
			foreach (var task in this.Values)	//.Where(t => (!t.autoExpandToAllCrewMembers) && (!t.isAutoExpanded))
				if (!(taskDictionary2.TryGetValue(task.Id, out task2) && task.Identical(task2)))
					return false;
			return true;
		}

		public string ErrorMessage
		{
			get
			{
				foreach (var task in this.Values.Where(t => t.Enabled))
				{
					var result = task.ErrorMessage;
					if (!String.IsNullOrEmpty(result))
						return String.Format(CultureInfo.InvariantCulture, "Invalid task “{0}”: {1}", task, result);
				}
				return String.Empty;
			}
		}

		public void Validate()
		{
			foreach (var task in this.Values)
				task.Validate();
		}

		/// <summary>
		/// Temporary data during loading to store relations between tasks as int before having real references.
		/// </summary>
		internal struct TaskRelationTemp
		{
			internal readonly SimulationTask task1;

			internal readonly TaskRelation.RelationType relation;

			internal readonly int taskId2;

			public TaskRelationTemp(SimulationTask task1, TaskRelation.RelationType relation, int taskId2)
			{
				this.task1 = task1;
				this.relation = relation;
				this.taskId2 = taskId2;
			}
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

		public static bool AddTaskRelation(SimulationTask task1, TaskRelation.RelationType relationType, SimulationTask task2)
		{
			if ((task1 == null) || (task2 == null)) return false;
			switch (relationType)
			{
				case TaskRelation.RelationType.Parallel:	//Make a complete directed graph
					if (task1.Id == task2.Id) return false;
					foreach (var task3 in task1.ParallelTasks.Values)
						foreach (var task4 in task2.ParallelTasks.Values)
							if (task3.Id != task4.Id)
							{
								if (!task3.ParallelTasks.ContainsKey(task4.Id)) task3.ParallelTasks.Add(task4.Id, task4);
								if (!task4.ParallelTasks.ContainsKey(task3.Id)) task4.ParallelTasks.Add(task3.Id, task3);
							}
					foreach (var task3 in task1.ParallelTasks.Values)
						if (task3.Id != task2.Id)
						{
							if (!task2.ParallelTasks.ContainsKey(task3.Id)) task2.ParallelTasks.Add(task3.Id, task3);
							if (!task3.ParallelTasks.ContainsKey(task2.Id)) task3.ParallelTasks.Add(task2.Id, task2);
						}
					foreach (var task4 in task2.ParallelTasks.Values)
						if (task4.Id != task1.Id)
						{
							if (!task1.ParallelTasks.ContainsKey(task4.Id)) task1.ParallelTasks.Add(task4.Id, task4);
							if (!task4.ParallelTasks.ContainsKey(task1.Id)) task4.ParallelTasks.Add(task1.Id, task1);
						}
					if (!task1.ParallelTasks.ContainsKey(task2.Id)) task1.ParallelTasks.Add(task2.Id, task2);
					if (!task2.ParallelTasks.ContainsKey(task1.Id)) task2.ParallelTasks.Add(task1.Id, task1);
					return true;
				case TaskRelation.RelationType.Slave:
					if ((task1.Id == task2.Id) || (task1.MasterTasks.ContainsKey(task2.Id)) ||
						(task2.SlaveTasks.ContainsKey(task1.Id))) return false;
					if (!task1.SlaveTasks.ContainsKey(task2.Id)) task1.SlaveTasks.Add(task2.Id, task2);
					if (!task2.MasterTasks.ContainsKey(task1.Id)) task2.MasterTasks.Add(task1.Id, task1);
					return true;
				case TaskRelation.RelationType.Master:
					if ((task1.Id == task2.Id) || (task1.SlaveTasks.ContainsKey(task2.Id)) ||
						(task2.MasterTasks.ContainsKey(task1.Id))) return false;
					if (!task1.MasterTasks.ContainsKey(task2.Id)) task1.MasterTasks.Add(task2.Id, task2);
					if (!task2.SlaveTasks.ContainsKey(task1.Id)) task2.SlaveTasks.Add(task1.Id, task1);
					return true;
				case TaskRelation.RelationType.None:
				default:
					return false;
			}
			/*else
			{
				duplicate = task1.Relations.FirstOrDefault(tr => (tr.Task2.Id == taskId2) && (tr.Relation == relationType)).Valid;
				if ((!duplicate) && taskRelation.Valid)
				{
					task1.Relations.Add(taskRelation);
					task2.Relations.Add(taskRelation);
				}
			}*/
		}

		public bool AddTaskRelation(SimulationTask task1, TaskRelation.RelationType relationType, int taskId2)
		{
			SimulationTask task2;
			return (base.TryGetValue(taskId2, out task2) && AddTaskRelation(task1, relationType, task2));
		}

		public bool AddTaskRelation(int taskId1, TaskRelation.RelationType relationType, int taskId2)
		{
			SimulationTask task1, task2;
			return (base.TryGetValue(taskId1, out task1) && base.TryGetValue(taskId2, out task2) &&
				AddTaskRelation(task1, relationType, task2));
		}

		internal void AddTaskRelations(IEnumerable<TaskRelationTemp> taskRelationsTemp)
		{
			foreach (var taskRelationTemp in taskRelationsTemp)
			{
				SimulationTask task2;
				if (base.TryGetValue(taskRelationTemp.taskId2, out task2))
					AddTaskRelation(taskRelationTemp.task1, taskRelationTemp.relation, task2);
			}
		}

		public void UpdateTaskRelations(SimulationTask task, TaskRelation.RelationType relationType, IEnumerable<int> taskIds)
		{
			if (task == null) return;
			switch (relationType)
			{
				case TaskRelation.RelationType.Parallel:
					task.ParallelTasks.Remove(task.Id);
					foreach (var task2 in task.ParallelTasks.Values)
					{
						task2.ParallelTasks.Remove(task.Id);
						foreach (var taskId3 in taskIds)
							task2.ParallelTasks.Remove(taskId3);
					}
					task.ParallelTasks.Clear();
					foreach (var taskId3 in taskIds)
					{
						SimulationTask task3;
						if (base.TryGetValue(taskId3, out task3))
							task3.ParallelTasks.Clear();
					}
					break;
				case TaskRelation.RelationType.Slave:
					task.SlaveTasks.Remove(task.Id);
					foreach (var task2 in task.SlaveTasks.Values)
						task2.MasterTasks.Remove(task.Id);
					task.SlaveTasks.Clear();
					break;
				case TaskRelation.RelationType.Master:
					task.MasterTasks.Remove(task.Id);
					foreach (var task2 in task.MasterTasks.Values)
						task2.SlaveTasks.Remove(task.Id);
					task.MasterTasks.Clear();
					break;
				default:
					return;
			}
			foreach (var taskId2 in taskIds)
				AddTaskRelation(task, relationType, taskId2);
		}

		/// <summary>
		/// Return the maximum ID plus one.
		/// </summary>
		public int NextTaskId
		{
			get { return (base.Count > 0) ? base.Keys.Max() + 1 : 1; }	//TODO: Should keep an XML attribute with the autoId
		}

		public SimulationTask TaskRef(int refId)
		{
			SimulationTask task;
			return base.TryGetValue(refId, out task) ? CreateTask(task, SimulationTask.LinkingType.Linked) : null;
		}

		#region IO
		public virtual void LoadFromXml(XElement element)
		{//TODO: Catch errors
			var taskRelationsTemp = new List<TaskDictionary.TaskRelationTemp>();
			foreach (var xmlTask in element.Elements("Task"))
			{
				var task = this.CreateTask(xmlTask.Attribute("id").ParseInteger());
				task.LoadFromXml(xmlTask);
				task.Validate();
				base.Add(task.Id, task);
				var xmlTaskRelations = xmlTask.Element("taskRelations");
				if (xmlTaskRelations != null)
					foreach (var taskRef in xmlTaskRelations.Elements("TaskRef"))
						taskRelationsTemp.Add(new TaskDictionary.TaskRelationTemp(task, TaskRelation.ParseRelationType(taskRef.Attribute("rel").Value), taskRef.Attribute("refId").Value.ParseInteger()));
			}
			this.AddTaskRelations(taskRelationsTemp);
			taskRelationsTemp.Clear();
		}

		public virtual void SaveToXml(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Tasks");
			if (xmlWriter.WriteState == WriteState.Start)
			{//Some additional declaration is needed if we are at the beginning of a new document
				xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
				xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			}
			foreach (var myTask in base.Values)
			{
				myTask.Validate();
				myTask.SaveToXml(xmlWriter);
			}
			xmlWriter.WriteEndElement();
		}

		protected internal virtual SimulationTask LoadTaskRefFromXml(XElement xmlTaskRef)
		{
			if (xmlTaskRef == null) return null;
			var refId = xmlTaskRef.Attribute("refId").Value.ParseInteger();
			SimulationTask refTask;
			if (base.TryGetValue(refId, out refTask))
			{
				var taskRef = this.CreateTask(refTask, SimulationTask.LinkingType.Linked);
				taskRef.LoadRefFromXml(xmlTaskRef);
				return taskRef;
			}
			else return null;
		}
		#endregion
	}
}
