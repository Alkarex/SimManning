using System;
using System.Diagnostics;
using System.Globalization;
using SimManning.IO;
using SimManning.Simulation;
using System.Text;

namespace SimManning.Domain.Basic
{
	static class Program
	{
		const string Usage = @"Usage 1: <nbReplications> <path> <workplaceName> <scenarioName> <crewName>
Usage 2: <nbReplications> <Single-XML dataSet on standard input>";

		static void Main(string[] args)
		{
			#region Load data
			Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;
			int nbReplications;	//Number of replications

			//DomainCreator is an object creating all other objects.
			//It is supposed to be inherited by custom domain implementations.
			//(Example here for a 'basic' domain).
			DomainCreator basicCreator = new BasicCreator();

			//SimulationDataSet is the central class for all simulation data input.
			//It is supposed to be inherited by custom domain implementations.
			SimulationDataSet simulationDataSet;
			try
			{
				nbReplications = args.Length > 0 ? Int32.Parse(args[0]) : 1;
				if (args.Length <= 1)
				{//Usage 1
					Console.WriteLine("Load data from standard input...\t({0})",
						DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
					simulationDataSet = basicCreator.LoadSimulationDataSetFromSingleXmlString(Console.In);
				}
				else if (args.Length == 5)
				{//Usage 2
					Console.WriteLine("Load data from files...\t({0})",
						DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
					simulationDataSet = basicCreator.LoadSimulationDataSetFromXml(path: args[1], workplaceName: args[2],
						scenarioName: args[3], crewName: args[4]);
				}
				else throw new ArgumentException("Invalid number of arguments!");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				Console.Error.WriteLine();
				Console.Error.WriteLine(Usage);
				return;
			}
			#endregion

			#region Prepare simulation
			Console.WriteLine("Prepare simulation...\t({0})", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
			
			//Prepare the simulation input data:
			//For some tasks, automatically create one task instance per crewman.
			simulationDataSet.AutoExpandTasks();	//For some tasks, automatically create one task instance per crewman

			//A dispatcher contains the logic for task dispatch (assignment strategy).
			//It is possible to implement custom dispatchers.
			DomainDispatcher dispatcher = new BasicDispatcher(simulationDataSet);
			dispatcher.OnTaskAssignment += (simulationTime, phase, crewman, task) =>
			{//Event when a task is assigned to a crewman (task is null when ideling)
				Console.WriteLine("{0}\t{1}\t{2}\t{3}", simulationTime.ToStringUI(), phase, crewman,
					task == null ? "-" : task.ToString());
				//Room to do something more with this event...
			};

			//The Simulator class is the simulation engine.
			//This one is _not_ supposed to be inherited.
			var simulator = new Simulator(simulationDataSet);
			simulator.OnErrorMessage += (text) => Console.Error.WriteLine(text);
			simulator.OnPhaseTransitionBegin += (phase, nextPhase, currentSimulationTime) =>
			{//Event when starting a transition to the next phase of the scenario
				if (nextPhase == null) Console.WriteLine("{0}\t\tEnd of scenario.", currentSimulationTime.ToStringUI());
				else Console.WriteLine("{0}\t{1}...", currentSimulationTime.ToStringUI(), nextPhase);
				//Room to do something more with this event...
			};
			#endregion

			#region Run simulation
			var stopwatch = Stopwatch.StartNew();
			for (var n = 1; n <= nbReplications; n++)
			{
				Console.WriteLine("Start simulation #{0}:\t{1}, {2}, {3}...\t({4})",
					nbReplications, simulationDataSet.Workplace.Name, simulationDataSet.Crew.Name,
					simulationDataSet.Scenario.Name, DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
				if (!simulator.Run(timeOrigin: SimulationTime.Zero, domainDispatcher: dispatcher))
				{
					Console.Error.WriteLine("Error while running simulation {0}!", nbReplications);
					break;
				}
				Console.WriteLine();
			}
			stopwatch.Stop();
			#endregion

			#region Display some statistics
			Console.WriteLine("==== Example of statistics: cumulated work time ====");
			//Display some basic statistics. It is up to implementors to gather more statistics.
			foreach (var crewman in simulationDataSet.Crew.Values)
				Console.WriteLine("Crewman {0}:\t{1}", crewman, crewman.CumulatedWorkTime.ToStringUI());
			Console.WriteLine();
			#endregion

			#region Display some performance statistics
			Console.WriteLine("==== Performance ====");
			Console.WriteLine("{0}-bit process of '.NET {1}' on '{2}' running '{3}'", Environment.Is64BitProcess ? 64 : 32,
				Environment.Version, Environment.MachineName, Environment.OSVersion);
			Console.WriteLine("Simulation duration:\t{0}", stopwatch.Elapsed.ToString());
			var process = Process.GetCurrentProcess();
			Console.WriteLine("Total processor time:\t{0}", process.TotalProcessorTime);
			Console.WriteLine("Peak paged memory usage:\t{0}", process.PeakPagedMemorySize64);
			Console.WriteLine("End.\t({0})", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
			#endregion
		}
	}
}
