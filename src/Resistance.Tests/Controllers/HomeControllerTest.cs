using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var controller = new HomeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Robby()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Robby() as ViewResult;

            // Assert
            //Assert.AreEqual("Your application description page.", result.ViewBag.Message);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Game()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Game() as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
