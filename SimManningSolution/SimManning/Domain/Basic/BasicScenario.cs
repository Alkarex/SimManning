using System;

namespace SimManning.Domain.Basic
{
	public class BasicScenario : Scenario
	{
		public BasicScenario(string name, Func<string, Phase> loadPhase) :
			base(name, loadPhase) { }

		protected override Phase CreatePhaseRef(Phase refPhase)
		{
			return new BasicPhase(refPhase);
		}
	}
}
