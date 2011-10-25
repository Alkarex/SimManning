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
		/// <summary>
		/// Create a new workplace.
		/// </summary>
		/// <param name="name">Name of the new workplace</param>
		/// <returns>The workplace</returns>
		public abstract Workplace CreateWorkplace(string name);

		/// <summary>
		/// Create a new task dictionnary.
		/// </summary>
		/// <returns>The task dictionnary</returns>
		public abstract TaskDictionary CreateTaskDictionary();

		/// <summary>
		/// Create a new phase.
		/// </summary>
		/// <param name="name">Name of the new phase</param>
		/// <param name="taskDictionary">An existing task dictionnary of the same domain</param>
		/// <returns>The new phase</returns>
		public abstract Phase CreatePhase(string name, TaskDictionary taskDictionary);

		/// <summary>
		/// Create a new scenario.
		/// </summary>
		/// <param name="name">Name of the new scenario</param>
		/// <param name="loadPhase">A delegate function capable of loading a phase given its name</param>
		/// <returns>A new scenario</returns>
		public abstract Scenario CreateScenario(string name, Func<string, Phase> loadPhase);

		/// <summary>
		/// Create a new crew.
		/// </summary>
		/// <param name="name">Name of the new crew</param>
		/// <param name="taskDictionary">An existing task dictionnary of the same domain</param>
		/// <returns></returns>
		public abstract Crew CreateCrew(string name, TaskDictionary taskDictionary);

		public abstract SimulationDataSet CreateSimulationDataSet(Workplace workplace, TaskDictionary taskDictionary, Scenario scenario, Crew crew);

		/// <summary>
		/// Constructor preparing a new empty dataSet.
		/// </summary>
		/// <param name="importedWorkplaceName">The name of the imported workplace</param>
		/// <param name="importedCrewName">The name of the imported crew</param>
		/// <param name="importedScenarioName">The name of the imported scenario</param>
		public abstract SimulationDataSet CreateSimulationDataSet(string importedWorkplaceName, string importedScenarioName, string importedCrewName);

		#region IO
		/// <summary>
		/// Load a crew from an XML serialisation.
		/// </summary>
		/// <param name="element">An XML element representing a complete simulation dataSet</param>
		/// <returns>The new simulation dataSet</returns>
		protected internal virtual SimulationDataSet LoadSimulationDataSetFromSingleXml(XElement element)
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
			var simulationDataSet = CreateSimulationDataSet(workplace, taskList, scenario, crew);
			var xmlAssertions = element.Element("Assertions");
			if (xmlAssertions != null)
				foreach (var xmlAssertion in xmlAssertions.Elements("Assertion"))
					if (!String.IsNullOrWhiteSpace(xmlAssertion.Value))
						simulationDataSet.Assertions.Add(xmlAssertion.Value);
			return simulationDataSet;
		}

		/// <summary>
		/// Load a crew from an XML serialisation.
		/// </summary>
		/// <param name="textReader">An XML text reader representing a complete simulation dataSet</param>
		/// <returns>The new simulation dataSet</returns>
		public SimulationDataSet LoadSimulationDataSetFromSingleXmlString(TextReader textReader)
		{
			using (var reader = XmlReader.Create(textReader, XmlIO.SimManningXmlReaderSettings))
			{
				return LoadSimulationDataSetFromSingleXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		/// <summary>
		/// Load a crew from an XML serialisation.
		/// </summary>
		/// <param name="xmlText">An XML string representing a complete simulation dataSet</param>
		/// <returns>The new simulation dataSet</returns>
		public SimulationDataSet LoadSimulationDataSetFromSingleXmlString(string xmlText)
		{
			if (String.IsNullOrEmpty(xmlText)) return null;
			return LoadSimulationDataSetFromSingleXmlString(new StringReader(xmlText));
		}
		#endregion
	}
}
