using System.Web.Mvc;
using inBloomApiLibrary;

namespace CollabTool.Web.Controllers
{
	public class AuthorizeController : Controller
	{
		public ActionResult Index(string code)
		{
			// Check for a token in the session already, and if found, no action is required
			if (!string.IsNullOrEmpty(SessionInfo.Current.AccessToken))
				return RedirectToAction("Index", "Home");

			// Init oAuth
			var oAuth = new OAuth();

			// We get a code back from the first leg of OAuth process.  If we don't have one, let's get it.
			if (string.IsNullOrEmpty(code))
			{
				string authorizationUrl = oAuth.GetAuthorizationUrl();
				return Redirect(authorizationUrl);
			}

			// Otherwise, we have a code, we can run the second leg of OAuth process.
			string authorization = oAuth.CallAuthorization(null, code);

			// OAuth successful so get values, store in session and continue
			if (authorization == "OAuthSuccess")
			{
				// Ensure that all required values were retrieved from the OAuth login
				if (oAuth.AccessToken != null && oAuth.UserFullName != null && oAuth.UserSLIRoles != null && oAuth.UserId != null)
				{
					// Authorization successful; set session variables
					SessionInfo.Current.AccessToken = oAuth.AccessToken;
					SessionInfo.Current.FullName = oAuth.UserFullName;
					SessionInfo.Current.Roles = oAuth.UserSLIRoles;
					SessionInfo.Current.UserId = oAuth.UserId;

					// Redirect to post login URL if one exists
					if (!string.IsNullOrEmpty(SessionInfo.Current.PostLoginRedirectUrl))
					{
						var returnUrl = SessionInfo.Current.PostLoginRedirectUrl;
						SessionInfo.Current.PostLoginRedirectUrl = null;
						return Redirect(returnUrl);
					}

					// Otherwise, just go to home
					return RedirectToAction("Index", "Home");
				}
			}

			return Content("Error authorizing");
		}

		public ActionResult LogOut()
		{
			// Init home service
			var service = new GetHomeData();
			
			// Logout the current token
			service.LogOut(SessionInfo.Current.AccessToken);

			// Clear the session
			Session.Abandon();
			
			// TODO: Redirect to login page
			return RedirectToAction("Index", "Home");
		}
	}
}