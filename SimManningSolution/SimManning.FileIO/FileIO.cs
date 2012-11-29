using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SimManning.IO
{
	/// <summary>
	/// Extensions for XML IO on the file system.
	/// Separated because it uses some APIs not available in Windows 8 (Metro) and Silverlight.
	/// This class can safely be removed in projects not supporting <see cref="System.IO"/>.
	/// </summary>
	public static class FileIO
	{
		public const string WorkplaceFileName = "Workplace.xml";	//TODO: Fusion into tasklist
		public const string TasksFileName = "Tasks.xml";
		public const string PhaseFileNameExtension = ".phase.xml";
		public const string ScenarioFileNameExtension = ".scenario.xml";
		public const string CrewFileNameExtension = ".crew.xml";
		public const string DataSetFileNameExtension = ".dataset.xml";

		#region workplace.xml
		public static void LoadFromXml(this Workplace workplace, string fileName)
		{
			if (String.IsNullOrEmpty(fileName) && !File.Exists(fileName))
				throw new FileNotFoundException("Workplace file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				workplace.LoadFromXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		public static void SaveToXml(this Workplace workplace, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				workplace.SaveToXml(writer);
			}
		}
		#endregion

		#region tasks.xml
		public static void LoadFromXml(this TaskDictionary taskDictionary, string fileName)
		{
			if (String.IsNullOrEmpty(fileName) && !File.Exists(fileName))
				throw new FileNotFoundException("Tasks file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				taskDictionary.LoadFromXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		public static void SaveToXml(this TaskDictionary taskDictionary, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				taskDictionary.SaveToXml(writer);
			}
		}
		#endregion

		#region phase.xml
		public static void LoadFromXml(this Phase phase, string fileName, TaskDictionary taskDictionary)
		{
			if (String.IsNullOrEmpty(fileName) && !File.Exists(fileName))
				throw new FileNotFoundException("Phase file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				phase.LoadFromXml(XDocument.Load(reader, LoadOptions.None).Root, taskDictionary);
			}
		}

		public static void SaveToXml(this Phase phase, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				phase.SaveToXml(writer);
			}
		}
		#endregion

		#region scenario.xml
		public static void LoadFromXml(this Scenario scenario, string fileName)
		{
			if (String.IsNullOrEmpty(fileName) && !File.Exists(fileName))
				throw new FileNotFoundException("Scenario file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				scenario.LoadFromXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		public static void SaveToXml(this Scenario scenario, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				scenario.SaveToXml(writer);
			}
		}
		#endregion

		#region crew.xml
		public static void LoadFromXml(this Crew crew, string fileName)
		{
			if (String.IsNullOrEmpty(fileName) && !File.Exists(fileName))
				throw new FileNotFoundException("Crew file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				crew.LoadFromXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		public static void SaveToXml(this Crew crew, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				crew.SaveToXml(writer);
			}
		}
		#endregion

		#region SimulationDataSet
		public static SimulationDataSet LoadSimulationDataSetFromXml(this DomainCreator domainCreator, string path, string workplaceName, string scenarioName, string crewName)
		{
			path = path.TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
			if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Data path not found! [" + path + ']');
			var workplace = domainCreator.CreateWorkplace(workplaceName);
			workplace.LoadFromXml(path + FileIO.WorkplaceFileName);
			var taskList = domainCreator.CreateTaskDictionary();
			taskList.LoadFromXml(path + FileIO.TasksFileName);
			var crew = domainCreator.CreateCrew(crewName, taskList);
			crew.LoadFromXml(path + crewName + CrewFileNameExtension);
			var scenario = domainCreator.CreateScenario(scenarioName,
				phaseRefName =>
				{
					var myPath = path + phaseRefName + FileIO.PhaseFileNameExtension;
					if (File.Exists(myPath))
					{
						var phase = domainCreator.CreatePhase(phaseRefName, taskList);
						phase.LoadFromXml(myPath, taskList);
						return phase;
					}
					else return null;
				});
			scenario.LoadFromXml(path + scenarioName + FileIO.ScenarioFileNameExtension);
			return domainCreator.CreateSimulationDataSet(workplace, taskList, scenario, crew);
		}

		public static SimulationDataSet LoadSimulationDataSetFromSingleXmlFile(this DomainCreator domainCreator, string fileName)
		{
			if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName))
				throw new FileNotFoundException("Simulation dataSet file not found!", fileName);
			using (var reader = XmlReader.Create(fileName, XmlIO.SimManningXmlReaderSettings))
			{
				return domainCreator.LoadSimulationDataSetFromSingleXml(XDocument.Load(reader, LoadOptions.None).Root);
			}
		}

		public static void SaveToXml(this SimulationDataSet simulationDataSet, string path)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			simulationDataSet.Workplace.SaveToXml(path + FileIO.WorkplaceFileName);
			simulationDataSet.TaskDictionary.SaveToXml(path + FileIO.TasksFileName);
			simulationDataSet.Crew.SaveToXml(path + simulationDataSet.Crew.Name + CrewFileNameExtension);
			foreach (var phase in simulationDataSet.Scenario.Phases)
				phase.SaveToXml(path + phase.Name + FileIO.PhaseFileNameExtension);
			simulationDataSet.Scenario.SaveToXml(path + simulationDataSet.Scenario.Name + FileIO.ScenarioFileNameExtension);
		}

		public static void SaveToSingleXml(this SimulationDataSet simulationDataSet, string fileName)
		{
			using (var writer = XmlWriter.Create(fileName, XmlIO.SimManningXmlWriterSettings))
			{
				simulationDataSet.SaveToSingleXml(writer);
			}
		}
		#endregion
	}
}
