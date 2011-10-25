using System;
using System.Xml;
using System.Xml.Linq;
using SimManning.IO;

namespace SimManning
{
	/// <summary>
	/// Object characterising the workplace where the <see cref="Scenario"/> will take place,
	/// and where the <see cref="Crew"/> will work.
	/// </summary>
	public abstract class Workplace
	{
		readonly string name;

		public string Name
		{
			get { return this.name; }
		}

		string description = String.Empty;

		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		string tag;

		/// <summary>
		/// Generic tag that can be used by implementations to store custom data.
		/// Not used in the simulation itself, not used for testing equality, and not imported/exported to e.g. XML.
		/// </summary>
		public string Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		protected Workplace(string name)
		{
			this.name = name;
		}

		public virtual bool Identical(Workplace other)
		{
			return (other != null) && (this.name == other.name);
		}

		#region IO
		protected internal virtual void LoadFromXml(XElement element)
		{
			var elem = element.Element("description");
			this.description = elem == null ? String.Empty : elem.Value;
		}

		protected internal virtual void SaveToXml(XmlWriter xmlWriter)
		{
			var needsDeclaration = xmlWriter.WriteState == WriteState.Start;
			xmlWriter.WriteStartElement("Workplace");
			if (needsDeclaration)
			{
				xmlWriter.WriteAttributeString("domain", XmlIO.XmlDomain);
				xmlWriter.WriteAttributeString("version", XmlIO.XmlDomainVersion);
			}
			xmlWriter.WriteAttributeString("name", this.name);
			xmlWriter.WriteElementString("description", this.description);
			xmlWriter.WriteEndElement();
		}
		#endregion
	}
}
