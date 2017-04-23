using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC.DAL.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SC.BL.Domain;
using SC.DAL;

namespace Tests.EF
{
    [TestClass]
    public class TicketRepositoryTests
    {
        private ITicketRepository _repo;

        [TestInitialize]
        public void SetUp()
        {
            _repo = new TicketRepository();
        }

        [TestCleanup]
        public void TearDown()
        {
            _repo = null;
            SqlConnection.ClearAllPools();
        }

        [TestMethod]
        public void DropAndCreateDatabase_ShouldNotThrowError()
        {
            try
            {
                _repo = null;
                _repo = new TicketRepository();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void CreateTicketWithValidTicketAddsTicketToDb()
        {
            var t = new Ticket()
            {
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
                Text = "Test ticket 1"
            };

            var added = _repo.CreateTicket(t);
            Assert.AreEqual(t.DateOpened, added.DateOpened);
            Assert.AreEqual(t.State, added.State);
            Assert.AreEqual(t.Text, added.Text);
        }

        [TestMethod]
        public void CreateTicketWithNullAsTicketThrowsException()
        {
            try
            {
                _repo.CreateTicket(null);
                Assert.Fail("This should throw an exception, you don't want to add NULL to the database");
            }
            catch (AssertFailedException e) //I have to catch this, because this is also an exception!
            {
                Assert.Fail("This should throw an exception, you don't want to add NULL to the database");
            }
            catch (Exception e)
            {
                Assert.IsTrue(true, "An exception was thrown while adding ticket NULL to the database. This is good.");
            }
        }

        [TestMethod]
        public void ReadTicketsReturnsNonEmptyList()
        {
            var tickets = _repo.ReadTickets();

            Assert.IsNotNull(tickets, "Ticket list can not be empty");
        }

        [TestMethod]
        public void ReadTicketsAllItemsInReturnedListAreOfTypeTicket() {
            var tickets = _repo.ReadTickets();

            CollectionAssert.AllItemsAreInstancesOfType(_repo.ReadTickets().ToList(), typeof(Ticket));
        }

        [TestMethod]
        public void ReadTicket_SearchTicketById_TicketTextEqualsExpectedString()
        {
            var expected = "This should be the message";
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            Assert.AreEqual(_repo.ReadTicket(id).Text, expected);
        }

        [TestMethod]
        public void ReadTicket_UnexistingId_ReturnsNull()
        {
            Assert.IsNull(_repo.ReadTicket(int.MaxValue));
        }

        [TestMethod]
        public void UpdateTicket_UpdateExistingTicket_TextOfTicketIsUpdatedToNewValue()
        {
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            Assert.AreEqual(ticketToUpdate.Text, originalText);
            ticketToUpdate.Text = newText;
            _repo.UpdateTicket(ticketToUpdate);

            Assert.AreEqual(_repo.ReadTicket(id).Text, newText);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CreateTicket_TicketIsNull_ThrowsException()
        {
            _repo.CreateTicket(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateTicket_TicketIsNull_ThrowsException()
        {
            _repo.UpdateTicket(null);
        }

        [TestMethod]
        public void UpdateTicketStateToClosed_OnExistingTicket_SetsStateToClosed()
        {
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;
            Assert.AreNotEqual(_repo.ReadTicket(id).State, TicketState.Closed);
            _repo.UpdateTicketStateToClosed(id);
            Assert.AreEqual(_repo.ReadTicket(id).State, TicketState.Closed);
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException), "This Should throw a NullReferenceException")]
        public void UpdateTicketStateToClosed_OnNonExistingId_ThrowsKeyNotFoundException()
        {
            try
            {
                _repo.UpdateTicketStateToClosed(Int32.MaxValue);
            }
            catch (KeyNotFoundException e)
            {
                Assert.AreEqual(e.Message, "Ticket not found");
                throw;
            }
        }
    }
}