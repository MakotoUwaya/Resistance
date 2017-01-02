using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Resistance;
using Resistance.Controllers;

namespace Resistance.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Robby()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Robby() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Game()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Game() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
