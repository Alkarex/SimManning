using System;
using System.Diagnostics;
using System.Linq;
using SimManning.Simulation;

namespace SimManning.Domain.Basic
{
	/// <summary>
	/// Basic dispatcher to assign tasks.
	/// </summary>
	/// <remarks>
	/// Implementators of specific domains should inherit from <see cref="DomainDispatcher"/> and provide a customised task assignment.
	/// </remarks>
	public class BasicDispatcher : DomainDispatcher
	{
		/// <summary>
		/// Maximum duration of a task before the dispatcher will attempt to assign it again,
		/// possibly to another crewman.
		/// </summary>
		public SimulationTime assignmentExpiry = new SimulationTime(TimeUnit.Hours, 4.0);

		public BasicDispatcher(SimulationDataSet dataSet) : base(dataSet) { }

		enum AssignmentCoefficiant : int
		{
			Zero = 0,
			Qualification = 5,	//Positive effect
			CumulatedWorkHours = -1,	//Negative effect
		}

		struct ScoreEntry : IComparable<ScoreEntry>
		{
			public readonly double score;
			public readonly Crewman crewman;

			public ScoreEntry(double score, Crewman crewman)
			{
				this.score = score;
				this.crewman = crewman;
			}

			public int CompareTo(ScoreEntry other)
			{
				var otherScore = other.score;
				if (this.score < otherScore) return -1;
				if (this.score > otherScore) return 1;
				return 0;
			}
		}

		public override SimulationTime TaskAssignment(SimulationTime time, Phase phase, SimulationTask task, Simulator.TasksInterruptedCallback tasksInterruptedCallback)
		{//TODO: Implement multitasking
			Debug.Assert(task.IsAllowedTime(time), "Task assignment must be within allowed daily hours!");
			if (task.NumberOfCrewmenNeeded == 0)
			{
				base.TriggerTaskAssignment(time, phase, null, task);
				return SimulationTime.MaxValue;
			}
			var priority = task.Priority;
			var scores = new BinaryHeap<ScoreEntry>();
			foreach (var qualification in task.simulationCurrentQualifications)
			{
				var crewman = qualification.Key;
				var qualificationLevel = qualification.Value;
				var currentPriority = crewman.TasksAssigned.Count > 0 ? crewman.TasksAssigned[0].Priority : 0;	//No multitasking implemented: right now, each crewman has only 1 task at a time
				Debug.Assert(currentPriority <= 0 || crewman.TasksAssigned[0].simulationCrewmenAssigned.Any(cm => cm.Id == crewman.Id),
					"If a crewman is assigned to a task, the symetric link should also be true, i.e. the task should be assigned to the same crewman!");
				if ((currentPriority == 0) || (priority / 100 > currentPriority / 100) || crewman.TasksAssigned.Contains(task))
				{
					crewman.RefreshStatus(time);
					var scoreEntry = new ScoreEntry((qualificationLevel * (int)AssignmentCoefficiant.Qualification) +
						(crewman.CumulatedWorkTime.TotalHours * (int)AssignmentCoefficiant.CumulatedWorkHours), crewman);
					if (task.NumberOfCrewmenNeeded > scores.Count)
						scores.Add(scoreEntry);
					else if (scores.Peek().score < scoreEntry.score)
					{
						scores.Remove();	//Remove the current minimal score
						scores.Add(scoreEntry);
					}
				}
			}
			if (scores.Count < task.NumberOfCrewmenNeeded)
			{
				scores.Clear();
				return SimulationTime.MinValue;	//The assignment was not successful
			}
			for (var i = scores.Count - 1; i >= 0; i--)
			{
				var scoreEntry = scores[i];
				var crewman = scoreEntry.crewman;
				if (crewman.TasksAssigned.Count > 0)
				{
					var currentTasks = crewman.TasksAssigned.ToList();
					tasksInterruptedCallback(currentTasks, tryAssignmentAgainNow: true);	//Do not clear the crew-members assigned before calling the interruption
					crewman.DismissAllTasks(time, phase);	//Currently not compatible with multitasking
					Debug.Assert(!currentTasks.Any(t => t.simulationCrewmenAssigned.Count > 0), "Nobody should remain assigned to a task that has been interrupted!");
				}
				crewman.AssignTask(time, phase, task);
				task.simulationCrewmenAssigned.Add(crewman);
				base.TriggerTaskAssignment(time, phase, crewman, task);
			}
			scores.Clear();
			return time + assignmentExpiry;
		}
	}
}
