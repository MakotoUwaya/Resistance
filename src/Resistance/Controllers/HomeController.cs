using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

using Resistance.Models.Game;
using Resistance.Data;

namespace Resistance.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Robby()
        {
            return View();
        }

        public ActionResult Game()
        {
            return RedirectToAction("Robby");
        }

        public ActionResult GameStart(string roomName)
        {
            var robby = new RobbyViewModel() { RoomName = roomName};
            return View("Game", robby);
        }
    }
}