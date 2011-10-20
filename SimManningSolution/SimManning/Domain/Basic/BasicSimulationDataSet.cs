using System;

namespace SimManning.Domain.Basic
{
	public class BasicSimulationDataSet : SimulationDataSet
	{
		public BasicSimulationDataSet(Workplace workplace, TaskDictionary taskDictionary, Scenario scenario, Crew crew) :
			base(workplace, taskDictionary, scenario, crew)
		{
			if (base.Workplace == null)
				base.Workplace = new BasicWorkplace("DefaultWorkplace");
			if (base.Crew == null)
				base.Crew = new BasicCrew("tempCrew", taskDictionary ?? new BasicTaskDictionary());
			if (base.TaskDictionary == null)
				base.TaskDictionary = base.Crew.TaskDictionary ?? new BasicTaskDictionary();
			if (base.Scenario == null)
				base.Scenario = new BasicScenario("tempScenario", loadPhase: null);
			base.TaskDictionaryExpanded = new BasicTaskDictionary();
		}

		public BasicSimulationDataSet(string importedWorkplaceName, string importedScenarioName, string importedCrewName) :
			base(new BasicWorkplace(importedWorkplaceName), null,
			new BasicScenario(importedScenarioName, null),
			new BasicCrew(importedCrewName, new BasicTaskDictionary()))
		{
			base.TaskDictionaryExpanded = new BasicTaskDictionary();
		}
	}
}
