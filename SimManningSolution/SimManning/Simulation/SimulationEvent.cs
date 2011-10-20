using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SimManning.Simulation
{
	/*public abstract class SimulationEvent
	{
		public enum MainType
		{
			Undefined,

			PhaseEvent,

			TaskEvent
		}

		protected MainType eventType;

		public MainType EventMainType
		{
			get { return this.eventType; }
		}

		protected SimulationTime time;

		/// <summary>
		/// Time since t0 beginning of simulation, of this event.
		/// </summary>
		public SimulationTime EventTime
		{
			get { return this.time; }
			set { this.time = value; }
		}
	}

	public sealed class SimulationPhaseEvent : SimulationEvent
	{
		public enum Subtype
		{
			Undefined,

			PhaseEnds,

			PhaseArrives
		}

		Subtype subtype;

		public Subtype EventSubtype
		{
			get { return this.subtype; }
			set { this.subtype = value; }
		}

		readonly Phase phase;

		public Phase Phase
		{
			get { return this.phase; }
		}

		public SimulationPhaseEvent(Phase phase, SimulationTime time, Subtype eventSubtype)
		{
			base.eventType = MainType.PhaseEvent;
			//task.simulationEvent = this;
			this.phase = phase;
			base.time = time;
			this.subtype = eventSubtype;
		}

		public SimulationPhaseEvent(Phase phase, TimeSpan time, Subtype eventSubtype)
			: this(phase, SimulationTime.FromTimeSpan(time), eventSubtype) { }

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", base.time, this.subtype, this.phase);
		}
	}*/

	public sealed class SimulationTaskEvent : /*SimulationEvent,*/ IComparable<SimulationTaskEvent>
	{
		/// <summary>
		/// Types of task events, sorted by events that should be processed first.
		/// </summary>
		/// <remarks>
		/// The enumeration is sorted by events that should be processed first. This is an essential feature.
		/// This enum is manually coded as [Flags] (but not explicitelly as this would be semantically wrong),
		/// in order to have bitwise operations on e.g. TaskMetaStop.
		/// </remarks>
		public enum SubtypeType
		{
			Undefined = 0,

			/// <summary>
			/// Used for tasks that must be removed immediately from the system without completing.
			/// </summary>
			TaskKilled = 2,

			/// <summary>
			/// Used for aborting a task that was planned, before it starts.
			/// </summary>
			TaskCancelled = 4,

			/// <summary>
			/// Used for the natural end of a task after completion.
			/// </summary>
			TaskEnds = 8,	//The tasks that are ending are processed among the firsts

			/// <summary>
			/// Used to put aside some tasks that are not allowed in the current phase, but which will be used again in later phases.
			/// </summary>
			/// <remarks>Slaves are stopped during an hibernation</remarks>
			TaskHibernated = 16,

			/// <summary>
			/// Used when a task reaches the end of the daily hours during which it is allowed to be performed (e.g. from 10:00 to 16:00), or when hitting a Sunday and the task must wait until Monday.
			/// </summary>
			TaskAdjourned = 32,

			/// <summary>
			/// Used when the currently assigned crewmember(s) are given a chance to leave the task to work on something else (or rest).
			/// </summary>
			TaskWorkInterrupted = 64,

			/// <summary>
			/// After the work has been interrupted, the task continues and the system attempts to assign it to some other or identical crewmember(s).
			/// </summary>
			TaskWorkContinues = 128,

			/// <summary>
			/// Used to resume a task that was adjourned.
			/// </summary>
			TaskResumes = 256,

			/// <summary>
			/// Used to resume a task that was hibernated.
			/// </summary>
			TaskAwakes = 512,

			TaskArrives = 1024,

			/// <summary>
			/// Used to set an occurrence of a task to start at some precise time in the future.
			/// </summary>
			TaskPlanned = 2048,	//The tasks that are planned are processed last

			/// <summary>
			/// Used to set an occurrence of a task to start when the next phase arrives.
			/// </summary>
			TaskForNextPhase = 4096,

			TaskMetaStart = TaskArrives | TaskAwakes | TaskPlanned | TaskResumes | TaskWorkContinues,

			TaskMetaStop = TaskAdjourned | TaskCancelled | TaskEnds | TaskHibernated | TaskKilled | TaskWorkInterrupted,

			TaskMetaNotStarted = TaskPlanned | TaskForNextPhase
		}

		public SubtypeType subtype;	//Hot path

		/*public Subtype subType
		{
			get { return this.subType; }
			set { this.subType = value; }
		}*/

		readonly SimulationTask task;

		public SimulationTask Task
		{
			get { return this.task; }
		}

		SimulationTime time;

		/// <summary>
		/// Time since t0 beginning of simulation, of this event.
		/// </summary>
		public SimulationTime EventTime
		{
			get { return this.time; }
			set { this.time = value; }
		}

		public SimulationTaskEvent(SimulationTask task, SimulationTime time, SubtypeType eventSubtype)
		{
			//base.eventType = MainType.TaskEvent;
			this.task = task;
			this.time = time;
			this.subtype = eventSubtype;
		}

		public SimulationTaskEvent(SimulationTask task, TimeSpan time, SubtypeType eventSubtype)
			: this(task, SimulationTime.FromTimeSpan(time), eventSubtype) { }

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", this.time, this.subtype, this.task);
		}

		/// <remarks>
		/// Very central function that governs the order of events in the queue of events.
		/// </remarks>
		/// <param name="other">Non-null other SimulationTaskEvent</param>
		public int CompareTo(SimulationTaskEvent other)
		{//Hot path function #1!
			if (!SimManningCommon.InternalEngineAllowed) throw new NotImplementedException(SimManningCommon.ErrorMessageInternalEngineNotAllowed);
			//if (Object.ReferenceEquals(other, null)) return -9;
			Debug.Assert(!Object.ReferenceEquals(other, null), "Simulation events must not be null!");
			{//int result = base.time.CompareTo(other.time);	//Too expensive, so inlining below
				var time1 = this.time.internalTime;
				var time2 = other.time.internalTime;
				if (time1 < time2) return -8 * Simulator.EventOrderSign;
				if (time1 > time2) return 8 * Simulator.EventOrderSign;
			}
			{//The order of types in this Enum is crucial (e.g. tasks ending should be processed before tasks starting)
				var type1 = this.subtype;
				var type2 = other.subtype;
				if (type1 < type2) return -7 * Simulator.EventOrderSign;
				if (type1 > type2) return 7 * Simulator.EventOrderSign;
			}
			var otherTask = other.task;
			Debug.Assert((!Object.ReferenceEquals(this.task, null)) && (!Object.ReferenceEquals(otherTask, null)), "Tasks of simulation events must not be null!");
			//if (this.task == null) return otherTask == null ? 0 : -6 * Simulator.EventOrderSign;
			//if (otherTask == null) return 6 * Simulator.EventOrderSign;
			{
				var t1p = this.task.Priority;
				var t2p = otherTask.Priority;
				if (t1p < t2p) return 5 * Simulator.EventOrderSign;	//Task with higher priority first
				if (t1p > t2p) return -5 * Simulator.EventOrderSign;
			}
			{
				var t1n = this.task.NumberOfCrewmenNeeded;
				var t2n = otherTask.NumberOfCrewmenNeeded;
				if (t1n < t2n) return 4 * Simulator.EventOrderSign;	//Task with higher number of resources needed first
				if (t1n > t2n) return -4 * Simulator.EventOrderSign;
			}
			{
				var t1pp = this.task.ParallelTasks.Count;
				var t2pp = otherTask.ParallelTasks.Count;
				if (t1pp < t2pp) return 3 * Simulator.EventOrderSign;	//Task with higher number of parallel tasks first
				if (t1pp > t2pp) return -3 * Simulator.EventOrderSign;
			}
			{
				var t1Id = this.task.Id;
				var t2Id = otherTask.Id;
				if (t1Id < t2Id) return -2 * Simulator.EventOrderSign;
				if (t1Id > t2Id) return 2 * Simulator.EventOrderSign;
			}
			{
				var timeArrive1 = this.task.SimulationTimeArrives;
				var timeArrive2 = otherTask.SimulationTimeArrives;
				if (timeArrive1 < timeArrive2) return -1 * Simulator.EventOrderSign;	//Oldest tasks first
				if (timeArrive1 > timeArrive2) return 1 * Simulator.EventOrderSign;
			}
			//if (result == 0) result = this.GetHashCode().CompareTo(other.GetHashCode());	//Too time consuming for added value
			return 0;
		}

		/*public override int GetHashCode()
		{
			return this.time.GetHashCode() ^ this.subType.GetHashCode() ^ this.task.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as SimulationTaskEvent;
			return (!Object.ReferenceEquals(other, null)) && (this == other);
		}

		public bool Equals(SimulationTaskEvent other)
		{
			return this == other;
		}*/

		#region Operators
		/*public static bool operator ==(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return true;
			return (!Object.ReferenceEquals(simulationEvent1, null)) && (simulationEvent1.CompareTo(simulationEvent2) == 0);
		}

		public static bool operator !=(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return false;
			return Object.ReferenceEquals(simulationEvent1, null) || (simulationEvent1.CompareTo(simulationEvent2) != 0);
		}*/

		/*public static bool operator <(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return false;
			if (Object.ReferenceEquals(simulationEvent1, null)) return false;
			return simulationEvent1.CompareTo(simulationEvent2) < 0;
		}

		public static bool operator <=(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return true;
			if (Object.ReferenceEquals(simulationEvent1, null)) return true;
			return simulationEvent1.CompareTo(simulationEvent2) <= 0;
		}

		public static bool operator >(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return false;
			if (Object.ReferenceEquals(simulationEvent1, null)) return false;
			return simulationEvent1.CompareTo(simulationEvent2) >= 0;
		}

		public static bool operator >=(SimulationTaskEvent simulationEvent1, SimulationTaskEvent simulationEvent2)
		{
			if (Object.ReferenceEquals(simulationEvent1, simulationEvent2)) return true;
			if (Object.ReferenceEquals(simulationEvent1, null)) return Object.ReferenceEquals(simulationEvent2, null);
			return simulationEvent1.CompareTo(simulationEvent2) >= 0;
		}*/
		#endregion
	}

	public sealed class SimulationEventAsapComparer : IComparer<SimulationTaskEvent>
	{
		const int sign = -1;

		public int Compare(SimulationTaskEvent x, SimulationTaskEvent y)
		{
			Debug.Assert((x != null) && (y != null), "Simulation events must not be null!");
			var taskA = x.Task;
			var taskB = y.Task;
			Debug.Assert((taskA != null) && (taskB != null), "Tasks of simulation events must not be null!");
			//if (taskA == null) return taskB == null ? 0 : -6 * sign;
			//if (taskB == null) return 6 * sign;
			{
				var t1p = taskA.Priority;
				var t2p = taskB.Priority;
				if (t1p < t2p) return 5 * sign;	//Task with higher priority first
				if (t1p > t2p) return -5 * sign;
			}
			{
				var t1n = taskA.NumberOfCrewmenNeeded;
				var t2n = taskB.NumberOfCrewmenNeeded;
				if (t1n < t2n) return 4 * sign;	//Task with higher number of resources needed first
				if (t1n > t2n) return -4 * sign;
			}
			{
				var t1pp = taskA.ParallelTasks.Count;
				var t2pp = taskB.ParallelTasks.Count;
				if (t1pp < t2pp) return 3 * sign;	//Task with higher number of parallel tasks first
				if (t1pp > t2pp) return -3 * sign;
			}
			{
				var t1Id = taskA.Id;
				var t2Id = taskB.Id;
				if (t1Id < t2Id) return -2 * sign;
				if (t1Id > t2Id) return 2 * sign;
			}
			{
				var timeArrive1 = taskA.SimulationTimeArrives;
				var timeArrive2 = taskB.SimulationTimeArrives;
				if (timeArrive1 < timeArrive2) return -1 * sign;	//Oldest tasks first
				if (timeArrive1 > timeArrive2) return 1 * sign;
			}
			return 0;
		}

		public static readonly SimulationEventAsapComparer Instance = new SimulationEventAsapComparer();
	}
}
