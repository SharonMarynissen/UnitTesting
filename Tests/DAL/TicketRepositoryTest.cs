﻿using System;
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
    //All of the methods are based on the fact that TicketRepository has a seeding method and thus contains Tickets and TicketResponses
    [TestClass]
    public class TicketRepositoryTests
    {
        private ITicketRepository _repo;

        //This is the arrange for all of the methods used in this class
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

        //Ik zou deze verwijderen. Dit is niet echt het testen van een drop en create maar het op null zetten en het creëeren
        //-> creatie wordt zowiezo al gedaan in setup dus is logisch dat dit lukt
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

        //We use the seed method of TicketRepository, so _repo should contain tickets
        [TestMethod]
        public void ReadTicketsReturnsNonEmptyList()
        {
            //Act
            var tickets = _repo.ReadTickets();

            //Assert
            Assert.AreNotEqual(0, tickets.Count(), 0.0, "Ticket list can not be empty");
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
            //Act
            string expected = "This should be the message";
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            //Assert
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
            //Act
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            //Assert
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
            //Act
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;

            //Assert
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
            //Act
            try
            {
                _repo.DeleteTicket(1);
            }
            catch (Exception)
            {
                Assert.Fail("This should not throw an excepion yet");
            }
            _repo.ReadTicket(1);

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
        public void ReadTicketResponsesOfTicketWithIdOneReturnsAListOfThreeTicketResponses()
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
            List<TicketResponse> result = _repo.ReadTicketResponsesOfTicket(id).ToList();

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