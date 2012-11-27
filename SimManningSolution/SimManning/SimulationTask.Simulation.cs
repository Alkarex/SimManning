using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimManning
{
	partial class SimulationTask
	{
		/// <summary>
		/// Cached information coming from Crewman.Qualifications. {Crewman, percentage}
		/// </summary>
		public readonly IDictionary<Crewman, byte> simulationCurrentQualifications;

		//public double timeTaskArrives;

		//public double timeTaskBegins;

		SimulationTime lastProcessTime;

		SimulationTime simulationTimeArrives = new SimulationTime(TimeUnit.Seconds, -1.0);

		/// <summary>
		/// Simulation time when this task instance has arrived (typically when changing state from "planned" to running).
		/// </summary>
		public SimulationTime SimulationTimeArrives
		{
			get { return this.simulationTimeArrives; }
			set
			{
				this.simulationTimeArrives = value;
				//if (this.simulationTimeLastCalculation < value)
				this.lastProcessTime = value;
			}
		}

		/*public SimulationTime SimulationTimeLastCalculation
		{
			get { return this.simulationTimeLastCalculation; }
			set { this.simulationTimeLastCalculation = value; }
		}*/

		SimulationTime remainingDuration = SimulationTime.Zero;

		/// <summary>
		/// Remaining man-hours before completion of the task.
		/// </summary>
		public SimulationTime RemainingDuration
		{
			get
			{
				//if (this.remainingDuration.NegativeOrZero) this.remainingDuration = SimulationTime.Zero;
				return this.remainingDuration;
			}
			internal set { this.remainingDuration = value; }
		}

		/// <summary>
		/// True if the task is completed, false otherwise.
		/// </summary>
		public bool Completed
		{
			get { return this.remainingDuration.NegativeOrZero; }
		}

		/// <summary>
		/// List of crewmen currently working on the task.
		/// </summary>
		public readonly List<Crewman> simulationCrewmenAssigned = new List<Crewman>();

		/// <summary>
		/// To call before starting a new occurence of the same simulation.
		/// </summary>
		public virtual void PrepareForNextOccurrence()
		{
			this.remainingDuration = this.Duration.XValue;
			this.lastProcessTime = this.simulationTimeArrives;
			//if (this.simulationCrewMembersAssigned == null) this.simulationCrewMembersAssigned = new List<CrewMember>();
			//else this.crewMembersAssigned.Clear();	//TODO: Find out if we want to clear the list at this point
		}

		/// <summary>
		/// To call when the task has been active until now, in order to update the statistics of the task (e.g. remaining duration).
		/// Does not call <see cref="Crewman.RefreshStatus"/> by default.
		/// </summary>
		/// <param name="currentTime">Current simulation time.</param>
		/// <param name="phase">Phase during which the process occurs.</param>
		/// <returns>A positive duration if the task was processed, <see cref="SimulationTime.Zero"/> otherwise.</returns>
		public virtual SimulationTime ProcessUntilNow(SimulationTime currentTime, Phase phase)
		{
			var previousProcessTime = this.lastProcessTime;
			this.lastProcessTime = currentTime;
			if (this.simulationCrewmenAssigned.Count >= this.numberOfCrewmenNeeded)
			{
				var duration = currentTime - previousProcessTime;
				if (duration.Positive)
				{
					this.remainingDuration -= duration;
					Debug.Assert((this.taskType == (int)StandardTaskType.InternalWait) || Allowed(phase), "A task must not run in phases where it is not allowed!");
					Debug.Assert(duration <= currentTime - phase.simulationTimeBegin, "A task cannot last longer than its phase!");
					return duration;
				}
			}
			return SimulationTime.Zero;
		}

		/// <summary>
		/// To call when the task has been waiting until now, in order to update the statistics of the task.
		/// </summary>
		/// <param name="currentTime">Current simulation time.</param>
		public virtual void SleepUntilNow(SimulationTime currentTime)
		{
			this.lastProcessTime = currentTime;
		}

		/// <summary>
		/// To call when the time since last update needs to be discarded.
		/// Used for instance when a task has to start a bit before than current time and be shortened accordingly.
		/// </summary>
		/// <param name="currentTime">Current simulation time.</param>
		public virtual void DiscardUntilNow(SimulationTime currentTime)
		{
			var duration = currentTime - this.lastProcessTime;
			if (duration.Positive) this.remainingDuration -= duration;
			this.lastProcessTime = currentTime;
		}

		public SimulationTime RemainingProcessingTime()
		{//TODO: Integrate simulationCrewMembersAssigned
			/*double speed = 1.0;
			double exclusivity = 1.0;
			return (speed <= 0.0) || (exclusivity <= 0) ? SimulationTime.MaxValue :
				new SimulationTime(TimeUnit.Seconds, (this.remainingDuration.TotalSeconds / speed) / exclusivity);*/
			return this.remainingDuration;
		}

		/// <summary>
		/// Check the different time constraints to tell when the task will next be allowed to be resumed.
		/// </summary>
		/// <param name="eventTime">A simulation time</param>
		/// <param name="allowCurrentTime">Gives the possibility to use the given simulation time</param>
		/// <returns>A simulation time in the relative future when the task is allowed to run</returns>
		public SimulationTime NextPossibleResume(SimulationTime eventTime, bool allowCurrentTime = true)
		{
			var workStart = eventTime.NextDayTime(this.DailyHourStart, allowCurrentTime);
			var workEnd = eventTime.NextDayTime(this.DailyHourEnd, allowCurrentTime);
			if ((workStart < workEnd) || (eventTime >= workEnd))
				eventTime = workStart;	//TimeWindow, next day
			else if (!allowCurrentTime) eventTime = eventTime.NextUp();
			if ((!this.onHolidays) && eventTime.IsSunday)
			{
				eventTime = eventTime.NextWeekTime(SimulationTime.OneDay, allowCurrentTime: false);	//Next monday
				workStart = eventTime.NextDayTime(this.DailyHourStart);
				workEnd = eventTime.NextDayTime(this.DailyHourEnd);
				if ((workStart < workEnd) || (eventTime >= workEnd))
					eventTime = workStart;	//TimeWindow, next day
			}
			return eventTime;
		}

		/// <summary>
		/// Tells if the task is allowed to run at this time of the day.
		/// </summary>
		/// <param name="eventTime">A date/time; only the time of the day will be used.</param>
		/// <returns>true if the task is allowed to run at this time of the day, false otherwise.</returns>
		public bool IsAllowedTime(SimulationTime eventTime)
		{
			return eventTime.InDayTimeInterval(this.DailyHourStart, this.DailyHourEnd) &&
				(this.onHolidays || (!eventTime.IsSunday) ||
					(!eventTime.NextDown().IsSunday));	//Case of Sunday at 00:00
		}
	}
}
