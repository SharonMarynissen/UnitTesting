using SC.BL;
using SC.BL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SC.UI.Web.Mvc.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketManager mgr = new TicketManager();

        // GET: Ticket
        public ActionResult Index()
        {
            return View(mgr.GetTickets());
        }

        // GET: Ticket/Details/5
        public ActionResult Details(int id)
        {
            Ticket t = mgr.GetTicket(id);

            ViewBag.Responses = new List<TicketResponse>(mgr.GetTicketResponses(id));
            return View(t);
        }

        // GET: Ticket/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ticket/Create
        [HttpPost]
        public ActionResult Create(Ticket ticket)
        {
            // TODO: Add insert logic here
            if (ModelState.IsValid)
            {
                mgr.AddTicket(ticket.AccountId, ticket.Text);
                return RedirectToAction("Details", new { id = ticket.TicketNumber });
            }

            return View();
        }

        // GET: Ticket/Edit/5
        public ActionResult Edit(int id)
        {
            return View(mgr.GetTicket(id));
        }

        // POST: Ticket/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Ticket ticket)
        {
            try
            {
                // TODO: Add update logic here
                mgr.ChangeTicket(ticket);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Ticket/Delete/5
        public ActionResult Delete(int id)
        {
            return View(mgr.GetTicket(id));
        }

        // POST: Ticket/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                mgr.RemoveTicket(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
