using System;
using System.Web;
using System.Web.Mvc;
using BundlingAndMinifyingInlineCssJs.ResponseFilters;

namespace MinCssAndJs.Filters
{
    public class BundleMinifyInlineCssJsAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Filter = new BundleAndMinifyResponseFilter(filterContext.HttpContext.Response.Filter);
        }

    }

}