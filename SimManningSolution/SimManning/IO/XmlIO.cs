using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SimManning.IO
{
	/// <summary>
	/// Basic XML input/output that is compatible with .NET 4.0 Client Profile, .NET 4.5, Metro, Silverlight 4, Mono 2.8+.
	/// Requires <see cref="System.Xml.Linq"/>.
	/// A significant part of this basic XML IO is implemented in the respective simulation classes
	/// such as <see cref="SimulationTask"/>, <see cref="Phase"/>, etc. to allow good inheritence.
	/// </summary>
	public static class XmlIO
	{
		/// <summary>
		/// Global string used at the root of XML documents to distinguish different domains.
		/// </summary>
		public static string XmlDomain = String.Empty;

		/// <summary>
		/// Global string used at the root of XML documents to distinguish different versions of the format.
		/// </summary>
		public static string XmlDomainVersion = "1.3";

		public static readonly XmlReaderSettings SimManningXmlReaderSettings = new XmlReaderSettings
		{
			CloseInput = true,
			ConformanceLevel = ConformanceLevel.Document,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true,
			IgnoreWhitespace = true
		};

		public static readonly XmlWriterSettings SimManningXmlWriterSettings = new XmlWriterSettings
		{
			CloseOutput = true,
			Indent = true,
			IndentChars = "\t",
			OmitXmlDeclaration = true
		};

		#region Parse extensions
		public static TimeUnit ParseTimeUnit(this XAttribute attribute)
		{
			return (attribute == null ? String.Empty : attribute.Value).ParseTimeUnit();
		}

		/*public static TimeDistribution ParseTimeDistribution(TimeUnit timeUnit, XAttribute min, XAttribute mode, XAttribute max)
		{
			return new TimeDistribution(timeUnit, min == null ? String.Empty : min.Value, mode == null ? String.Empty : mode.Value, max == null ? String.Empty : max.Value);
		}*/

		public static TimeDistribution ParseTimeDistribution(XAttribute timeUnit, XAttribute min, XAttribute mode, XAttribute max)
		{
			return new TimeDistribution(timeUnit.ParseTimeUnit(), min == null ? String.Empty : min.Value, mode == null ? String.Empty : mode.Value, max == null ? String.Empty : max.Value);
		}

		public static bool ParseBoolean(this XAttribute attribute)
		{
			return (attribute == null ? String.Empty : attribute.Value).ParseBoolean();
		}

		public static int ParseInteger(this XAttribute attribute)
		{
			return (attribute == null ? String.Empty : attribute.Value).ParseInteger();
		}

		/*static double ParseReal(this XAttribute xAttribute)
		{
			return (xAttribute == null ? String.Empty : xAttribute.Value).ParseReal();
		}*/

		public static SimulationTime ParseSimulationTime(this XAttribute value, TimeUnit timeUnit)
		{
			return new SimulationTime(timeUnit, value == null ? String.Empty : value.Value);
		}

		public static TaskInterruptionPolicies ParseTaskInterruptionPolicy(this XAttribute attribute)
		{
			return SimulationTask.ParseTaskInterruptionPolicy(attribute == null ? String.Empty : attribute.Value);
		}

		public static PhaseInterruptionPolicies ParsePhaseInterruptionPolicy(this XAttribute attribute)
		{
			return SimulationTask.ParsePhaseInterruptionPolicy(attribute == null ? String.Empty : attribute.Value);
		}

		public static ScenarioInterruptionPolicies ParseScenarioInterruptionPolicy(this XAttribute attribute)
		{
			return SimulationTask.ParseScenarioInterruptionPolicy(attribute == null ? String.Empty : attribute.Value);
		}

		public static TaskDuplicatesPolicy ParseTaskDuplicatesPolicy(this XAttribute attribute)
		{
			return SimulationTask.ParseTaskDuplicatesPolicy(attribute == null ? String.Empty : attribute.Value);
		}

		public static RelativeTimeType ParseRelativeTimeType(this XAttribute attribute)
		{
			return SimulationTask.ParseRelativeTimeType(attribute == null ? String.Empty : attribute.Value);
		}
		#endregion
	}
}
