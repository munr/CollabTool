using System;
using System.Diagnostics;
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

		/// <summary>
		/// Shortcut to the current session token
		/// </summary>
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
					                Name = string.Concat(token.name.firstName, " ", token.name.lastSurname),
									Class = "Mathematics 101",	// TODO: Get from API
									Grade = "8th Grade"			// TODO: Get from API
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
					Name = string.Concat(objStudent.name.firstName, " ", objStudent.name.lastSurname),
					GPA = 3.7,								// TODO: Get from API
					Classes = "Math 101, English 102",		// TODO: Get from API
					GradeLevel = "8th Grade",				// TODO: Get from API
					LimitedEnglish = "Limited",					// TODO: Get from API this is not a boolean
					Disabilities = "None"					// TODO: Get from API
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
			// Set internal properties
			note.TeacherId = SessionInfo.Current.UserId;

			// Get the existing student notes
			var notes = GetStudentNotes(studentId);

			// Add the new note to it
			notes.Notes.Add(note);

			// Convert the list of note into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save then note back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		public JsonResult DeleteNote(string studentId, string noteId)
		{
			// Get the existing student notes
			var notes = GetStudentNotes(studentId);

			// Remove the note
			notes.Notes.RemoveAll(x => x.Id.ToString() == noteId);

			// Convert the list of note into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save the note notes back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region Discipline incident and action

		/// <summary>
		/// Add a new discipline incident to a student
		/// </summary>
		public JsonResult AddDisciplineIncident(string studentId, DisciplineIncident disciplineIncident)
		{
			try
			{
				// TODO: This might need to be user-entered but use random string for now
				var incidentIdentifier = Guid.NewGuid().ToString().Split('-').First();

				// Get the student school id
				dynamic student = _studentService.GetStudentById(CurrentAccessToken, studentId);

				// Setup the data we need to post to create the incident
				var obj = new { incidentIdentifier, student.schoolId };

				// Post the discipline incident
				var service = new GetDisciplineData();
				var response = service.PostDisciplineIncidents(CurrentAccessToken, JsonConvert.SerializeObject(obj));

				Debug.WriteLine("Created discipline incident: " + response);

				return Json(new { success = true, incidentIdentifier, message = "Added discipline incident successfully" }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e)
			{
				Debug.WriteLine("Error adding discipline incident: " + e);
				return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// Add a new discipline action to a discipline incident
		/// </summary>
		public JsonResult AddDisciplineAction(string studentId, string disciplineActionIdentifier, DisciplineAction disciplineAction)
		{
			try
			{
				// Setup the data we need to post to create the incident
				// https://inbloom.org/sites/default/files/docs-developer-1.0.68-20130118/ch-data_model-entities.html#type-DisciplineAction
				// https://inbloom.org/sites/default/files/docs-developer-1.0.68-20130118/ch-data_model-entities.html#type-DisciplineDescriptorType

				var obj = new
					{
						disciplineActionIdentifier,
						disciplineDate = disciplineAction.DateTime,
						disciplines = new { shortDescription = disciplineAction.ShortDescription, description = disciplineAction.Description }
					};

				// Post the discipline incident
				var service = new GetDisciplineData();
				var response = service.PostDisciplineActions(CurrentAccessToken, JsonConvert.SerializeObject(obj));

				Debug.WriteLine("Created discipline action: " + response);

				return Json(new { success = true, message = "Added discipline action successfully" }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e)
			{
				Debug.WriteLine("Error adding discipline action: " + e);
				return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion

        #region nhi's tuff

        //private readonly GetAttendancesData _attendanceService = new GetAttendancesData();
        //private const string[] AttendanceEventCategoryType = new string[5]{"In Attendance", "Excused Absence", "Unexcused Absence", "Tardy", "Early departure"};

        public JsonResult GetAssessments(string studentId)
        {
            return Json(_studentService.GetStudentAssessments(CurrentAccessToken), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAttendanceCount(string studentId)
        {
            var a = GetAttendance(studentId);
            var att = a[0]["schoolYearAttendance"].ToString();
            var att2 = JArray.Parse(att);
            JArray events = null;

            for (var i = 0; i < att2.Count; i++)
            {
                JArray year = JArray.Parse(att2[i]["attendanceEvent"].ToString());
                if (year.Count > 0)
                    events = year;
            }
            var agroup = events.GroupBy(x => x["event"]).ToList();
            JArray attendanceCount = JArray.Parse("[{'event': 'In Attendance', 'count':'0'},{'event': 'Excused Absence', 'count':'0'},{'event': 'Unexcused Absence', 'count':'0'},{'event': 'Tardy', 'count':'0'},{'event': 'Early departure', 'count':'0'}]");
            for (var x = 0; x < agroup.Count; x++)
            {
                for (var y = 0; y < attendanceCount.Count; y++)
                {
                    if (agroup[x].Key.ToString() == attendanceCount[y]["event"].ToString())
                        attendanceCount[y]["count"] = agroup[x].Count();
                }
            }

            return Json(attendanceCount, JsonRequestBehavior.AllowGet);
        }



        #endregion
	}
}