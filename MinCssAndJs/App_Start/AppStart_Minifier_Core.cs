using System.Web.Mvc;
using System.Web.Routing;

[assembly: WebActivator.PreApplicationStartMethod(typeof(MinCssAndJs.App_Start.AppStart_Minifier_Core), "PreStart")]
namespace MinCssAndJs.App_Start 
{
    public static class AppStart_Minifier_Core
	{
        public static void PreStart() 
		{
            RouteTable.Routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.min.js(/.*)?" });
        }
    }
}