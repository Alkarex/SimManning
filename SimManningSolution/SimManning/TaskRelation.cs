using System;
using System.Collections.Generic;

namespace SimManning
{
	/// <summary>
	/// A relationship between two tasks (<see cref="SimulationTask"/>).
	/// The different possible relation types are defined in <see cref="RelationType"/>.
	/// </summary>
	public struct TaskRelation : IComparable<TaskRelation>, IEquatable<TaskRelation>
	{
		/// <summary>
		/// The different possible relation types between two <see cref="SimulationTask"/>s.
		/// </summary>
		public enum RelationType
		{
			/// <summary>
			/// No relation between the two tasks. This is invalid.
			/// In case of no relation between two tasks, simply remove this <see cref="TaskRelation"/> completely.
			/// </summary>
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

			/// <summary>
			/// The two tasks are to be executed only in parallel,
			/// meaning that one task must wait for the other before starting,
			/// and be interrupted if the other is interrupted.
			/// </summary>
			Parallel = 4,

			/// <summary>
			/// The first task is a slave of the second task,
			/// meaning that when the second task (<see cref="Master"/>) starts,
			/// the first task starts if not already started by another master.
			/// When the second task stops, the first task will stop if it does not have another active master.
			/// </summary>
			Slave = 7,

			/// <summary>
			/// The first task is a master of the second task,
			/// meaning that when the first task starts,
			/// the second task (<see cref="Slave"/>) starts if not already started by another master.
			/// When the first task stops, the second task will stop if it does not have another active master.
			/// </summary>
			Master = -7,

			//NoDuplicate = 8	//Reflexive (task 1 = task 2). Replaced by Task.NoDuplicate
		}

		readonly SimulationTask task1;

		/// <summary>
		/// The first task of the relation.
		/// </summary>
		public SimulationTask Task1
		{
			get { return this.task1; }
		}

		readonly RelationType relation;

		/// <summary>
		/// The type of relation between the two tasks (e.g. <see cref="RelationType.Parallel"/>).
		/// </summary>
		public RelationType Relation
		{
			get { return this.relation; }
		}

		readonly SimulationTask task2;

		/// <summary>
		/// The second task of the relation.
		/// </summary>
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

		/// <summary>
		/// Return true if the is no error in <see cref="ErrorMessage"/>, false otherwise.
		/// </summary>
		public bool Valid
		{
			get { return String.IsNullOrEmpty(this.ErrorMessage); }
		}

		/// <summary>
		/// Reports if the relation is valid or not:
		/// both taks and the relation must be non-null.
		/// </summary>
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
	}
}
