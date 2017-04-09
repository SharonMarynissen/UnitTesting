using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC.DAL.EF;
using SC.BL.Domain;
using System.Collections.Generic;
using SC.DAL;

namespace Tests
{
    [TestClass]
    public class TicketRepositoryTest
    { 
        [TestMethod]
        public void TicketRepositoryIsNotNull()
        {
            ITicketRepository repo = new TicketRepository();

            Assert.IsNotNull(repo, "Repository can't be null");
        }

        //[TestMethod]
        //public void TestReadTickets()
        //{
        //    TicketRepository repo = new TicketRepository();

        //    IEnumerable<Ticket> tickets = repo.ReadTickets();

        //    Assert.IsNotNull(tickets, "Ticket list can not be empty");
        //}


        //[TestMethod]
        //public void TestCreateTicket()
        //{
        //    //TicketRepository repo = new TicketRepository();

        //    Ticket t1 = new Ticket()
        //    {
        //        AccountId = 5,
        //        Text = "Dit ticket is aangemaakt om te testen",
        //        DateOpened = DateTime.Now,
        //        State = TicketState.Open, 
        //    };

        //    //repo.CreateTicket(t1);

        //   //CollectionAssert.Contains((List<Ticket>)repo.ReadTickets(), t1, "Database doesn't contain the new ticket");
        //}
    }
}
