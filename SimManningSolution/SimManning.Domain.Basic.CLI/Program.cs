using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using SimManning.IO;
using SimManning.Simulation;

namespace SimManning.Domain.Basic
{
	sealed class Worker
	{
		int id;
		SimulationDataSet simulationDataSet;
		Simulator simulator;
		BasicDispatcher dispatcher;
		//MaritimeStatistics statistics;
		int nbReplications;
		int nbEventInsertions;

		public Worker(int id, string path, string workplaceName, string scenarioName, string crewName, int nbReplications)
		{
			this.id = id;
			Console.WriteLine("Thread {0}\tLoad data...\t({1})", this.id, DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
			var basicCreator = new BasicCreator();
			this.simulationDataSet = basicCreator.LoadSimulationDataSetFromXml(path, workplaceName, scenarioName, crewName);
			Console.WriteLine("Thread {0}\tBuild simulation...\t({1})", this.id, DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
			this.simulationDataSet.AutoExpandTasks();
			this.simulator = new Simulator(simulationDataSet);
			this.simulator.OnErrorMessage += (text) => Console.Error.WriteLine(text);
			this.dispatcher = new BasicDispatcher(simulationDataSet);
			this.dispatcher.OnTaskAssignment += Dispatcher_OnTaskAssignment;
			//this.statistics = new MaritimeStatistics(simulator, dispatcher, histogramResolution: new SimulationTime(TimeUnit.Hours, 1.0), subCodeLevel: 1, detailedLog: false);
			this.nbReplications = nbReplications;
		}

		void Dispatcher_OnTaskAssignment(SimulationTime simulationTime, Phase phase, Crewman crewman, SimulationTask task)
		{
			Console.WriteLine("Thread {0}\t{1}\t{2}\t{3}\t{4}", this.id, simulationTime.ToStringUI(), phase, crewman, task == null ? "-" : task.ToString());
		}

		public void Job()
		{
			var timeOrigin = new SimulationTime(TimeUnit.Hours, 0.0);
			this.simulator.OnPhaseTransitionBegin += (phase, nextPhase, currentSimulationTime) =>
			{
				if (nextPhase == null) Console.WriteLine("Thread {0}\t{1}\t\tEnd of scenario.", this.id, currentSimulationTime.ToStringUI());
				else Console.WriteLine("Thread {0}\t{1}\t{2}...", this.id, currentSimulationTime.ToStringUI(), nextPhase);
			};
			this.nbEventInsertions = 0;
			for (var n = nbReplications; n > 0; n--)
			{
				Console.WriteLine("Thread {0}\tStart simulation #{1}:\t{2}, {3}, {4}...\t({5})", this.id, 1 + nbReplications - n, simulationDataSet.Workplace.Name, simulationDataSet.Crew.Name, simulationDataSet.Scenario.Name, DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
				//this.statistics.PrepareForNextReplication();
				if (!this.simulator.Run(timeOrigin, dispatcher))
				{
					Console.Error.WriteLine("Thread {0}\tError while running simulation {1}!", this.id, 1 + nbReplications - n);
					break;
				}
				this.nbEventInsertions += simulator.NbEventInsertions;
				Console.WriteLine();
			}
		}

		public void Results()
		{
			Console.WriteLine("Thread {0}\t==== Results ====", this.id);
			Console.WriteLine("Total number of significant events:\t{0}", nbEventInsertions);
			Console.WriteLine();
			Console.WriteLine("== Phase types statistics ==");
			Console.WriteLine("Phase type ||        Sea Passage        ||         Transition        ||           Port");
			Console.WriteLine("Total dur. ||{0}||{1}||{2}", FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 1, useMean: false), FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 2, useMean: false), FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 3, useMean: false));
			Console.WriteLine("Mean dur.  ||{0}||{1}||{2}", FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 1, useMean: true), FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 2, useMean: true), FormatPhaseTypeDuration(simulationDataSet.PhaseTypes, 3, useMean: true));
			Console.WriteLine();
		}

		static string PadCenter(string text, int length)
		{
			return String.Format(CultureInfo.InvariantCulture,
				"{0}{1}{2}", String.Empty.PadLeft((length - text.Length) / 2), text, String.Empty.PadLeft((int)(Math.Ceiling((length - text.Length) / 2.0))));
		}

		static string FormatPhaseTypeDuration(IDictionary<int, PhaseType> phaseTypes, int phaseTypeId, bool useMean = true)
		{
			PhaseType phaseType;
			return PadCenter(phaseTypes.TryGetValue(phaseTypeId, out phaseType) ?
				(useMean ? phaseType.DurationMean : phaseType.TotalDuration).ToStringUI() :
				String.Empty, 27);
		}
	}

	sealed class Program
	{
		static void Usage()
		{
			Console.WriteLine("Usage: <path> <workplaceName> <scenarioName> <crewName> <nbReplications> <nbThreads=1>");
		}

		static void Main(string[] args)
		{
			Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;
			if ((args == null) || (args.Length < 5))
			{
				Usage();
				return;
			}
			else if (!Directory.Exists(args[0]))
			{
				Console.Error.WriteLine("Path not found! [{0}]", args[0]);
				return;
			}
			List<Worker> workers;
			List<Thread> threads;
			try
			{
				var nbReplications = Int32.Parse(args[4]);
				var nbThreads = 1;
				if (args.Length >= 6) Int32.TryParse(args[5], out nbThreads);
				workers = new List<Worker>(nbThreads);
				threads = new List<Thread>(nbThreads);
				for (var i = 0; i < nbThreads; i++)
				{
					workers.Add(new Worker(id: i, path: args[0], workplaceName: args[1], scenarioName: args[2], crewName: args[3], nbReplications: nbReplications));
					threads.Add(new Thread(workers[i].Job));
					threads[i].IsBackground = false;
					threads[i].Priority = ThreadPriority.AboveNormal;	//TODO: The application is not using all physical cores. Find out why
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				Usage();
				return;
			}

			#region Parallel execution
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i < threads.Count; i++) threads[i].Start();
			for (var i = 0; i < threads.Count; i++) threads[i].Join();
			stopwatch.Stop();
			#endregion

			for (var i = 0; i < workers.Count; i++)
				workers[i].Results();
			Console.WriteLine("==== Performance ====");
			Console.WriteLine("{0}-bit process of '.NET {1}' on '{2}' running '{3}'", Environment.Is64BitProcess ? 64 : 32, Environment.Version, Environment.MachineName, Environment.OSVersion);
			Console.WriteLine("Simulation duration:\t{0}", stopwatch.Elapsed.ToString());
			var process = Process.GetCurrentProcess();
			Console.WriteLine("Total processor time:\t{0}", process.TotalProcessorTime);
			Console.WriteLine("Peak paged memory usage:\t{0}", process.PeakPagedMemorySize64);
			Console.WriteLine("End.\t({0})", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
			#if (DEBUG)
			Console.WriteLine("Press return to terminate.");
			Console.Read();
			#endif
		}
	}
}
