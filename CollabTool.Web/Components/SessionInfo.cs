using System;
using System.Diagnostics;
using System.Web;
using System.Web.SessionState;


namespace CollabTool.Web
{
	/// <summary>
	/// This class is used for storing all information which needs to be 
	/// maintained at Session Scope. If more items need to be stored in 
	/// Session later in the project, add a property to this class.
	/// </summary>
	[Serializable]
	public class SessionInfo
	{
		public const string SessionKey = "SessionInfo";

		#region Accessors

		public string UserId { get; set; }
		public string AccessToken { get; set; }
		public string FullName { get; set; }
		public string Roles { get; set; }

		#endregion

		#region Static Accessors

		/// <summary>
		/// Returns a SessionInfo instance for the current user session
		/// </summary>
		/// 
		public static SessionInfo Current
		{
			get
			{
				if (HttpContext.Current == null || HttpContext.Current.Session == null)
				{
					Debug.WriteLine("SessionInfo.Current : No HttpContext, so session info will not be saved");
					return new SessionInfo();
				}

				return ForSession(new HttpSessionStateWrapper(HttpContext.Current.Session));
			}
		}

		public static SessionInfo Empty
		{
			get
			{
				return new SessionInfo();
			}
		}

		#endregion

		/// <summary>
		/// Reset all session settings to default values
		/// </summary>
		public void Reset()
		{
			UserId = string.Empty;
			AccessToken = string.Empty;
			FullName = string.Empty;
			Roles = string.Empty;
		}

		#region Helper Methods

		// Instances of SessionInfo should only be created by 
		// the Current property, so make the constructor private
		private SessionInfo()
		{
			Reset();
		}

		/// <summary>
		/// Gets the SessionInfo for the specified session
		/// </summary>
		public static SessionInfo ForSession(HttpSessionStateBase session)
		{
			var sessionInfo = session[SessionKey] as SessionInfo;
			if (sessionInfo == null)
			{
				sessionInfo = new SessionInfo();
				session.Add(SessionKey, sessionInfo);
			}
			return sessionInfo;
		}

		#endregion
	}
}