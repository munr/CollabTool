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
			// First get the student demographics
			var studentService = new inBloomApiLibrary.GetStudentsData();
			var jsonStudentObj = studentService.GetStudentById(SessionInfo.Current.AccessToken, studentId);
			dynamic objStudent = jsonStudentObj.FirstOrDefault() ?? new Object();

			// Summarize data into single StudentDetail object
			var studentDetail = new
				{
					Name = string.Concat(objStudent.name.firstName, " ", objStudent.name.lastSurname)
				};

			return Json(studentDetail, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetStudentDetail(string studentId)
		{
			throw new NotImplementedException();
		}

		public JsonResult GetNotes(string studentId)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Add a new note to a student record
		/// </summary>
		/// <param name="studentId">The ID of the student to which the note should be added</param>
		/// <param name="note">The note object to be added to the student</param>
		/// <returns></returns>
		public JsonResult AddNote(string studentId, Note note)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get student attendance
		/// </summary>
		/// <param name="studentId"></param>
		/// <returns></returns>
		public JsonResult GetAttendance(string studentId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get student test scores
		/// </summary>
		/// <param name="studentId"></param>
		/// <returns></returns>
		public JsonResult GetTestScores(string studentId)
		{
			throw new NotImplementedException();
		}
	}
}