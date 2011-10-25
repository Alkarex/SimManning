using System;
using System.Collections.Generic;

namespace SimManning
{
	/// <summary>
	/// Different means of specifying when a task should occur.
	/// </summary>
	public enum RelativeDateType
	{
		//AbsoluteStopMonthDay = -13,
		//AbsoluteStopWeekDay = -12,
		//RelativeStopFromPreviousStopOccurrence = -4,
		//RelativeStopFromPreviousStartOccurrence = -3,

		/// <summary>
		/// Plan the task to stop at a given duration before the planned end of a phase.
		/// </summary>
		RelativeStopFromEndOfPhase = -2,
		//RelativeStopFromStartOfPhase = -1,

		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Plan the task to start at a given duration after a phase has started.
		/// </summary>
		RelativeStartFromStartOfPhase = 1,

		/// <summary>
		/// Plan the task to start at a given duration before the planned end of a phase.
		/// </summary>
		RelativeStartFromEndOfPhase = 2,

		/// <summary>
		/// Plan the task to occur several times at a specified frequency,
		/// using the start time of the task as a reference.
		/// </summary>
		Frequency = 3,

		//RelativeStartFromPreviousStopOccurrence = 4,

		/// <summary>
		/// Plan the task to start a given duration after the end of the previous occurence of the same task.
		/// </summary>
		RelativeStartFromPreviousStart = 5,

		/// <summary>
		/// The task will only start when triggered by another event (<see cref="SimulationTask.MasterTasks"/>).
		/// </summary>
		TriggeredByAnEvent = 9,

		/// <summary>
		/// A day during the week, e.g. on Monday.
		/// </summary>
		AbsoluteStartWeekDay = 12,

		/// <summary>
		/// A day during the month, e.g. the 24th.
		/// </summary>
		AbsoluteStartMonthDay = 13,

		//MetaPhaseIndependent = AbsoluteStartMonthDay | AbsoluteStartWeekDay | Frequency | RelativeStartFromPreviousStart	//Meta that contains the date types that are phase-independent (to be used only if we update to [Flags])
	}

	/// <summary>
	/// Different means of specifying during which hours, or at which time of the day a task should occur.
	/// </summary>
	public enum RelativeTimeType
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// The task will be scheduled to end at a specific time (e.g. if a meal must be ready at 12:00 but not earlier).
		/// </summary>
		AbsoluteStopTime = -1,

		/// <summary>
		/// The task will wait for a given time before starting.
		/// For instance, if <see cref="SimulationTask.DailyHourStart"/> is 09:00 and <see cref="SimulationTask.DailyHourEnd"/> is 16:00:
		/// - If the task arrives at 08:00, it will wait to start at 09:00 (just like <see cref="RelativeTimeType.TimeWindow"/>);
		/// - If the task arrives at 10:00, it will wait to start the next day at 09:00 (if possible);
		/// </summary>
		AbsoluteStartTime = 1,

