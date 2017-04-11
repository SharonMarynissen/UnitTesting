using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC.UI.Web.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using NSubstitute;
using SC.BL;
using SC.BL.Domain;

namespace Tests.Controllers
{
    [TestClass]
    public class TicketControllerTests
    {
        private TicketController _controller;
        private ITicketManager mgr;

        [TestInitialize]
        public void SetUp()
        {
            mgr = Substitute.For<ITicketManager>();
            _controller = new TicketController(mgr);
        }

        [TestMethod]
        public void ControllerIndexMethod_CallsGetTicketsOnManager()
        {
            _controller.Index();
            mgr.Received().GetTickets();
        }

        [TestMethod]
        public void ControllerDetailsMethod_CallsGetTicketOnManagerWithIdAndReturnsActionResult()
        {
            var result = _controller.Details(5);
            mgr.Received(1).GetTicket(5);
            Assert.IsInstanceOfType(result, typeof(ActionResult));
        }

        [TestMethod]
        public void ControllerCreateMethod_WithValidTicketAsParameter_CallsManagerAddTicket()
        {
            Ticket t = new Ticket()
            {
                DateOpened = DateTime.Now,
                Text = "Some Text",
                State = TicketState.Open,
                AccountId = 1,
            };
            var result = _controller.Create(t);
            mgr.Received(1).AddTicket(1, "Some Text");
        }

        [TestMethod]
        public void ControllerEditMethod_WithOnlyIdAsParameter_CallsMgrToGetTicket()
        {
            _controller.Edit(15);
            mgr.Received().GetTicket(15);
        }

        [TestMethod]
        public void ControllerEditMethodWithIdAndTicketAsParameter_CallsMgrUpdateTicket()
        {
            Ticket t = new HardwareTicket();
            _controller.Edit(15, t);
            mgr.Received(1).ChangeTicket(t);
        }

        [TestMethod]
        public void ControllerDelete_WithIdOnly_GetsTicketDetailsInMgr()
        {
            _controller.Delete(459);
            mgr.Received(1).GetTicket(459);
        }

        [TestMethod]
        public void ControllerDelete_WithIdAndFormCollection_CallsMgrDeleteTicket()
        {
            FormCollection frmCollection = new FormCollection();
            _controller.Delete(6, frmCollection);
            mgr.Received(1).RemoveTicket(6);
        }
    }
}