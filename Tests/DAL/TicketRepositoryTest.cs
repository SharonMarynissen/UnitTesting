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
        //private TicketRepository repo;

        //[TestInitialize]
        //private void Init()
        //{
        //    repo = new TicketRepository();
        //}

        [TestMethod]
        public void TestCreateTicket()
        {
            ITicketRepository repo = new TicketRepository();

            Ticket t1 = new Ticket()
            {
                AccountId = 5,
                Text = "Dit ticket is aangemaakt om te testen",
                DateOpened = DateTime.Now,
                State = TicketState.Open, 
            };

            repo.CreateTicket(t1);

           //CollectionAssert.Contains((List<Ticket>)repo.ReadTickets(), t1, "Databank bevat het nieuwe ticket niet");
        }
    }
}
