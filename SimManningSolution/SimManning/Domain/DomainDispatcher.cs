using System;
using System.Diagnostics;
using SimManning.Simulation;

namespace SimManning.Domain
{
	public abstract class DomainDispatcher
	{
		protected readonly SimulationDataSet dataSet;

		#region Events
		public delegate void TaskAssignmentEventHandler(SimulationTime time, Phase phase, Crewman crewman, SimulationTask task = null);

		public event TaskAssignmentEventHandler OnTaskAssignment;

		protected void TriggerTaskAssignment(SimulationTime time, Phase phase, Crewman crewman, SimulationTask task = null)
		{
			if (this.OnTaskAssignment != null) this.OnTaskAssignment(time, phase, crewman, task);
		}
		#endregion

		protected DomainDispatcher(SimulationDataSet dataSet)
		{
			this.dataSet = dataSet;
		}

		/// <summary>
		/// Attempt to assign a task to the required number of crew-members, using a score to select the most fitted crew-members.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="phase"></param>
		/// <param name="task"></param>
		/// <param name="tasksInterruptedCallback"></param>
		/// <param name="taskAssignmentEvent"></param>
		/// <returns>A Simulation time at which the task assignment will not be valid anymore, or a negative value (e.g. SimulationTime.MinValue) if the task assignment was not successful</returns>
		public abstract SimulationTime TaskAssignment(SimulationTime time, Phase phase, SimulationTask task,
			Simulator.TasksInterruptedCallback tasksInterruptedCallback);

		public virtual void TaskDismiss(SimulationTime time, Phase phase, SimulationTask task)
		{
			var trigger = this.OnTaskAssignment != null;
			foreach (var crewman in task.simulationCrewmenAssigned)
			{
				Debug.Assert(task.IsAllowedTime(time), "A task must not run outside allowed daily hours!");
				crewman.DismissTask(time, phase, task);
				if (trigger) this.OnTaskAssignment(time, phase, crewman);
			}
			task.simulationCrewmenAssigned.Clear();
		}
	}
}
