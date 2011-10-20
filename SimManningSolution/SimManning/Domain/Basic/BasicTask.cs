using System;

namespace SimManning.Domain.Basic
{
	public class BasicTask : SimulationTask
	{
		public BasicTask(int id) : base(id) { }

		public BasicTask(SimulationTask refTask, LinkingType linkingType = LinkingType.Linked) : base(refTask, linkingType) { }

		internal BasicTask(int id, SimulationTask refTask, LinkingType linkingType = LinkingType.Linked) : base(id, refTask, linkingType) { }
	}
}
