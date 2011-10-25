using System;
using System.Text;
using SimManning.Simulation;

namespace SimManning.Domain.Basic
{
	static class ProgramSimple
	{
		static void Main(string[] args)
		{
			Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;

			//DomainCreator is an object creating all other objects.
			//It is supposed to be inherited by custom domain implementations.
			//(Example here for a 'basic' domain).
			DomainCreator basicCreator = new BasicCreator();

			//SimulationDataSet is the central class for all simulation data input.
			//It is supposed to be inherited by custom domain implementations.
			//Here, loads the data from a single XML string provided on the standard input.
			Console.WriteLine("Load XML simulation data from standard input…");
			SimulationDataSet simulationDataSet =
				basicCreator.LoadSimulationDataSetFromSingleXmlString(Console.In);

			//Prepare the simulation input data:
			//For some tasks, automatically create one task instance per crewman.
			simulationDataSet.AutoExpandTasks();

			//A dispatcher contains the logic for task dispatch (assignment strategy).
			//It is supposed to be inherited by custom domain implementations.
			DomainDispatcher dispatcher = new BasicDispatcher(simulationDataSet);
			dispatcher.OnTaskAssignment += (simulationTime, phase, crewman, task) =>
			{//Event when a task is assigned to a crewman (task is null when ideling)
				Console.WriteLine("{0}\tCrewman “{1}” is assigned to the task “{2}”",
					simulationTime.ToStringUI(), crewman.Name,
					task == null ? "-" : task.Name);
				//Room to do something more with this event…
			};

			//The Simulator class is the simulation engine.
			//This one is _not_ supposed to be inherited.
			var simulator = new Simulator(simulationDataSet);
			simulator.OnErrorMessage += (text) => Console.Error.WriteLine(text);
			simulator.OnPhaseTransitionBegin += (phase, nextPhase, simulationTime) =>
			{//Event when starting a transition to the next phase of the scenario
				if (nextPhase == null) Console.WriteLine("{0}\tEnd of scenario.",
					simulationTime.ToStringUI());
				else Console.WriteLine("{0}\tStart phase “{1}”",
					simulationTime.ToStringUI(), nextPhase.Name);
				//Room to do something more with this event…
			};

			//Run one replication of the simulation.
			Console.WriteLine("Start simulation");
			if (!simulator.Run(timeOrigin: SimulationTime.Zero, domainDispatcher: dispatcher))
				Console.Error.WriteLine("Error while running simulation!");

			//Display some basic statistics.
			//It is up to implementers to gather more statistics.
			Console.WriteLine();
			Console.WriteLine("==== Example of statistics: cumulated work time ====");
			foreach (var crewman in simulationDataSet.Crew.Values)
				Console.WriteLine("Crewman “{0}”:\t{1}", crewman.Name,
					crewman.CumulatedWorkTime.ToStringUI());
		}
	}
}
