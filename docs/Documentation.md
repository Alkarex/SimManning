## Documentation of the _SimManning_ library for discrete-event simulation of manning / staffing

The library contains:
* Some abstract classes to be inherited for implementing custom domains / applications:
	* Data structures defining tasks, task relations, scenarios with multiple phases, crews with several crewmen and their qualifications, etc.
	* A dispatcher to contain task assignment strategies.
* An simple example of implementation, called _Basic_, of a custom domain / application.
	* This is meant to be used as a starting point for implementing other domains.
* A discrete-event simulation engine using the above.
	* This is the most valuable class doing most of the work.
	* By default, it does not perform scheduling optimisation, but runs a fast discrete-event simulation largely governed by the dispatcher.

The source-code is written with performance and maintainability in mind. The global Maintainability Index is 85/100, and always higher than 30 for any sub-function. Some efforts have also been made for documenting the code and providing examples.

### Target group
This project is intended to be used by software engineers / programmers seeking to implement a new model of human activity during scenarios possibly involving several phases, in order to become a tool for operation management like helping decisions regarding staffing / manning.

### How to get started
# Run the demo program (SimManning.Domain.Basic.CLI) and play with the XML input data.
# See the the _Basic_ domain example in the source code: SimManningSolution/SimManning/Domain/Basic.
	* Implementers will need to do something very similar for their own domain implementation.
# Look at the [class diagram](Documentation_SimManning-ClassDiagram-v1.3.png).
# Browse the [API documentation](Documentation_SimManningDocumentation-v1.3.chm) (CHM format, but can be compiled to other formats).
	* (If you download this documentation with Internet Explorer on Windows, you may have to right-click the file and chose "unblock" before being able to read it)

### Example of input data
Most of the simulation is data-driven with very little hard-coded in the discrete-event simulation engine.
The input data can be provided to the simulation library as XML, either as once single document (as exemplified below), or several files, which are useful when working with combinations of several crews and scenarios. Implementers have the possibility to use their own data sources, and/or to override the XML input/output to add custom fields.

