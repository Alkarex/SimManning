using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;

namespace SimManning
{
	partial class SimulationTask
	{
		protected internal virtual bool LoadFromXml(XElement xmlTask)
		{//TODO: Error messages
			XAttribute attr;
			this.name = (attr = xmlTask.Attribute("name")) == null ? String.Empty : attr.Value;
			this.TaskType = xmlTask.Attribute("taskType").ParseInteger();
			//this.systemTask = (attr = xmlTask.Attribute("systemTask")) == null ? false : attr.Value.ParseBoolean();
			this.autoExpandToAllCrewmen = (attr = xmlTask.Attribute("autoAssignToAllCrewMembers")) == null ? false : attr.Value.ParseBoolean();
			this.RelativeDate = (attr = xmlTask.Attribute("relativeDate")) == null ? RelativeDateType.Undefined : SimulationTask.ParseRelativeDateType(attr.Value);
			this.StartDate = XmlIO.ParseTimeDistribution(xmlTask.Attribute("startDateUnit"),
				xmlTask.Attribute("startDateMin"), xmlTask.Attribute("startDateMean"), xmlTask.Attribute("startDateMax"));
			if (this.relativeDate == RelativeDateType.Frequency)
				this.DateOffset = XmlIO.ParseTimeDistribution(xmlTask.Attribute("dateOffsetUnit"),
					xmlTask.Attribute("dateOffsetMin"), xmlTask.Attribute("dateOffsetMean"), xmlTask.Attribute("dateOffsetMax"));
			this.relativeTime = xmlTask.Attribute("relativeTime").ParseRelativeTimeType();
			this.DailyHourStart = xmlTask.Attribute("workingHourStart").ParseSimulationTime(TimeUnit.Hours);
			this.DailyHourEnd = xmlTask.Attribute("workingHourEnd").ParseSimulationTime(TimeUnit.Hours);
			this.onHolidays = xmlTask.Attribute("onHolidays").ParseBoolean();
			this.enabled = (attr = xmlTask.Attribute("enabled")) == null ? true : attr.ParseBoolean();
			this.Duration = XmlIO.ParseTimeDistribution(xmlTask.Attribute("taskDurationUnit"),
				xmlTask.Attribute("taskDurationMin"), xmlTask.Attribute("taskDurationMean"), xmlTask.Attribute("taskDurationMax"));
			attr = xmlTask.Attribute("taskInterruptionPolicy");
			if (attr == null) this.taskInterruptionPolicy = TaskInterruptionPolicies.ContinueOrResumeWithoutError;	//Back compatibility behaviour
			else this.taskInterruptionPolicy = attr.ParseTaskInterruptionPolicy();
			attr = xmlTask.Attribute("phaseInterruptionPolicy");
			if (attr == null)
			{
				attr = xmlTask.Attribute("interruptibility");	//Backward compatibility
				if (attr == null) attr = xmlTask.Attribute("interruptability");	//Spelling compatibility
			}
			this.phaseInterruptionPolicy = attr.ParsePhaseInterruptionPolicy();
			attr = xmlTask.Attribute("scenarioInterruptionPolicy");
			if (attr == null) this.scenarioInterruptionPolicy = ScenarioInterruptionPolicies.DropWithoutError;	//Back compatibility behaviour
			else this.scenarioInterruptionPolicy = attr.ParseScenarioInterruptionPolicy();
			this.priority = xmlTask.Attribute("priority").ParseInteger();
			this.numberOfCrewmenNeeded = xmlTask.Attribute("numberOfCrewMembersNeeded").ParseInteger();
			if ((attr = xmlTask.Attribute("rotation")) != null)
				this.Rotation = new SimulationTime(TimeUnit.Hours, Math.Min(24.0, Math.Max(0.0, attr.Value.ParseReal())));
			else if ((attr = xmlTask.Attribute("isRotating")) != null)	//Compatibility
				this.Rotation = new SimulationTime(TimeUnit.Hours, attr.Value.ParseBoolean() ? 6 : 0);	//Previously hard-coded to 6-hour rotation
			else this.Rotation = SimulationTime.Zero;
			XElement elem;
			this.description = (elem = xmlTask.Element("description")) == null ? String.Empty : elem.Value;
			elem = xmlTask.Element("phaseTypes");
			if (elem != null)
				foreach (var phaseTypeRef in elem.Elements("PhaseTypeRef"))
				{
					attr = phaseTypeRef.Attribute("refCode");
					if (attr != null) this.phaseTypes.Add(attr.Value.ParseInteger());
				}
			elem = xmlTask.Element("crewMemberTypes");
			if (elem != null)
				foreach (var phaseTypeRef in elem.Elements("CrewMemberTypeRef"))
				{
					attr = phaseTypeRef.Attribute("refCode");
					if (attr != null) this.crewmanTypes.Add(attr.Value.ParseInteger());
				}
			attr = xmlTask.Attribute("taskDuplicatesPolicy");
			if (attr == null) this.NoDuplicate = (attr = xmlTask.Attribute("noDuplicate")) == null ? false : attr.Value.ParseBoolean();	//Back compatibility behaviour
			else this.duplicatesPolicy = attr.ParseTaskDuplicatesPolicy();
			//Task relations are loaded by TaskDictionary
			return true;
		}

