using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC.BL.Domain;
using SC.DAL;
using SC.DAL.EF;

namespace Tests.DAL
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
        public void DropAndCreateDatabaseShouldNotThrowError()
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
            Assert.AreEqual(t.DateOpened, added.DateOpened, "Dates should be equal");
            Assert.AreEqual(t.State, added.State, "States should be equal");
            Assert.AreEqual(t.Text, added.Text, "Texts should be equal");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CreateTicketTicketIsNullThrowsArgumentNullException() {
            _repo.CreateTicket(null);
        }

        [TestMethod]
        public void ReadTicketsReturnsNonEmptyList()
        {
            var tickets = _repo.ReadTickets();

            Assert.IsNotNull(tickets, "Ticket list can not be empty");
        }

        [TestMethod]
        public void ReadTicketsAllItemsInReturnedListAreOfTypeTicket() {
            CollectionAssert.AllItemsAreInstancesOfType(_repo.ReadTickets().ToList(), typeof(Ticket));
        }

        [TestMethod]
        public void ReadTicketWithExistingIdReturnsExcpectedTicket()
        {
            var expected = "This should be the message";
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            Assert.AreEqual(_repo.ReadTicket(id).Text, expected, "Not the correct ticket");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void ReadTicketWithUnexistingIdReturnsNull()
        {
            _repo.ReadTicket(int.MaxValue);
            Assert.Fail("No exception was thrown");
        }

        [TestMethod]
        public void UpdateTicketUpdateExistingTicketTextOfTicketIsUpdatedToNewValueAfterUpdate()
        {
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            Assert.AreEqual(ticketToUpdate.Text, originalText);
            ticketToUpdate.Text = newText;
            _repo.UpdateTicket(ticketToUpdate);

            Assert.AreEqual(_repo.ReadTicket(id).Text, newText, "The text wasn't updated");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateTicketNonExistingTicketThrowsKeyNotFoundException()
        {
            Ticket t = new Ticket
            {
                TicketNumber = Int32.MaxValue,
                Text = "This ticket is not in the database",
                AccountId = 987,
                State = TicketState.Answered,
                DateOpened = DateTime.Now,
            };
            _repo.UpdateTicket(t);
            Assert.Fail("No exception was thrown");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateTicketTicketIsNullThrowsArgumentNullException()
        {
            _repo.UpdateTicket(null);
            Assert.Fail("No exception was thronw");
        }

        [TestMethod]
        public void UpdateTicketStateToClosedOnExistingTicketSetsStateToClosed()
        {
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;
            Assert.AreNotEqual(_repo.ReadTicket(id).State, TicketState.Closed, "Ticketstate should not be closed yet");
            _repo.UpdateTicketStateToClosed(id);
            Assert.AreEqual(_repo.ReadTicket(id).State, TicketState.Closed, "Ticketstate should be closed");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException), "This Should throw a NullReferenceException")]
        public void UpdateTicketStateToClosedOnNonExistingIdThrowsKeyNotFoundException()
        {
                _repo.UpdateTicketStateToClosed(int.MaxValue);

        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteTicketWithExistingTicketRemovesTicketFromDatabase()
        {
            try
            {
                _repo.DeleteTicket(1);
            }
            catch (Exception)
            {
                Assert.Fail("This should not throw an excepion yet");
            }
            _repo.ReadTicket(1);
            Assert.Fail("Reading ticket that is not in database should throw an exception");     
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteTicketWithNonExistingIdThrowsKeyNotFoundException()
        {
            _repo.DeleteTicket(Int32.MaxValue);
            Assert.Fail("No exception was thrown when removing a non excisting ticket");
        }

        [TestMethod]
        public void ReadTicketResponsesOfTicketWithIdOneReturnsAListOfThreeTicketResponses()
        {
            Ticket testTicket = new Ticket
            {
                Text = "Test ticket",
                AccountId = 1,
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
            };

            TicketResponse tr = new TicketResponse
            {
                Date = DateTime.Today.AddDays(1),
                IsClientResponse = false,
                Text = "Answer 1",
                Ticket = testTicket,
            };

            var id = _repo.CreateTicket(testTicket).TicketNumber;
            _repo.CreateTicketResponse(tr);
            List<TicketResponse> result = _repo.ReadTicketResponsesOfTicket(id).ToList();

            CollectionAssert.Contains(result, tr, "The ticket response is not in the list");
        }

        [TestMethod]
        public void ReadTicketResponsesOfNonExistingTicketReturnsEmptyList()
        {
            Assert.AreEqual(0, _repo.ReadTicketResponsesOfTicket(Int32.MaxValue).ToList().Count, "The size of the list should be 0");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CreateTicketResponseWithNullAsTicketResponseThrowsException()
        {
            _repo.CreateTicketResponse(null);
            Assert.Fail("No exception was thrown");
        }
    }
}