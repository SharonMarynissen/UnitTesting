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
    //Test class for TicketRepository written with the Microsof Unit Testing Framework
    [TestClass]
    public class TicketRepositoryMsTest
    {
        private ITicketRepository _repo = new TicketRepository();

        //This is the arrange for all of the methods used in this class
        [TestInitialize]
        public void SetUp()
        {
            _repo.ClearDatabase(); //This method is created to clear the database. This should make us independent from te Seed implementation!
        }

        [TestMethod]
        public void CreateTicketWithValidTicketAddsTicketToDb()
        {
            //Arrange
            var t = new Ticket()
            {
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
                Text = "Test ticket 1"
            };

            //Act
            var added = _repo.CreateTicket(t);

            //Assert
            Assert.AreEqual(t.DateOpened, added.DateOpened, "Dates should be equal");
            Assert.AreEqual(t.State, added.State, "States should be equal");
            Assert.AreEqual(t.Text, added.Text, "Texts should be equal");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CreateTicketTicketIsNullThrowsArgumentNullException() {
            //Act
            _repo.CreateTicket(null);

            //Assert
            Assert.Fail("No exception was thrown");
        }

        [TestMethod]
        public void ReadTicketsReturnsEmptyList()
        {
            //No arrange needed. Initialization has been done in TestInitialize
            //Act
            var tickets = _repo.ReadTickets();

            //Assert
            Assert.IsNotNull(tickets);
            Assert.IsInstanceOfType(tickets, typeof(IEnumerable<Ticket>));
            Assert.IsFalse(tickets.Any());
            Assert.AreEqual(0, tickets.Count(), 0.0, "Ticket list should be empty");
        }

        [TestMethod]
        public void ReadTicketsAllItemsInReturnedListAreOfTypeTicket() {
            //Act 
            var tickets = _repo.ReadTickets().ToList();

            //Assert
            CollectionAssert.AllItemsAreInstancesOfType(tickets, typeof(Ticket), "All items in the list should be a Ticket type");
        }

        [TestMethod]
        public void ReadTicketWithExistingIdReturnsExcpectedTicket()
        {
            //Arrange
            string expected = "This should be the message";
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            //Act + Assert at once
            Assert.AreEqual(_repo.ReadTicket(id).Text, expected, "Not the correct ticket");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void ReadTicketWithUnexistingIdReturnsNull()
        {
            //Act
            _repo.ReadTicket(0);

            //Assert
            Assert.Fail("No exception was thrown");
        }

        [TestMethod]
        public void UpdateTicketUpdateExistingTicketTextOfTicketIsUpdatedToNewValueAfterUpdate()
        {
            //Arrange
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            //Assert (to verify starting conditions)
            Assert.AreEqual(ticketToUpdate.Text, originalText);
            
            //Act
            ticketToUpdate.Text = newText;
            _repo.UpdateTicket(ticketToUpdate);

            //Assert
            Assert.AreEqual(_repo.ReadTicket(id).Text, newText, "The text wasn't updated");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateTicketNonExistingTicketThrowsKeyNotFoundException()
        {
            //Arrange
            Ticket t = new Ticket
            {
                TicketNumber = Int32.MaxValue,
                Text = "This ticket is not in the database",
                AccountId = 987,
                State = TicketState.Answered,
                DateOpened = DateTime.Now,
            };

            //Act
            _repo.UpdateTicket(t);

            //Assert
            Assert.Fail("No exception was thrown");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void UpdateTicketTicketIsNullThrowsArgumentNullException()
        {
            //Act
            _repo.UpdateTicket(null);

            //Assert
            Assert.Fail("No exception was thronw");
        }

        [TestMethod]
        public void UpdateTicketStateToClosedOnExistingTicketSetsStateToClosed()
        {
            //Arrange
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;

            //Assert (to verify starting conditions)
            Assert.AreNotEqual(_repo.ReadTicket(id).State, TicketState.Closed, "Ticketstate should not be closed yet");

            //Act
            _repo.UpdateTicketStateToClosed(id);

            //Assert
            Assert.AreEqual(_repo.ReadTicket(id).State, TicketState.Closed, "Ticketstate should be closed");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateTicketStateToClosedOnNonExistingIdThrowsKeyNotFoundException()
        {
            //Act
            _repo.UpdateTicketStateToClosed(0);

            //Assert
            Assert.Fail("This Should throw a NullReferenceException");
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteTicketWithExistingTicketRemovesTicketFromDatabase()
        {
            //Arrange
            int id = _repo.CreateTicket(new Ticket
            {
                Text = "Dummy ticket",
                AccountId = 1,
                State = TicketState.Open,
                DateOpened = DateTime.Today
            }).TicketNumber;

            //Act
            try
            {
                _repo.DeleteTicket(id);
            }
            catch (Exception)
            {
                Assert.Fail("This should not throw an excepion yet");
            }
            _repo.ReadTicket(id);

            //Assert
            Assert.Fail("Reading ticket that is not in database should throw an exception");     
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteTicketWithNonExistingIdThrowsKeyNotFoundException()
        {
            //Act
            _repo.DeleteTicket(0);

            //Assert
            Assert.Fail("No exception was thrown when removing a non excisting ticket");
        }

        [TestMethod]
        public void ReadTicketResponsesOfTicketWithIdOneReturnsAListOfTicketResponses()
        {
            //Arrange
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

            //Act
            var id = _repo.CreateTicket(testTicket).TicketNumber;
            _repo.CreateTicketResponse(tr);
            var result = _repo.ReadTicketResponsesOfTicket(id).ToList();

            //Assert
            CollectionAssert.Contains(result, tr, "The ticket response is not in the list");
        }

        [TestMethod]
        public void ReadTicketResponsesOfNonExistingTicketReturnsEmptyList()
        {
            //Act
            var tickets = _repo.ReadTicketResponsesOfTicket(0).ToList();

            //Assert
            Assert.AreEqual(0, tickets.Count, "The size of the list should be 0");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CreateTicketResponseWithNullAsTicketResponseThrowsException()
        {
            //Act
            _repo.CreateTicketResponse(null);

            //Assert
            Assert.Fail("No exception was thrown");
        }
    }
}