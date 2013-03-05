using System;
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
			var students = service.GetStudents(SessionInfo.Current.AccessToken);
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