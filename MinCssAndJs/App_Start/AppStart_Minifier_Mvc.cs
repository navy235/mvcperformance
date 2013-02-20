using System.Web.Mvc;
using System.Web.Routing;

[assembly: WebActivator.PreApplicationStartMethod(typeof(MinCssAndJs.App_Start.AppStart_Minifier_Mvc), "PreStart")]
namespace MinCssAndJs.App_Start 
{
    public static class AppStart_Minifier_Mvc 
	{
        public static void PreStart() 
		{
            GlobalFilters.Filters.Add(new Lervik.Minifier.Mvc.MinifyActionAttribute());
        }
    }
}