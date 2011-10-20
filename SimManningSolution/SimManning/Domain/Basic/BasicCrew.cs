using System;

namespace SimManning.Domain.Basic
{
	public class BasicCrew : Crew
	{
		public BasicCrew(string name, TaskDictionary taskDictionary) : base(name, taskDictionary) { }

		public override Crewman CreateCrewman(int id)
		{
			return new BasicCrewman(id);
		}
	}
}
