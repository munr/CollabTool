using System.Web.Mvc;

namespace CollabTool.Web.Components
{
	/// <summary>
	/// Checks the User's authentication using FormsAuthentication
	/// and redirects to the Login Url for the application on fail
	/// </summary>
	public class RequiresAuthenticationAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var sessionInfo = SessionInfo.ForSession(filterContext.HttpContext.Session);

			if (sessionInfo == null || string.IsNullOrEmpty(sessionInfo.AccessToken))
			{
				// Redirect to the authorize handler which in turn will send user to inBloom
				filterContext.HttpContext.Response.Redirect("~/Authorize");
			}
		}
	}
}