{code:XML}
<DataSet domain="BasicDomain" version="1.3">
 <Workplace name="Basic example">
  <description>A basic generic example with a 2-people crew and 4 tasks with 2 in parallel.</description>
 </Workplace>
 <Tasks>
  <Task id="1" name="Task 1" taskType="24" autoAssignToAllCrewMembers="False" relativeDate="RelativeStartFromStartOfPhase" startDateUnit="Days" startDateMin="0" startDateMean="0" startDateMax="0" relativeTime="TimeWindow" workingHourStart="0" workingHourEnd="0" onHolidays="True" taskDurationUnit="Hours" taskDurationMin="4" taskDurationMean="4" taskDurationMax="4" taskInterruptionPolicy="ContinueOrResumeWithoutError" phaseInterruptionPolicy="ResumeOrDropWithError" scenarioInterruptionPolicy="DropWithoutError" priority="200" numberOfCrewMembersNeeded="1" rotation="0" taskDuplicatesPolicy="KillOldDuplicates">
   <phaseTypes>
    <PhaseTypeRef refCode="1" />
   </phaseTypes>
   <crewMemberTypes />
   <taskRelations>
    <TaskRef rel="Parallel" refId="2" />
   </taskRelations>
  </Task>
  <Task id="2" name="Task 2" taskType="23" autoAssignToAllCrewMembers="False" relativeDate="RelativeStartFromStartOfPhase" startDateUnit="Days" startDateMin="0" startDateMean="0" startDateMax="0" relativeTime="TimeWindow" workingHourStart="0" workingHourEnd="0" onHolidays="True" taskDurationUnit="Hours" taskDurationMin="4" taskDurationMean="4" taskDurationMax="4" taskInterruptionPolicy="ContinueOrResumeWithoutError" phaseInterruptionPolicy="ResumeOrDropWithError" scenarioInterruptionPolicy="DropWithoutError" priority="200" numberOfCrewMembersNeeded="1" rotation="0" taskDuplicatesPolicy="KillOldDuplicates">
   <phaseTypes>
    <PhaseTypeRef refCode="1" />
   </phaseTypes>
   <crewMemberTypes />
   <taskRelations>
    <TaskRef rel="Parallel" refId="1" />
   </taskRelations>
  </Task>
  <Task id="3" name="Task 3" taskType="101" autoAssignToAllCrewMembers="False" relativeDate="RelativeStartFromStartOfPhase" startDateUnit="Days" startDateMin="0" startDateMean="0" startDateMax="0" relativeTime="TimeWindow" workingHourStart="0" workingHourEnd="0" onHolidays="True" taskDurationUnit="Hours" taskDurationMin="2" taskDurationMean="2" taskDurationMax="2" taskInterruptionPolicy="ContinueOrResumeWithoutError" phaseInterruptionPolicy="DropWithError" scenarioInterruptionPolicy="DropWithoutError" priority="500" numberOfCrewMembersNeeded="1" rotation="0" taskDuplicatesPolicy="KillOldDuplicates">
   <phaseTypes>
    <PhaseTypeRef refCode="1" />
   </phaseTypes>
   <crewMemberTypes />
   <taskRelations />
  </Task>
  <Task id="4" name="Task 4" taskType="101" autoAssignToAllCrewMembers="False" relativeDate="RelativeStartFromStartOfPhase" startDateUnit="Hours" startDateMin="6" startDateMean="6" startDateMax="6" relativeTime="TimeWindow" workingHourStart="0" workingHourEnd="0" onHolidays="True" taskDurationUnit="Hours" taskDurationMin="1" taskDurationMean="1" taskDurationMax="1" taskInterruptionPolicy="ContinueOrResumeWithoutError" phaseInterruptionPolicy="DropWithError" scenarioInterruptionPolicy="DropWithoutError" priority="500" numberOfCrewMembersNeeded="1" rotation="0" taskDuplicatesPolicy="KillOldDuplicates">
   <phaseTypes>
    <PhaseTypeRef refCode="1" />
   </phaseTypes>
   <crewMemberTypes />
   <taskRelations />
  </Task>
 </Tasks>
 <Phase name="Phase1" phaseId="1" phaseType="1" phaseDurationUnit="Hours"
  phaseDurationMin="7" phaseDurationMean="7" phaseDurationMax="7">
  <description>A basic phase of type "1"</description>
 </Phase>
 <Phase name="Phase2" phaseId="2" phaseType="3" phaseDurationUnit="Hours"
  phaseDurationMin="1" phaseDurationMean="1" phaseDurationMax="1">
  <description>A basic phase of type "2"</description>
 </Phase>
 <Phase name="Phase3" phaseId="3" phaseType="1" phaseDurationUnit="Hours"
  phaseDurationMin="7" phaseDurationMean="7" phaseDurationMax="7">
  <description>A basic phase of type "1" again</description>
 </Phase>
 <Scenario name="Basic scenario 1">
  <description>A basic scenario with 3 phases</description>
  <PhaseRef refName="Phase1" />
  <PhaseRef refName="Phase2" />
  <PhaseRef refName="Phase3" />
 </Scenario>
 <Crew name="Basic crew 1">
  <description>2-people example crew</description>
  <CrewMember id="1" name="CM01" crewMemberType="0">
   <TaskRef refId="1" percent="100" />
   <!-- Crewman 1 is 100% desirable for task 1 and 3 -->
   <TaskRef refId="3" percent="100" />
  </CrewMember>
  <CrewMember id="2" name="CM02" crewMemberType="0">
   <TaskRef refId="2" percent="100" />
   <TaskRef refId="4" percent="100" />
  </CrewMember>
 </Crew>
</DataSet>
{code:XML}

### Example of program using the library
A fully-working console application is provided using the above library with the _Basic_ domain. A simpler version is shown here:

{code:c#}
using System;
using System.Text;
using SimManning.Simulation;

namespace SimManning.Domain.Basic
{
  static class ProgramSimple
  {
    static void Main(string[]() args)
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
{code:c#}

### Basic output
Here is the console output of the above example program running the example XML data-set:

{code:other}
>SimManning.Domain.Default.CLI.exe < "Basic example.dataset.xml"
Load XML simulation data from standard input…
Start simulation
00:00:00	Start phase “Phase1”
00:00:00	Crewman “CM01” is assigned to the task “Task 3”
02:00:00	Crewman “CM01” is assigned to the task “-”
02:00:00	Crewman “CM01” is assigned to the task “Task 1”
02:00:00	Crewman “CM02” is assigned to the task “Task 2”
06:00:00	Crewman “CM01” is assigned to the task “-”
06:00:00	Crewman “CM02” is assigned to the task “-”
06:00:00	Crewman “CM02” is assigned to the task “Task 4”
07:00:00	Crewman “CM02” is assigned to the task “-”
07:00:00	Start phase “Phase2”
08:00:00	Start phase “Phase3”
08:00:00	Crewman “CM01” is assigned to the task “Task 3”
10:00:00	Crewman “CM01” is assigned to the task “-”
10:00:00	Crewman “CM01” is assigned to the task “Task 1”
10:00:00	Crewman “CM02” is assigned to the task “Task 2”
14:00:00	Crewman “CM01” is assigned to the task “-”
14:00:00	Crewman “CM02” is assigned to the task “-”
14:00:00	Crewman “CM02” is assigned to the task “Task 4”
15:00:00	Crewman “CM02” is assigned to the task “-”
15:00:00	End of scenario.

==== Example of statistics: cumulated work time ====
Crewman “CM01”: 12:00:00
Crewman “CM02”: 10:00:00
{code:other}
