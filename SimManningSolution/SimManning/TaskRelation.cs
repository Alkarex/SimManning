using System;
using System.Collections.Generic;

namespace SimManning
{
	public struct TaskRelation : IComparable<TaskRelation>, IEquatable<TaskRelation>
	{
		public enum RelationType
		{
			None = 0,
			//IsStartedWhenBegin = -1,
			//IsStoppedWhenBegin = -2,
			//IsStartedWhenEnd = -5,
			//IsStoppedWhenEnd = -6,
			//StartWhenBegin = 1,	//Old name: ParentChild
			//StopWhenBegin = 2,	//Old name: Interrupt
			//StartWhenEnd = 5,
			//StopWhenEnd = 6,
			//Predecessor = 3,
			Parallel = 4,
			/// <summary>
			/// Task attached to n other "master" tasks (is created with the first master, and destroyed when the last master ends).
			/// </summary>
			Slave = 7,
			Master = -7,
			//NoDuplicate = 8	//Replaced by Task.NoDuplicate
		}

		readonly SimulationTask task1;

		public SimulationTask Task1
		{
			get { return this.task1; }
		}

		readonly RelationType relation;

		public RelationType Relation
		{
			get { return this.relation; }
		}

		readonly SimulationTask task2;

		public SimulationTask Task2
		{
			get { return task2; }
		}

		public TaskRelation(SimulationTask task1, RelationType relation, SimulationTask task2)
		{
			this.task1 = task1;
			this.relation = relation;
			this.task2 = task2;
		}

		public bool Valid
		{
			get { return String.IsNullOrEmpty(this.ErrorMessage); }
		}

		public string ErrorMessage
		{
			get
			{
				if (this.task1 == null) return "Reference task is null!";
				if (this.task2 == null) return "Reference affected task is null!";
				if (this.relation == RelationType.None) return "Relation type is undefined!";
				return String.Empty;
			}
		}

		public static RelationType ParseRelationType(string value)
		{
			if (String.IsNullOrEmpty(value)) return RelationType.None;
			var i = value.IndexOfAny(new char[] { ' ' });
			if (i > 0) value = value.Substring(0, i);
			try
			{
				var result = (RelationType)Enum.Parse(typeof(RelationType), value, ignoreCase: true);
				return Enum.IsDefined(typeof(RelationType), result) ? result : RelationType.None;
			}
			catch (ArgumentException) { return RelationType.None; }
			catch (OverflowException) { return RelationType.None; }
		}

		public override int GetHashCode()
		{
			return this.task1.Id ^ this.task2.Id ^ (int)this.relation;
		}

		public override bool Equals(object obj)
		{
			var b = obj as TaskRelation?;
			return b.HasValue && (this == b.Value);
		}

		public bool Equals(TaskRelation other)
		{
			return this == other;
		}

		#region Operators
		public static bool operator ==(TaskRelation relation1, TaskRelation relation2)
		{
			return (relation1.task1.Id == relation2.task1.Id) && (relation1.task2.Id == relation2.task2.Id) && (relation1.relation == relation2.relation);
		}

		public static bool operator !=(TaskRelation relation1, TaskRelation relation2)
		{
			return (relation1.task1.Id != relation2.task1.Id) || (relation1.task2.Id != relation2.task2.Id) || (relation1.relation != relation2.relation);
		}

		public static bool operator <(TaskRelation relation1, TaskRelation relation2)
		{
			if (relation1.task1.Id < relation2.task1.Id) return true;
			if (relation1.task2.Id < relation2.task2.Id) return true;
			if (relation1.relation < relation2.relation) return true;
			return false;
		}

		public static bool operator <=(TaskRelation relation1, TaskRelation relation2)
		{
			if (relation1.task1.Id < relation2.task1.Id) return true;
			if (relation1.task2.Id < relation2.task2.Id) return true;
			if (relation1.relation < relation2.relation) return true;
			return (relation1.task1.Id == relation2.task1.Id) && (relation1.task2.Id == relation2.task1.Id) && (relation1.relation == relation2.relation);
		}

		public static bool operator >(TaskRelation relation1, TaskRelation relation2)
		{
			if (relation1.task1.Id > relation2.task1.Id) return true;
			if (relation1.task2.Id > relation2.task2.Id) return true;
			if (relation1.relation > relation2.relation) return true;
			return false;
		}

		public static bool operator >=(TaskRelation relation1, TaskRelation relation2)
		{
			if (relation1.task1.Id > relation2.task1.Id) return true;
			if (relation1.task2.Id > relation2.task2.Id) return true;
			if (relation1.relation > relation2.relation) return true;
			return (relation1.task1.Id == relation2.task1.Id) && (relation1.task2.Id == relation2.task1.Id) && (relation1.relation == relation2.relation);
		}
		#endregion

		public int CompareTo(TaskRelation other)
		{
			if (this.task1.Id < other.task1.Id) return -3;
			if (this.task1.Id > other.task1.Id) return 3;
			if (this.task2.Id < other.task2.Id) return -2;
			if (this.task2.Id > other.task2.Id) return 2;
			if (this.relation < other.relation) return -1;
			if (this.relation > other.relation) return 1;
			return 0;
		}

		public sealed class SemanticsEqualityComparer : IEqualityComparer<TaskRelation>
		{//TODO: Try to remove
			/// <summary>
			/// Tests if two relations are semantically equal.
			/// </summary>
			/// <param name="a">A relation</param>
			/// <param name="b">Another relation</param>
			/// <returns>True if the the two relations are semantically equivalent, false otherwise.</returns>
			/// <remarks>
			/// All relations that a strictly equal are also semantically equal, but the reverse is not true.
			/// For instance, {Task1, parallel, Task2} is equivalent to {Task2, parallel, Task1}.
			/// </remarks>
			public bool Equals(TaskRelation x, TaskRelation y)
			{
				if (x.Relation != y.Relation) return false;
				if ((x.Relation == RelationType.Parallel) && (x.Task1.Id == y.Task2.Id) && (x.Task2.Id == y.Task1.Id)) return true;
				return (x.Task1.Id == y.Task1.Id) && (x.Task2.Id == y.Task2.Id);
			}

			public int GetHashCode(TaskRelation obj)
			{
				return obj.GetHashCode();
			}
		}
	}
}
