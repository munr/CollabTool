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
		#region Fields

		/// <summary>
		/// Student service
		/// </summary>
		private readonly GetStudentsData _studentService = new GetStudentsData();

		#endregion

		#region Properties

		/// <summary>
		/// Shortcut to the access token stored in the user's session
		/// </summary>
		private string CurrentAccessToken
		{
			get { return SessionInfo.Current.AccessToken; }
		}

		#endregion

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
			// First get the student data we need
			dynamic studentDetails = _studentService.GetStudentById(CurrentAccessToken, studentId)[0];
			dynamic studentAcademicRecord = _studentService.GetStudentAcademicRecordByStudentId(CurrentAccessToken, studentId)[0];
			dynamic studentReportCards = _studentService.GetStudentReportCards(CurrentAccessToken, studentId);

			/*
			 * student has reportCard
			 * reportCard has gradingPeriod
			 * gradingPeriod has gradingPeriodIdentity
			 * gradingPeriodIdentity has gradingPeriod
			 * ...but not all students have reportCards so there has to be another more reliable
			 * ...way of getting grade level.
			 */

			// Get the GPA
			string gradePointAverage = studentAcademicRecord.cumulativeGradePointAverage;
			string limitedEnglishProficiency = studentDetails.limitedEnglishProficiency;

			// Get disabilities
			string disabilities = string.Join(",", studentDetails.disabilities);

			// Summarize data into single StudentDetail object
			var studentDetail = new
				{
					Name = string.Concat(studentDetails.name.firstName, " ", studentDetails.name.lastSurname),
					GPA = gradePointAverage,
					Classes = "Math 101, English 102",		// TODO: Get from API
					GradeLevel = "8th Grade",				// TODO: Get from API
					LimitedEnglish = limitedEnglishProficiency.SplitAtCapitalLetters(),
					Disabilities = disabilities.IfNullThen("None")
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
		/// Get a teachers name from their ID using the inBloom API
		/// </summary>
		private string GetTeacherNameFromId(string id)
		{
			// Set missing teacher names to "unknown"
			// Should never occur, but just in case we hit test data
			if (string.IsNullOrEmpty(id))
				return "Unknown";

			// Otherwise, we have a teacher ID so use it to get the teacher name
			dynamic teacher = new GetTeachersData().GetTeacherById(CurrentAccessToken, id)[0];
			return string.Concat(teacher.name.firstName, " ", teacher.name.lastSurname);
		}

		/// <summary>
		/// Get a collection of notes for the specified student from the inBloom data store
		/// </summary>
		private NoteContainer GetStudentNotes(string studentId, bool includeDisciplineIncidents)
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

			// Now set teacher names if required
			foreach (var note in notes.Notes)
			{
				// Don't bother trying to get teacher name if we have it stored already
				if (!string.IsNullOrEmpty(note.TeacherName))
					continue;

				// Otherwise, we don't have a name so try and get it
				note.TeacherName = GetTeacherNameFromId(note.TeacherId);
			}

			if (includeDisciplineIncidents)
			{
				var disciplineService = new GetDisciplineData();

				// 1. Get student discipline incident associations
				// https://inbloom.org/sites/default/files/docs-developer-1.0.68-20130118/ch-examples-v1.0.html#ex-v1.0-students-id-studentDisciplineIncidentAssociations
				dynamic associations = _studentService.GetStudentStudentDisciplineIncidentAssociations(CurrentAccessToken, studentId);

				// 2. Get discipline incidents
				foreach (dynamic association in associations)
				{
					string disciplineIncidentId = association.disciplineIncidentId;
					dynamic disciplineIncident = disciplineService.GetDisciplineIncidentById(CurrentAccessToken, disciplineIncidentId);

					// Discipline incidents can contain multiple actions.
					// How do we convert this into a note - there's no simple way to achieve this
				}

				// 3. Convert incidents into note objects and add to notes
			}

			// Re-sort list by date descending (newest first)
			notes.Notes = notes.Notes.OrderByDescending(x => x.DateTime).ToList();

			// All done, return it
			return notes;
		}

		/// <summary>
		/// Gets the notes for a student
		/// </summary>
		/// <param name="studentId">The Student Id for whom notes should be returned</param>
		/// <param name="includeDisciplineIncidents">Boolean value specifying whether the notes returned should also include incidents</param>
		public JsonResult GetNotes(string studentId, bool includeDisciplineIncidents = true)
		{
			var notes = GetStudentNotes(studentId, includeDisciplineIncidents);
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Add a new note to a student record
		/// </summary>
		/// <param name="note">The note object to be added to the student</param>
		public JsonResult AddNote(Note note)
		{
			// Get the student ID from the note
			var studentId = note.StudentId;

			// Set internal properties
			note.TeacherId = SessionInfo.Current.UserId;

			// Just store the teacher name as well, to make reading the notes back quicker
			note.TeacherName = GetTeacherNameFromId(note.TeacherId);

			// Get the existing student notes
			// Do not include disciplines as these are not actually stored in the custom blob
			var notes = GetStudentNotes(studentId, false);

			// Add the new note to it
			notes.Notes.Add(note);

			// Convert the list of note into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save then note back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Deletes the note with the specified noteId from the specified user
		/// </summary>
		/// <param name="studentId">The Id of the note from whom the note should be removed</param>
		/// <param name="noteId">The Id of the note that should be removed</param>
		public JsonResult DeleteNote(string studentId, string noteId)
		{
			// Get the existing student notes
			// Do not include disciplines as this function is for deleting notes only
			var notes = GetStudentNotes(studentId, false);

			// Remove the note
			notes.Notes.RemoveAll(x => x.Id.ToString() == noteId);

			// Convert the list of notes into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save the note notes back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(notes, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Marks the note with the specified ID belonging to the specified user ID as resolved
		/// </summary>
		public JsonResult MarkNoteAsResolved(string studentId, string noteId)
		{
			// Get the existing student notes
			// Do not include disciplines as this function is for deleting notes only
			var notes = GetStudentNotes(studentId, false);

			// Find the note
			var note = notes.Notes.Find(x => x.Id.ToString() == noteId);

			// Note not found?
			if (note == null)
				return Json(new { success = false, message = "Note not found" }, JsonRequestBehavior.AllowGet);

			// Mark the note as resolved
			note.Resolved = true;

			// Convert the list of notes into JSON
			var data = JsonConvert.SerializeObject(notes);

			// Save the note notes back to the inBloom data store
			_studentService.PutStudents(CurrentAccessToken, data, studentId);

			// Return the new list
			return Json(new { success = true, message = "Note marked as resolved successfully" }, JsonRequestBehavior.AllowGet);
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

				// Get the student school id.  It appears that a student can be associated with multiple schools
				// so just get the first school they are associated with.
				dynamic associations = _studentService.GetStudentSchoolAssociationSchools(CurrentAccessToken, studentId);

				// Setup the data we need to post to create the incident
				var obj = new { incidentIdentifier, associations[0].Id };

				// Create the discipline incident
				var service = new GetDisciplineData();

				// TODO: Need to get the new discipline ID back from here in order to create the association
				dynamic response = service.PostDisciplineIncidents(CurrentAccessToken, JsonConvert.SerializeObject(obj));

				// Create the discipline incident association between the student and the incident
				// This doesn't work yet due to the problem above with getting back new discipline incident id
				var newAssociation = new { disciplineIncidentId = response.incidentId };
				_studentService.PostStudentDisciplineIncidentAssociations(CurrentAccessToken, JsonConvert.SerializeObject(newAssociation));

				Debug.WriteLine("Created discipline incident");

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

		//--------------------------------------------------------------
		// No functionality for deleting discipline incidents or actions
		//--------------------------------------------------------------

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