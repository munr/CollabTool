using System;
using System.Linq;
using System.Web.Mvc;
using CollabTool.Web.Components;
using CollabTool.Web.Models;
using Newtonsoft.Json.Linq;
using inBloomApiLibrary;

namespace CollabTool.Web.Controllers
{
	/// <summary>
	/// Handles all interaction between user interface and inBloom data store
	/// </summary>
	[RequiresAuthentication]
	public class ApiController : Controller
	{
		#region Student List and Detail

		/// <summary>
		/// Get a list of students
		/// </summary>
		public JsonResult GetStudents()
		{
			var service = new GetStudentsData();
			var array = service.GetStudents(SessionInfo.Current.AccessToken);
			var students = (from dynamic token in array
			                select new Student
				                {
					                Id = token.id,
					                Name = string.Concat(token.name.firstName, " ", token.name.lastSurname)
				                });

			return Json(students, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Gets detailed information for a student
		/// </summary>
		public JsonResult GetStudentDetail(string studentId)
		{
			// First get the student demographics
			var studentService = new GetStudentsData();
			var jsonStudentObj = studentService.GetStudentById(SessionInfo.Current.AccessToken, studentId);
			dynamic objStudent = jsonStudentObj.FirstOrDefault() ?? new Object();

			// Summarize data into single StudentDetail object
			var studentDetail = new
				{
					Name = string.Concat(objStudent.name.firstName, " ", objStudent.name.lastSurname)
					// TODO: Add more demographics here
				};

			return Json(studentDetail, JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region Attendance and Scores

		/// <summary>
		/// Get student attendance
		/// </summary>
		/// <param name="studentId"></param>
		/// <returns></returns>
		public JArray GetAttendance(string studentId)
		{
			// No transformation required so just return whatever we got back from the inBloom API

			var services = new GetStudentsData();
			return services.GetStudentAttendances(SessionInfo.Current.AccessToken, studentId);
		}

		/// <summary>
		/// Get student test scores
		/// </summary>
		/// <param name="studentId"></param>
		/// <returns></returns>
		public JsonResult GetTestScores(string studentId)
		{
			// TODO: Which endpoint does this need to get data from?

			throw new NotImplementedException();
		}

		#endregion

		#region Notes

		/// <summary>
		/// Gets the notes for a student
		/// </summary>
		public JsonResult GetNotes(string studentId)
		{
			// Get the student custom data
			// If none, create
			// Save
			// Return it

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
			// Get the student custom data
			// If none, create
			// Add new note
			// Save
			// Return (what) ?

			throw new NotImplementedException();
		}

		#endregion
	}
}