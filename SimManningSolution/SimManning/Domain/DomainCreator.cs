using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;

namespace SimManning
{
	/// <summary>
	/// Creator (also called Factory) base class to generate specific decendent objects for a given domain.
	/// </summary>
	public abstract class DomainCreator
	{
		public abstract Workplace CreateWorkplace(string name);

		public abstract TaskDictionary CreateTaskDictionary();

		public abstract Phase CreatePhase(string name, TaskDictionary taskDictionary);

		public abstract Scenario CreateScenario(string name, Func<string, Phase> loadPhase);

		public abstract Crew CreateCrew(string name, TaskDictionary taskDictionary);

		public abstract SimulationDataSet CreateSimulationDataSetManning(Workplace workplace, TaskDictionary taskDictionary, Scenario scenario, Crew crew);

		/// <summary>
		/// Constructor preparing a new empty dataSet.
		/// </summary>
		/// <param name="importedWorkplaceName">The name of the imported workplace</param>
		/// <param name="importedCrewName">The name of the imported crew</param>
		/// <param name="importedScenarioName">The name of the imported scenario</param>
		public abstract SimulationDataSet CreateSimulationDataSetManning(string importedWorkplaceName, string importedScenarioName, string importedCrewName);

		#region IO
		protected internal virtual SimulationDataSet LoadSimulationDataSetManningFromSingleXml(XElement element)
		{
			var myElement = element.Element("Workplace");
			if (myElement == null) return null;
			var workplace = CreateWorkplace(myElement.Attribute("name").Value);
			workplace.LoadFromXml(myElement);
			myElement = element.Element("Tasks");
			if (myElement == null) return null;
			var taskList = CreateTaskDictionary();
			taskList.LoadFromXml(myElement);
			myElement = element.Element("Crew");
			if (myElement == null) return null;
			var crew = CreateCrew(myElement.Attribute("name").Value, taskList);
			crew.LoadFromXml(myElement);
			myElement = element.Element("Scenario");
			if (myElement == null) return null;
			var scenario = CreateScenario(myElement.Attribute("name").Value,
				phaseRefName =>
				{
					var xmlPhase = element.Elements("Phase").FirstOrDefault(xe => xe.Attribute("name").Value == phaseRefName);
					if (xmlPhase == null) return null;
					var phase = CreatePhase(xmlPhase.Attribute("name").Value, taskList);
					phase.LoadFromXml(xmlPhase, taskList);
					return phase;
				});
			scenario.LoadFromXml(myElement);
			var simulationDataSet = CreateSimulationDataSetManning(workplace, taskList, scenario, crew);
			var xmlAssertions = element.Element("Assertions");
			if (xmlAssertions != null)
				foreach (var xmlAssertion in xmlAssertions.Elements("Assertion"))
					if (!String.IsNullOrWhiteSpace(xmlAssertion.Value))
						simulationDataSet.Assertions.Add(xmlAssertion.Value);
			return simulationDataSet;
		}

		public virtual SimulationDataSet LoadSimulationDataSetManningFromSingleXml(string xmlText)
		{
			if (!String.IsNullOrEmpty(xmlText))
			{
				var textReader = new StringReader(xmlText);
				using (var reader = XmlReader.Create(textReader, XmlIO.SimManningXmlReaderSettings))
				{
					return LoadSimulationDataSetManningFromSingleXml(XDocument.Load(reader, LoadOptions.None).Root);
				}
			}
			return null;
		}
		#endregion
	}
}
