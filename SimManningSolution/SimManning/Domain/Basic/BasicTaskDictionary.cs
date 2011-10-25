using System;

namespace SimManning.Domain.Basic
{
	public class BasicTaskDictionary : TaskDictionary
	{
		public override SimulationTask CreateTask(int id)
		{
			return new BasicTask(id);
		}

		public override SimulationTask CreateTask(SimulationTask refTask, TaskLinkingType linkingType)
		{
			return new BasicTask(refTask, linkingType);
		}

		protected internal override SimulationTask CreateTask(int id, SimulationTask refTask, TaskLinkingType linkingType)
		{
			return new BasicTask(id, refTask, linkingType);
		}
	}
}
