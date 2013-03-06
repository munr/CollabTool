using System.Web.Mvc;
using System.Web.Routing;

namespace CollabTool.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "ApiGetNotes",
				url: "api/GetNotes/{studentId}/{includeDisciplineIncidents}",
				defaults: new { controller = "Api", action = "GetNotes", includeDisciplineIncidents = true }
			);

			routes.MapRoute(
				name: "Api",
				url: "api/{action}/{studentId}",
				defaults: new { controller = "Api" }
			);

			routes.MapRoute(
				name: "Authorize",
				url: "Authorize",
				defaults: new { controller = "Authorize", action = "Index" }
			);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "CollabTool" }
            );
        }
    }
}