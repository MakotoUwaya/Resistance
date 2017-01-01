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

        //[HttpPost]
        //public ActionResult Robby(RobbyViewModel model)
        //{

        //    return View();
        //}

        [HttpPost]
        public ActionResult CreateRoom(RobbyViewModel model)
        {           
            if (RoomInfo.List.Where(l => l.Name == model.RoomName).Count() == 0)
            {
                var playerName = User.Identity.GetUserName();
                var player = ClientsInfo.List.Where(c => c.Name == playerName).SingleOrDefault();
                if (player != null)
                {
                    RoomInfo.AddRoom(model.RoomName, player);
                }
                else
                {
                    var newPlayer = new Core.Player("", playerName);
                    ClientsInfo.List.Add(newPlayer);
                    RoomInfo.AddRoom(model.RoomName, newPlayer);
                }
            }
            else
            {
                ModelState.AddModelError("", "このルーム名は既に登録されています。");
            }

            model.RoomName = "";
            return View("Robby", model);
        }

        public ActionResult Game()
        {
            return View();
        }
    }
}