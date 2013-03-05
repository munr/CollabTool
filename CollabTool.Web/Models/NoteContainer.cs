using System.Collections.Generic;

namespace CollabTool.Web.Models
{
	public class NoteContainer
	{
		public NoteContainer()
		{
			Notes = new List<Note>();
		}

		public List<Note> Notes { get; set; }
	}
}