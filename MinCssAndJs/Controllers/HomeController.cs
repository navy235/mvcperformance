using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MinCssAndJs.Filters;

namespace MinCssAndJs.Controllers
{


    public class HomeController : Controller
    {
        //
        // GET: /Home/
        //[GzipHtml(Order = 1)]
        //[CompressHtml(Order = 2)]
        //[BundleMinifyInlineCssJs(Order = 3)]
        //[MinifyHtml]
        [GenerateStatic]
        public ActionResult Index()
        {
            return View(new MinCssAndJs.Models.Member());
        }

    }
}
