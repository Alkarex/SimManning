using System;
using SimManning.IO;

namespace SimManning.Domain.Basic
{
	public sealed class BasicCreator : DomainCreator
	{
		/// <summary>
		/// Default global initialisation of static variables.
		/// </summary>
		static BasicCreator()
		{
			XmlIO.XmlDomain = "BasicDomain";
			XmlIO.XmlDomainVersion = "1.2";
		}

		public override Workplace CreateWorkplace(string name)
		{
			return new BasicWorkplace(name);
		}

		public override TaskDictionary CreateTaskDictionary()
		{
			return new BasicTaskDictionary();
		}

		public override Crew CreateCrew(string name, TaskDictionary taskDictionary)
		{
			return new BasicCrew(name, taskDictionary);
		}

		public override Phase CreatePhase(string name, TaskDictionary taskDictionary)
		{
			return new BasicPhase(name, taskDictionary);
		}

		public override Scenario CreateScenario(string name, Func<string, Phase> loadPhase)
		{
			return new BasicScenario(name, loadPhase);
		}

		public override SimulationDataSet CreateSimulationDataSet(Workplace workplace, TaskDictionary taskDictionary, Scenario scenario, Crew crew)
		{
			return new BasicSimulationDataSet(workplace, taskDictionary, scenario, crew);
		}

		public override SimulationDataSet CreateSimulationDataSet(string importedWorkplaceName, string importedScenarioName, string importedCrewName)
		{
			return new BasicSimulationDataSet(importedWorkplaceName, importedScenarioName, importedCrewName);
		}
	}
}
