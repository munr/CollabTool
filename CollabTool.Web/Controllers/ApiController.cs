using System;
using System.Linq;
using System.Web.Mvc;
using CollabTool.Web.Components;
using CollabTool.Web.Models;

namespace CollabTool.Web.Controllers
{
	/// <summary>
	/// Handles all interaction between user interface and inBloom data store
	/// </summary>
	[RequiresAuthentication]
	public class ApiController : Controller
	{
		public JsonResult GetStudents()
		{
			var service = new inBloomApiLibrary.GetStudentsData();
			var array = service.GetStudents(SessionInfo.Current.AccessToken);
			var students = (from dynamic token in array
							select new Student
							{
								Id = token.id,
								Name = string.Concat(token.name.firstName, " ", token.name.lastSurname)
							});

			return Json(students, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetStudent(string studentId)
		{
			throw new NotImplementedException();
		}

		public JsonResult GetStudentDetail(string studentId)
		{
			throw new NotImplementedException();
		}

		public JsonResult GetNotes(string studentId)
		{
			throw new NotImplementedException();
		}
		
		public JsonResult AddNote(Note note)
		{
			throw new NotImplementedException();
		}
	}
}