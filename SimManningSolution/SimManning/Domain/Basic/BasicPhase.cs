using System;

namespace SimManning.Domain.Basic
{
	public class BasicPhase : Phase
	{
		public BasicPhase(string name, TaskDictionary taskDictionary) :
			base(name)
		{
			base.Tasks = new BasicTaskDictionary();
		}

		public BasicPhase(Phase refPhase) : base(refPhase) { }
	}
}
