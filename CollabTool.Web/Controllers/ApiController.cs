using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using CollabTool.Web.Components;
using CollabTool.Web.Models;
using Newtonsoft.Json;
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
		private readonly GetStudentsData _studentService = new GetStudentsData();

		private string CurrentAccessToken
		{
			get { return SessionInfo.Current.AccessToken; }
		}

		#region Student List and Detail

		/// <summary>
		/// Get a list of students
		/// </summary>
		public JsonResult GetStudents()
		{
			var array = _studentService.GetStudents(CurrentAccessToken);
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
			var jsonStudentObj = _studentService.GetStudentById(CurrentAccessToken, studentId);
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
			// No transformation required yet so just return whatever we got back from the inBloom API

			return _studentService.GetStudentAttendances(CurrentAccessToken, studentId);
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
		/// Get a collection of notes for the specified student from the inBloom data store
		/// </summary>
		private NoteContainer GetStudentNotes(string studentId)
		{
			// Declare data
			JArray data = null;

			try
			{
				// Try and get custom data from the inBloom API
				data = _studentService.GetStudentCustom(CurrentAccessToken, studentId);
			}
			catch (Exception ex)
			{
				var wex = ex as WebException;

				if (wex != null)
				{
					var hex = wex.Response as HttpWebResponse;

					if (hex != null)
					{
						if (hex.StatusCode != HttpStatusCode.NotFound)
						{
							throw;
						}
					}
				}
			}

			// Convert JArray back to string so we can convert into strongly typed collection
			var json = (data == null) ? string.Empty : data.ToString();

			// Remove the array as we're only working with a single object
			if (json.StartsWith("[") && json.EndsWith("]"))
				json = json.Substring(1, json.Length - 2);

			// Convert into strongly typed collection if possible
			var notes = (!string.IsNullOrEmpty(json)) ? JsonConvert.DeserializeObject<NoteContainer>(json) : new NoteContainer();

			// All done, return it
			return notes;
		}

		/// <summary>
		/// Gets the notes for a student
		/// </summary>
		public JsonResult GetNotes(string studentId)
		{
			var notes = GetStudentNotes(studentId);
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Add a new note to a student record
		/// </summary>
		/// <param name="studentId">The ID of the student to which the note should be added</param>
		/// <param name="note">The note object to be added to the student</param>
		/// <returns></returns>
		public JsonResult AddNote(string studentId, Note note)
		{
			// Get the existing student notes
			var notes = GetStudentNotes(studentId);

			// Add the new note to it
			notes.Notes.Add(note);

			// Convert the list of note into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save then notes back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		public JsonResult AddTestNote(string studentId, string subject = "Test Subject")
		{
			var note = new Note {Subject = subject, Text = "This is a test body", SentimentType = SentimentType.Positive};
			return AddNote(studentId, note);
		}

		#endregion
	}
}