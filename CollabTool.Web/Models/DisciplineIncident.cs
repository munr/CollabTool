using System;

namespace CollabTool.Web.Models
{
	public class DisciplineIncident
	{
		public string Description { get; set; }
		public DateTime DateTime { get; set; }
		public string Location { get; set; }
		public string BehaviorType { get; set; }
	}
}