		protected internal virtual void SaveToXml(XmlWriter xmlWriter)
		{
			//if (this.systemTask) return;	//Do not save system tasks
			xmlWriter.WriteStartElement("Task");
			xmlWriter.WriteAttributeString("id", this.id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteAttributeString("taskType", this.taskType.ToString(CultureInfo.InvariantCulture));
			//xmlWriter.WriteAttributeString("systemTask", this.systemTask.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("autoAssignToAllCrewMembers", this.autoExpandToAllCrewmen.ToString());
			xmlWriter.WriteAttributeString("relativeDate", this.relativeDate.ToString());
			xmlWriter.WriteAttributeString("startDateUnit", this.StartDate.Unit.ToString());
			xmlWriter.WriteAttributeString("startDateMin", this.StartDate.Min.ToStringRaw());
			xmlWriter.WriteAttributeString("startDateMean", this.StartDate.Mode.ToStringRaw());
			xmlWriter.WriteAttributeString("startDateMax", this.StartDate.Max.ToStringRaw());
			if ((this.relativeDate == RelativeDateType.Frequency) && (this.DateOffset.Unit != TimeUnit.Undefined))
			{
				xmlWriter.WriteAttributeString("dateOffsetUnit", this.DateOffset.Unit.ToString());
				xmlWriter.WriteAttributeString("dateOffsetMin", this.DateOffset.Min.ToStringRaw());
				xmlWriter.WriteAttributeString("dateOffsetMean", this.DateOffset.Mode.ToStringRaw());
				xmlWriter.WriteAttributeString("dateOffsetMax", this.DateOffset.Max.ToStringRaw());
			}
			xmlWriter.WriteAttributeString("relativeTime", this.relativeTime.ToString());
			xmlWriter.WriteAttributeString("workingHourStart", this.DailyHourStart.ToStringRaw());
			xmlWriter.WriteAttributeString("workingHourEnd", this.DailyHourEnd.ToStringRaw());
			xmlWriter.WriteAttributeString("onHolidays", this.onHolidays.ToString());
			if (!this.enabled)
				xmlWriter.WriteAttributeString("enabled", this.enabled.ToString());
			xmlWriter.WriteAttributeString("taskDurationUnit", this.Duration.Unit.ToString());
			xmlWriter.WriteAttributeString("taskDurationMin", this.Duration.Min.ToStringRaw());
			xmlWriter.WriteAttributeString("taskDurationMean", this.Duration.Mode.ToStringRaw());
			xmlWriter.WriteAttributeString("taskDurationMax", this.Duration.Max.ToStringRaw());
			//xmlWriter.WriteAttributeString("frequencyUnit", task.FrequencyUnit.ToString());
			//xmlWriter.WriteAttributeString("frequency", task.Frequency.ToTimeEntry(this.frequencyUnit));
			//xmlWriter.WriteAttributeString("occurrences", task.Occurrences.ToString());
			xmlWriter.WriteAttributeString("taskInterruptionPolicy", this.taskInterruptionPolicy.ToString());
			xmlWriter.WriteAttributeString("phaseInterruptionPolicy", this.phaseInterruptionPolicy.ToString());
			xmlWriter.WriteAttributeString("scenarioInterruptionPolicy", this.scenarioInterruptionPolicy.ToString());
			xmlWriter.WriteAttributeString("interruptionErrorPolicy", this.interruptionErrorPolicy.ToString());
			xmlWriter.WriteAttributeString("priority", this.priority.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("numberOfCrewMembersNeeded", this.numberOfCrewmenNeeded.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("rotation", this.Rotation.TotalHours.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("noDuplicate", this.NoDuplicate.ToString());
			xmlWriter.WriteAttributeString("taskDuplicatesPolicy", this.duplicatesPolicy.ToString());
			if (!String.IsNullOrWhiteSpace(this.description))
				xmlWriter.WriteElementString("description", this.description);
			xmlWriter.WriteStartElement("phaseTypes");
			foreach (var phaseType in this.phaseTypes)
			{
				xmlWriter.WriteStartElement("PhaseTypeRef");
				xmlWriter.WriteAttributeString("refCode", phaseType.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("crewMemberTypes");
			foreach (var crewmanType in this.crewmanTypes)
			{
				xmlWriter.WriteStartElement("CrewMemberTypeRef");
				xmlWriter.WriteAttributeString("refCode", crewmanType.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("taskRelations");
			//var relStr = ((int)TaskRelation.RelationType.Parallel).ToString(CultureInfo.InvariantCulture);
			var relStr = TaskRelation.RelationType.Parallel.ToString();
			foreach (var taskId2 in this.parallelTasks.Keys)
			{
				xmlWriter.WriteStartElement("TaskRef");
				xmlWriter.WriteAttributeString("rel", relStr);
				xmlWriter.WriteAttributeString("refId", taskId2.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			//relStr = ((int)TaskRelation.RelationType.Slave).ToString(CultureInfo.InvariantCulture);
			relStr = TaskRelation.RelationType.Slave.ToString();
			foreach (var taskId2 in this.slaveTasks.Keys)
			{
				xmlWriter.WriteStartElement("TaskRef");
				xmlWriter.WriteAttributeString("rel", relStr);
				xmlWriter.WriteAttributeString("refId", taskId2.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			//relStr = ((int)TaskRelation.RelationType.Master).ToString(CultureInfo.InvariantCulture);
			relStr = TaskRelation.RelationType.Master.ToString();
			foreach (var taskId2 in this.masterTasks.Keys)
			{
				xmlWriter.WriteStartElement("TaskRef");
				xmlWriter.WriteAttributeString("rel", relStr);
				xmlWriter.WriteAttributeString("refId", taskId2.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected internal virtual bool LoadRefFromXml(XElement xmlTaskRef)
		{
			XAttribute attr;
			var timeUnit = (attr = xmlTaskRef.Attribute("startDateUnit")) == null ? this.refTask.StartDate.Unit : attr.Value.ParseTimeUnit();
			this.StartDate = new TimeDistribution(timeUnit,
				(attr = xmlTaskRef.Attribute("startDateMin")) == null ? this.refTask.StartDate.Min : new SimulationTime(timeUnit, attr.Value),
				(attr = xmlTaskRef.Attribute("startDateMean")) == null ? this.refTask.StartDate.Mode : new SimulationTime(timeUnit, attr.Value),
				(attr = xmlTaskRef.Attribute("startDateMax")) == null ? this.refTask.StartDate.Max : new SimulationTime(timeUnit, attr.Value));
			if (this.relativeDate == RelativeDateType.Frequency)
			{
				timeUnit = (attr = xmlTaskRef.Attribute("dateOffsetUnit")) == null ? this.refTask.DateOffset.Unit : attr.Value.ParseTimeUnit();
				this.DateOffset = new TimeDistribution(timeUnit,
					(attr = xmlTaskRef.Attribute("dateOffsetMin")) == null ? this.refTask.DateOffset.Min : new SimulationTime(timeUnit, attr.Value),
					(attr = xmlTaskRef.Attribute("dateOffsetMean")) == null ? this.refTask.DateOffset.Mode : new SimulationTime(timeUnit, attr.Value),
					(attr = xmlTaskRef.Attribute("dateOffsetMax")) == null ? this.refTask.DateOffset.Max : new SimulationTime(timeUnit, attr.Value));
			}
			if ((attr = xmlTaskRef.Attribute("workingHourStart")) != null)
				this.DailyHourStart = new SimulationTime(TimeUnit.Hours, attr.Value);
			if ((attr = xmlTaskRef.Attribute("workingHourEnd")) != null)
				this.DailyHourEnd = new SimulationTime(TimeUnit.Hours, attr.Value);
			if ((attr = xmlTaskRef.Attribute("onHolidays")) != null)
				this.onHolidays = attr.Value.ParseBoolean();
			timeUnit = (attr = xmlTaskRef.Attribute("taskDurationUnit")) == null ? this.refTask.Duration.Unit : attr.Value.ParseTimeUnit();
			this.Duration = new TimeDistribution(timeUnit,
				(attr = xmlTaskRef.Attribute("taskDurationMin")) == null ? this.refTask.Duration.Min : new SimulationTime(timeUnit, attr.Value),
				(attr = xmlTaskRef.Attribute("taskDurationMean")) == null ? this.refTask.Duration.Mode : new SimulationTime(timeUnit, attr.Value),
				(attr = xmlTaskRef.Attribute("taskDurationMax")) == null ? this.refTask.Duration.Max : new SimulationTime(timeUnit, attr.Value));
			if ((attr = xmlTaskRef.Attribute("priority")) != null)
				this.priority = attr.Value.ParseInteger();
			if ((attr = xmlTaskRef.Attribute("numberOfCrewMembersNeeded")) != null)
				this.numberOfCrewmenNeeded = attr.Value.ParseInteger();
			if ((attr = xmlTaskRef.Attribute("rotation")) != null)
				this.Rotation = new SimulationTime(TimeUnit.Hours, Math.Min(24.0, Math.Max(0, attr.Value.ParseReal())));
			else if ((attr = xmlTaskRef.Attribute("isRotating")) != null)	//Compatibility
				this.Rotation = new SimulationTime(TimeUnit.Hours, attr.Value.ParseBoolean() ? 6.0 : 0.0);	//Previously hard-coded to 6-hour rotation
			XElement elem;
			if ((elem = xmlTaskRef.Element("description")) != null)
				this.description = elem.Value;
			return true;
		}

		protected internal virtual void SaveRefToXml(XmlWriter xmlWriter)
		{
			//if (this.systemTask) return;	//Do not save system tasks
			xmlWriter.WriteStartElement("TaskRef");
			xmlWriter.WriteAttributeString("refId", this.id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("informativeName", this.name);
			//Override values
			//if (this.relativeDate != this.refTask.relativeDate) xmlWriter.WriteAttributeString("relativeDate", this.relativeDate.ToString());
			//if (this.autoAssignToAllCrewMembers != this.refTask.autoAssignToAllCrewMembers) xmlWriter.WriteAttributeString("autoAssignToAllCrewMembers", this.autoAssignToAllCrewMembers.ToString(CultureInfo.InvariantCulture));
			if (this.StartDate.Unit != this.refTask.StartDate.Unit)
				xmlWriter.WriteAttributeString("startDateUnit", this.StartDate.Unit.ToString());
			if (this.StartDate.Min != this.refTask.StartDate.Min)
				xmlWriter.WriteAttributeString("startDateMin", this.StartDate.Min.ToStringRaw());
			if (this.StartDate.Mode != this.refTask.StartDate.Mode)
				xmlWriter.WriteAttributeString("startDateMean", this.StartDate.Mode.ToStringRaw());
			if (this.StartDate.Max != this.refTask.StartDate.Max)
				xmlWriter.WriteAttributeString("startDateMax", this.StartDate.Max.ToStringRaw());
			if (this.relativeDate == RelativeDateType.Frequency)
			{
				if (this.DateOffset.Unit != this.refTask.DateOffset.Unit)
					xmlWriter.WriteAttributeString("dateOffsetUnit", this.DateOffset.Unit.ToString());
				if (this.DateOffset.Min != this.refTask.DateOffset.Min)
					xmlWriter.WriteAttributeString("dateOffsetMin", this.DateOffset.Min.ToStringRaw());
				if (this.DateOffset.Mode != this.refTask.StartDate.Mode)
					xmlWriter.WriteAttributeString("dateOffsetMean", this.DateOffset.Mode.ToStringRaw());
				if (this.DateOffset.Max != this.refTask.StartDate.Max)
					xmlWriter.WriteAttributeString("dateOffsetMax", this.DateOffset.Max.ToStringRaw());
			}
			//if (this.relativeTime != this.refTask.relativeTime) xmlWriter.WriteAttributeString("relativeTime", this.relativeTime.ToString());
			if (this.DailyHourStart != this.refTask.DailyHourStart)
				xmlWriter.WriteAttributeString("workingHourStart", this.DailyHourStart.ToStringRaw());
			if (this.DailyHourEnd != this.refTask.DailyHourEnd)
				xmlWriter.WriteAttributeString("workingHourEnd", this.DailyHourEnd.ToStringRaw());
			if (this.onHolidays != this.refTask.OnHolidays)
				xmlWriter.WriteAttributeString("onHolidays", this.onHolidays.ToString());
			if (this.Duration.Unit != this.refTask.Duration.Unit)
				xmlWriter.WriteAttributeString("taskDurationUnit", this.Duration.Unit.ToString());
			if (this.Duration.Min != this.refTask.Duration.Min)
				xmlWriter.WriteAttributeString("taskDurationMin", this.Duration.Min.ToStringRaw());
			if (this.Duration.Mode != this.refTask.Duration.Mode)
				xmlWriter.WriteAttributeString("taskDurationMean", this.Duration.Mode.ToStringRaw());
			if (this.Duration.Max != this.refTask.Duration.Max)
				xmlWriter.WriteAttributeString("taskDurationMax", this.Duration.Max.ToStringRaw());
			if (this.priority != this.refTask.Priority)
				xmlWriter.WriteAttributeString("priority", this.priority.ToString(CultureInfo.InvariantCulture));
			if (this.numberOfCrewmenNeeded != this.refTask.NumberOfCrewmenNeeded)
				xmlWriter.WriteAttributeString("numberOfCrewMembersNeeded", this.numberOfCrewmenNeeded.ToString(CultureInfo.InvariantCulture));
			if (this.Rotation != this.refTask.Rotation)
				xmlWriter.WriteAttributeString("rotation", this.Rotation.TotalHours.ToString(CultureInfo.InvariantCulture));
			if (this.description != this.refTask.Description)
				xmlWriter.WriteElementString("description", this.description);
			xmlWriter.WriteStartElement("alternativePhases");
			//foreach (var alternativePhase in this.alternativePhases)
			//	alternativePhase.Save(xmlAlternativePhases);	//TODO: alternative phases
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}
	}
}
