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
				// Store the return url
				if (sessionInfo != null && filterContext.HttpContext != null && filterContext.HttpContext.Request != null && filterContext.HttpContext.Request.Url != null)
					sessionInfo.PostLoginRedirectUrl = filterContext.HttpContext.Request.Url.ToString();

				// Redirect to the authorize handler which in turn will send user to inBloom
				filterContext.Result = new RedirectResult("~/Authorize");
			}
		}
	}
}