		/// <summary>
		/// The task will only run during a time window (e.g. from 09:00 to 16:00).
		/// This is the most common choice.
		/// </summary>
		TimeWindow = 3
	}

	/// <summary>
	/// Defines what happens to a task when it is interrupted during another task.
	/// </summary>
	public enum TaskInterruptionPolicies : int
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// The task will be discarded without generating an error.
		/// </summary>
		DropWithError = 2,

		/// <summary>
		/// The task will be discarded and will generate an error.
		/// </summary>
		DropWithoutError = 4,

		/// <summary>
		/// The task must be re-assigned immediatly to another qualified <see cref="Crewman"/>, otherwise cancelled with error.
		/// </summary>
		ContinueOrDropWithError = 8,

		/// <summary>
		/// The task must be re-assigned immediatly to another qualified <see cref="Crewman"/>, otherwise cancelled without error.
		/// </summary>
		ContinueOrDropWithoutError = 16,

		/// <summary>
		/// 
		/// </summary>
		ContinueOrResumeWithError = 32,

		/// <summary>
		/// Most common behaviour.
		/// </summary>
		ContinueOrResumeWithoutError = 64,
	}

	/// <summary>
	/// Defines what happens to a task when it is interrupted during a phase, or not complete at the end of a phase.
	/// </summary>
	/// <remarks>
	/// Types lower than 10 are with error, while types higher than ≥10 are without error.
	/// Types from 2 to 3 generates errors when interrupted (preempted),
	/// while types 1 and 4 to 7 generates errors when not completed (deleted, routed to exit).
	/// </remarks>
	public enum PhaseInterruptionPolicies : int
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// The task may be interrupted and must be finished before next phase.
		/// It is an error not to complete the task before the end of the current phase
		/// (theoretical, as the task will prevent the phase from ending).
		/// </summary>
		Obligatory = 1,

		/// <summary>
		/// Once started, the task should not be interrupted.
		/// If interrupted (by a task or by a phase change), the task will then be treated as ContinueOrDropWithoutError.
		/// Interruptions are errors (and if the task is not completed, this is not an additional error).
		/// </summary>
		[Obsolete("Use task interruption instead!")]
		DoNotInterrupt = 2,

		/// <summary>
		/// The task must start at the beginning of the current phase and must stop at the end of the current phase.
		/// If interrupted (or if the task cannot start at the very beginning of the current phase),
		/// the task will generate and error and must try to resume in the current phase.
		/// </summary>
		WholePhase = 3,

		/// <summary>
		/// The task may be interrupted and should resume in the current or another phase
		/// (not necessarily the very next phase),
		/// if the phase is listed as a possible phase.
		/// The task must not be cancelled before being completed (finished),
		/// except at the end of the simulation (replication) where it generates an error.
		/// </summary>
		ResumeOrDropWithError = 4,

		/// <summary>
		/// The task may be interrupted and should resume in the current or very next phase
		/// if the phase is listed as a possible phase.
		/// If the task is not completed, it is cancelled (dropped) and generates an error.
		/// </summary>
		ContinueOrDropWithError = 6,

		/// <summary>
		/// The task may be interrupted and should resume in the current phase.
		/// The task is cancelled if not completed at the end of the current phase, and generates an error.
		/// </summary>
		DropWithError = 7,

		/// <summary>
		/// The task may be interrupted and may resume in the current or another phase
		/// (not necessarily the very next phase),
		/// if the phase is listed as a possible phase.
		/// The task may not be cancelled before being completed (finished),
		/// but does not generates an error at the end of the simulation (replication).
		/// </summary>
		ResumeOrDropWithoutError = 14,

		/// <summary>
		/// The task may be interrupted and may resume in the current or very next phase
		/// if the phase is listed as a possible phase.
		/// If the task is not completed, it is cancelled (dropped) without generating an error.
		/// </summary>
		ContinueOrDropWithoutError = 16,

		/// <summary>
		/// The task may be interrupted and may resume in the current phase.
		/// If the task is not completed at the end of the current phase,
		/// the task is cancelled without generating an error.
		/// </summary>
		DropWithoutError = 17
	}

	/// <summary>
	/// Defines what happens to a task when it is interrupted at the end of the scenario.
	/// </summary>
	public enum ScenarioInterruptionPolicies : int
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// The task is discarded and will generate an error.
		/// </summary>
		DropWithError = 7,

		/// <summary>
		/// The task is discarded and will not generate an error.
		/// </summary>
		DropWithoutError = 17
	}

	/// <summary>
	/// Actions to undertake when a task is interrupted.
	/// </summary>
	[Flags]
	public enum InterruptionTypes
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		None,

		/// <summary>
		/// The task should wait.
		/// </summary>
		Delayed,

		/// <summary>
		/// The task should be reassigned.
		/// </summary>
		Reassigned,

		/// <summary>
		/// The interrupting task is killed.
		/// </summary>
		TaskIncomingEnd,

		/// <summary>
		/// The interruption of the task triggers the end of the phase.
		/// </summary>
		PhaseEnd,

		/// <summary>
		/// The interruption of the task triggers the end of the scenario.
		/// </summary>
		ScenarioEnd
	}

	/// <summary>
	/// Different types of standard tasks.
	/// Implementations inheriting from <see cref="SimulationTask"/> may have some additional types.
	/// It is suggested to use "accounting-like" codes where the first digit is the most significant,
	/// and other digits add precision.
	/// For instance, while 3 is <see cref="StandardTaskType.Rest"/>, more precise rests could have a code 31 or 32.
	/// </summary>
	public enum StandardTaskType : int
	{
		/// <summary>
		/// Not a task.
		/// </summary>
		Idle = 0,

		/// <summary>
		/// Rest (like sleep, holidays).
		/// </summary>
		Rest = 3,

		/// <summary>
		/// External conditions such as weather events or other perturbations.
		/// </summary>
		ExternalCondition = 4,	//TODO: Merge with ExternalCondition

		/// <summary>
		/// Used internally by <see cref="SimManning.Simulation.Simulator"/>
		/// to handle phases planned/minimal duration.
		/// </summary>
		InternalWait = 49,

		/// <summary>
		/// Important event such as accident, fire... typically triggering high-priority tasks.
		/// </summary>
		CriticalEvents = 5	//TODO: Merge with ExternalCondition
	}

	/// <summary>
	/// Implementation technicality specifying how a task is created when
	/// another task is given as a reference.
	/// This concerns only (some of) the data-structures (e.g. lists and other collections).
	/// </summary>
	public enum TaskLinkingType
	{
		/// <summary>
		/// Undefined and thus invalid.
		/// </summary>
		Undefined,

		/// <summary>
		/// The task shares the data-structures with the reference task.
		/// </summary>
		Linked,

		/// <summary>
		/// A copy of the data-structures of the reference task is made.
		/// </summary>
		Copy,

		/// <summary>
		/// The data-structures are not used and new empty ones are created instead.
		/// </summary>
		Clear
	}

	partial class SimulationTask
	{
		public static RelativeDateType ParseRelativeDateType(string value)
		{
			if (String.IsNullOrEmpty(value)) return RelativeDateType.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			if (value.StartsWith("RelativeStartFromPrevious", StringComparison.Ordinal) && ((value == "RelativeStartFromPreviousStartOccurrence") ||
				(value == "RelativeStartFromPreviousStartOccurence") || (value == "RelativeStartFromPreviousEndOccurence")))	//Mind the spelling Occurence/Occurrence.
				return RelativeDateType.Frequency;	//This is a hack due to changes of meaning in simulator implementation since earlier versions
			try
			{
				var result = (RelativeDateType)Enum.Parse(typeof(RelativeDateType), value, ignoreCase: true);
				return Enum.IsDefined(typeof(RelativeDateType), result) ? result : RelativeDateType.Undefined;
			}
			catch (ArgumentException) { return RelativeDateType.Undefined; }
			catch (OverflowException) { return RelativeDateType.Undefined; }
		}

		public static RelativeTimeType ParseRelativeTimeType(string value)
		{
			if (String.IsNullOrEmpty(value)) return RelativeTimeType.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (RelativeTimeType)Enum.Parse(typeof(RelativeTimeType), value, ignoreCase: true);
				return Enum.IsDefined(typeof(RelativeTimeType), result) ? result : RelativeTimeType.Undefined;
			}
			catch (ArgumentException) { return RelativeTimeType.Undefined; }
			catch (OverflowException) { return RelativeTimeType.Undefined; }
		}

		public static TaskInterruptionPolicies ParseTaskInterruptionPolicy(string value)
		{
			if (String.IsNullOrEmpty(value)) return TaskInterruptionPolicies.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (TaskInterruptionPolicies)Enum.Parse(typeof(TaskInterruptionPolicies), value, ignoreCase: true);
				return Enum.IsDefined(typeof(TaskInterruptionPolicies), result) ? result : TaskInterruptionPolicies.Undefined;
			}
			catch (ArgumentException) { return TaskInterruptionPolicies.Undefined; }
			catch (OverflowException) { return TaskInterruptionPolicies.Undefined; }
		}

		public static PhaseInterruptionPolicies ParsePhaseInterruptionPolicy(string value)
		{
			if (String.IsNullOrEmpty(value)) return PhaseInterruptionPolicies.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			if (value == "DontInterrupt") value = "DoNotInterrupt";	//This is a hack due to change of name
			else if (value == "ResumeAndComplete") return PhaseInterruptionPolicies.ResumeOrDropWithError;
			try
			{
				var result = (PhaseInterruptionPolicies)Enum.Parse(typeof(PhaseInterruptionPolicies), value, ignoreCase: true);
				return Enum.IsDefined(typeof(PhaseInterruptionPolicies), result) ? result : PhaseInterruptionPolicies.Undefined;
			}
			catch (ArgumentException) { return PhaseInterruptionPolicies.Undefined; }
			catch (OverflowException) { return PhaseInterruptionPolicies.Undefined; }
		}

		public static ScenarioInterruptionPolicies ParseScenarioInterruptionPolicy(string value)
		{
			if (String.IsNullOrEmpty(value)) return ScenarioInterruptionPolicies.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (ScenarioInterruptionPolicies)Enum.Parse(typeof(ScenarioInterruptionPolicies), value, ignoreCase: true);
				return Enum.IsDefined(typeof(ScenarioInterruptionPolicies), result) ? result : ScenarioInterruptionPolicies.Undefined;
			}
			catch (ArgumentException) { return ScenarioInterruptionPolicies.Undefined; }
			catch (OverflowException) { return ScenarioInterruptionPolicies.Undefined; }
		}

		public static InterruptionTypes ParseInterruptionTypes(string value)
		{
			if (String.IsNullOrEmpty(value)) return InterruptionTypes.None;
			try
			{
				var result = (InterruptionTypes)Enum.Parse(typeof(InterruptionTypes), value, ignoreCase: true);
				return Enum.IsDefined(typeof(InterruptionTypes), result) ? result : InterruptionTypes.None;
			}
			catch (ArgumentException) { return InterruptionTypes.None; }
			catch (OverflowException) { return InterruptionTypes.None; }
		}
	}
}
