using System;

namespace SimManning.Domain.Basic
{
	public class BasicTaskDictionary : TaskDictionary
	{
		public override SimulationTask CreateTask(int id)
		{
			return new BasicTask(id);
		}

		public override SimulationTask CreateTask(SimulationTask refTask, SimulationTask.LinkingType linkingType)
		{
			return new BasicTask(refTask, linkingType);
		}

		public override SimulationTask CreateTask(int id, SimulationTask refTask, SimulationTask.LinkingType linkingType)	//TODO: Make protected internal
		{
			return new BasicTask(id, refTask, linkingType);
		}
	}
}
