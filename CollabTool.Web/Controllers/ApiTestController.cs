using System;
using System.Web.Mvc;
using CollabTool.Web.Models;

namespace CollabTool.Web.Controllers
{
	public class ApiTestController : Controller
	{
		/*
			{
				Id: "0129416134b14db0b69a696323ff2eaf44d7aa47_id",
				Name: "Tomasa Cleaveland",
				Class: "Mathematics 101",
				Grade: "8th Grade"
			},
		*/

		private readonly ApiController _apiController = new ApiController();

		public JsonResult AddTestNote(string studentId = "")
		{
			if (string.IsNullOrEmpty(studentId))
				studentId = "0129416134b14db0b69a696323ff2eaf44d7aa47_id";  // Tomasa Cleaveland

			var note = new Note { StudentId = studentId, NoteType = "Compliment", Subject = "Something good about the student", Text = "The student is very good in class.  Added on " + DateTime.Now.ToString("MMMM dd yyyy HH:mm:ss"), TeacherId = SessionInfo.Current.UserId };
			return _apiController.AddNote(note);
		}

		public JsonResult AddTestDisciplineIncident(string studentId = "")
		{
			if (string.IsNullOrEmpty(studentId))
				studentId = "0129416134b14db0b69a696323ff2eaf44d7aa47_id";  // Tomasa Cleaveland

			var disciplineIncident = new DisciplineIncident {Description = "The student done something very bad today and had to be disciplined", BehaviorType = "School Code of Conduct", Location = "School" };
			return _apiController.AddDisciplineIncident(studentId, disciplineIncident);
		}
	}
}