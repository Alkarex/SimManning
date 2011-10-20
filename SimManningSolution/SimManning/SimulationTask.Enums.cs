using System;
using System.Collections.Generic;

namespace SimManning
{
	partial class SimulationTask
	{
		public enum RelativeDateType
		{
			//AbsoluteStopMonthDay = -13,
			//AbsoluteStopWeekDay = -12,
			//RelativeStopFromPreviousStopOccurrence = -4,
			//RelativeStopFromPreviousStartOccurrence = -3,
			RelativeStopFromEndOfPhase = -2,
			//RelativeStopFromStartOfPhase = -1,
			Undefined = 0,
			RelativeStartFromStartOfPhase = 1,
			RelativeStartFromEndOfPhase = 2,
			Frequency = 3,
			//RelativeStartFromPreviousStopOccurrence = 4,
			RelativeStartFromPreviousStart = 5,
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

		public static RelativeDateType ParseRelativeDateType(string value)
		{
			if (String.IsNullOrEmpty(value)) return RelativeDateType.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			if (value.StartsWith("RelativeStartFromPrevious", StringComparison.Ordinal) && ((value == "RelativeStartFromPreviousStartOccurrence") ||
				(value == "RelativeStartFromPreviousStartOccurence") || (value == "RelativeStartFromPreviousEndOccurence")))	//Mind the spelling Occurence/Occurrence.
				return RelativeDateType.Frequency;	//This is a hack due to changes of meaning in simulator implementation
			try
			{
				var result = (RelativeDateType)Enum.Parse(typeof(RelativeDateType), value, ignoreCase: true);
				return Enum.IsDefined(typeof(RelativeDateType), result) ? result : RelativeDateType.Undefined;
			}
			catch (ArgumentException) { return RelativeDateType.Undefined; }
			catch (OverflowException) { return RelativeDateType.Undefined; }
		}

		public enum RelativeTimeType
		{
			/// <summary>
			/// Tasks with an undefined RelativeTimeType are invalid.
			/// </summary>
			Undefined = 0,
			AbsoluteStopTime = -1,
			AbsoluteStartTime = 1,
			TimeWindow = 3
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

		/// <summary>
		/// Defines what happens to a task when it is interrupted during another task.
		/// </summary>
		public enum TaskInterruptionPolicies : int
		{
			/// <summary>
			/// Unspecified behaviour. Such a task is invalid.
			/// </summary>
			Undefined = 0,
			DropWithError = 2,
			DropWithoutError = 4,
			/// <summary>
			/// Must be re-assigned immediatly to another qualified crewmember, otherwise cancelled with error.
			/// </summary>
			ContinueOrDropWithError = 8,
			/// <summary>
			/// Must be re-assigned immediatly to another qualified crewmember, otherwise cancelled without error.
			/// </summary>
			ContinueOrDropWithoutError = 16,
			ContinueOrResumeWithError = 32,
			/// <summary>
			/// Most common behaviour.
			/// </summary>
			ContinueOrResumeWithoutError = 64,
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
			/// Unspecified behaviour. Such a task is invalid.
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

		/// <summary>
		/// Defines what happens to a task when it is interrupted at the end of the scenario.
		/// </summary>
		public enum ScenarioInterruptionPolicies : int
		{
			/// <summary>
			/// Unspecified behaviour. Such a task is invalid.
			/// </summary>
			Undefined = 0,
			DropWithError = 7,
			DropWithoutError = 17
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

		[Flags]
		public enum InterruptionTypes
		{
			None,	//Undefined
			Delayed,
			Reassigned,
			TaskInterruption,
			PhaseInterruption,
			ScenarioEnd
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

		public enum StandardType : int
		{
			Idle = 0,
			Rest = 3,
			ExternalCondition = 4,
			InternalWait = 49,
			CriticalEvents = 5
		}

		public enum LinkingType
		{
			Undefined,
			Linked,
			Copy,
			Clear
		}

		public sealed class IdEqualityComparer : IEqualityComparer<SimulationTask>
		{//TODO: Try to remove
			/// <summary>
			/// Tests if two tasks have the same ID.
			/// </summary>
			public bool Equals(SimulationTask x, SimulationTask y)
			{
				return ((x != null) && (y != null) && (x.id == y.id));
			}

			public int GetHashCode(SimulationTask obj)
			{
				return obj.GetHashCode();
			}
		}
	}
}
