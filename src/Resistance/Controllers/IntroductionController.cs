using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Resistance.Controllers
{
    [AllowAnonymous]
    public class IntroductionController : Controller
    {
        // GET: Introduction
        public ActionResult Story()
        {
            return View();
        }

        public ActionResult GameRule()
        {
            return View();
        }

        public ActionResult PlotCardList()
        {
            return View();
        }
    }
}