using System;

namespace SimManning
{
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

		public static TaskDuplicatesPolicy ParseTaskDuplicatesPolicy(string value)
		{
			if (String.IsNullOrEmpty(value)) return TaskDuplicatesPolicy.Undefined;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (TaskDuplicatesPolicy)Enum.Parse(typeof(TaskDuplicatesPolicy), value, ignoreCase: true);
				return Enum.IsDefined(typeof(TaskDuplicatesPolicy), result) ? result : TaskDuplicatesPolicy.Undefined;
			}
			catch (ArgumentException) { return TaskDuplicatesPolicy.Undefined; }
			catch (OverflowException) { return TaskDuplicatesPolicy.Undefined; }
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
