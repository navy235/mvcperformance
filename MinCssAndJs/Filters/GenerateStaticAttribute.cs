using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web.Routing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace MinCssAndJs.Filters
{
    public class GenerateStaticAttribute : ActionFilterAttribute
    {

        public GenerateStaticAttribute(int duration = 60, bool childActionNotByParams = false, bool isLoginNotCache = true)
        {
            Duration = duration;
            ChildActionNotByParams = childActionNotByParams;
            IsLoginNotCache = isLoginNotCache;
        }

        public int Duration { get; set; }

        public bool ChildActionNotByParams { get; set; }

        public bool IsLoginNotCache { get; set; }

        private TextWriter _originalWriter;

        private string _staticFileName;

        private bool _nocache;



        private static MethodInfo _switchWriterMethod = typeof(HttpResponse).GetMethod("SwitchWriter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        private bool DetermineNoCache(ActionExecutingContext filterContext)
        {
            return false;
        }
        private int GetDuration(ControllerContext filterContext)
        {
            return Duration;
        }

        private bool isOutDuration()
        {
            var lastTime = File.GetLastWriteTimeUtc(_staticFileName);
            return DateTime.UtcNow.CompareTo(lastTime.AddSeconds(Duration)) > 0;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            _nocache = DetermineNoCache(filterContext);
            if (_nocache) return;

            _staticFileName = ComputeStaticFileName(filterContext);
            string cachedOutput = string.Empty;
            if (File.Exists(_staticFileName) && !isOutDuration())
            {
                cachedOutput = File.ReadAllText(_staticFileName, Encoding.UTF8);
                filterContext.Result = new ContentResult { Content = cachedOutput };
                _originalWriter = null;
            }
            else
            {
                _originalWriter = (TextWriter)_switchWriterMethod.Invoke(HttpContext.Current.Response, new object[] { new HtmlTextWriter(new StringWriter()) });

            }

            //string cachedOutput = (string)filterContext.HttpContext.Cache[_cacheKey];


        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (_nocache) return;

            if (_originalWriter != null) // Must complete the caching
            {
                HtmlTextWriter cacheWriter = (HtmlTextWriter)_switchWriterMethod.Invoke(HttpContext.Current.Response, new object[] { _originalWriter });

                string textWritten = ((StringWriter)cacheWriter.InnerWriter).ToString();

                if (!File.Exists(_staticFileName) || isOutDuration())
                {
                    File.WriteAllText(_staticFileName, textWritten, Encoding.UTF8);
                }
                filterContext.HttpContext.Response.Write(textWritten);
            }
        }

        private string ComputeStaticFileName(ActionExecutingContext filterContext)
        {
            // Assumptions: empty param values & order of params are irrelevant; optimize by sorting and removing empties

            var context = filterContext.HttpContext;
            var request = context.Request;
            var url = request.Url;
            var keyBuilder = new StringBuilder();
            var staticPath = "~/Content/static/";
            var controller = filterContext.RouteData.Values["controller"].ToString();
            var action = filterContext.RouteData.Values["action"].ToString();
            staticPath = string.Format("{0}/{1}/{2}/", staticPath, controller, action);

            var fileDirectory = context.Server.MapPath(staticPath);
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            keyBuilder.Append(fileDirectory);

            if (ChildActionNotByParams && filterContext.IsChildAction)
            {
                keyBuilder.AppendFormat("{0}_{1}_", action, controller);
            }
            else
            {
                foreach (var pair in filterContext.RouteData.Values.Where(p => p.Value != null).OrderBy(p => p.Key))
                    keyBuilder.AppendFormat("{0}_{1}_", pair.Key, pair.Value.ToString());
            }
            //keyBuilder.AppendFormat("rd{0}_{1}_", pair.Key.GetHashCode(), pair.Value.GetHashCode());

            //foreach (var pair in filterContext.ActionParameters.Where(p => p.Value != null).OrderBy(p => p.Key))
            //    keyBuilder.AppendFormat("{0}_{1}_", pair.Key, pair.Value.ToString());
            keyBuilder.Append(".html");


            return keyBuilder.ToString();
        }

        public void ReturnStaticFile(ActionExecutingContext filterContext)
        {
            UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
            Dictionary<string, string> routeDictionary = new Dictionary<string, string>();
            var rq = filterContext.HttpContext.Request.QueryString;
            foreach (string key in rq.Keys)
            {
                routeDictionary[key] = rq[key];
            }
            // Add action
            routeDictionary["action"] = filterContext.RouteData.Values["action"].ToString();
            // Add controller
            routeDictionary["controller"] = filterContext.RouteData.Values["controller"].ToString();
            // Return link
            string s = string.Empty;
            foreach (KeyValuePair<string, string> kvp in routeDictionary)
            {
                s += kvp.Key + ":" + kvp.Value + ",";
            }
            filterContext.HttpContext.Response.Write(s);
            filterContext.HttpContext.RewritePath(urlHelper.Content("~/Content/index.html"));

            //filterContext.HttpContext.Response.End();

        }
    }
}