using System;
using System.Text;
using System.Collections.Generic;
using SC.DAL;
using Xunit;
using SC.DAL.EF;
using SC.BL.Domain;
using System.Linq;

namespace Tests.DAL
{
    //Test class for the TicketRepository written with the XUnit Testing Framework
    //All of the methods are based on the fact that TicketRepository has a seeding method and thus contains Tickets and TicketResponses
    public class TicketRepositoryXUnitTest
    {
        private ITicketRepository _repo;

        [Fact]
        public void CreateTicketWithValidTicketAddsTicketToDb()
        {
            //Arrange
            _repo = new TicketRepository();
       
            Ticket t = new Ticket()
            {
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
                Text = "Test ticket 1"
            };

            //Act
            Ticket added = _repo.CreateTicket(t);

            //Assert
            Assert.Equal(t.DateOpened, added.DateOpened);
            Assert.Equal(t.State, added.State);
            Assert.Equal(t.Text, added.Text);
        }

        //Werkt nog niet correct!
        [Fact]
        public void CreateTicketTicketIsNullThrowsArgumentNullException()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            //var create = _repo.CreateTicket(null);

            //Assert
            Assert.Throws<ArgumentNullException>(() => _repo.CreateTicket(null));
        }

        [Fact]
        public void ReadTicketsReturnsNonEmptyList()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            var tickets = _repo.ReadTickets();

            //Assert
            Assert.NotEmpty(tickets);
        }

        //Weet niet welke assert te gebruiken
        [Fact]
        public void ReadTicketsAllItemsInReturnedListAreOfTypeTicket()
        {
            //Arragnge
            _repo = new TicketRepository();

            //Act 
            var tickets = _repo.ReadTickets().ToList();

            //Assert
            
        }

        [Fact]
        public void ReadTicketWithExistingIdReturnsExcpectedTicket()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            String expected = "This should be the message";
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            //Assert
            Assert.Equal(_repo.ReadTicket(id).Text, expected);
        }

        //Werkt niet
        [Fact]
        public void ReadTicketWithUnexistingIdReturnsNull()
        {
            //Arrange
            _repo = new TicketRepository();

            //Assert and act
            Assert.Throws<KeyNotFoundException>(()=> _repo.ReadTicket(0));
        }

        [Fact]
        public void UpdateTicketUpdateExistingTicketTextOfTicketIsUpdatedToNewValueAfterUpdate()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            //Assert
            Assert.Equal(ticketToUpdate.Text, originalText);

            //Act
            ticketToUpdate.Text = newText;
            _repo.UpdateTicket(ticketToUpdate);

            //Assert
            Assert.Equal(_repo.ReadTicket(id).Text, newText);
        }

        [Fact]
        public void UpdateTicketNonExistingTicketThrowsKeyNotFoundException()
        {
            //Arrange
            _repo = new TicketRepository();
            
            Ticket t = new Ticket
            {
                TicketNumber = Int32.MaxValue,
                Text = "This ticket is not in the database",
                AccountId = 987,
                State = TicketState.Answered,
                DateOpened = DateTime.Now,
            };   
            
            //Assert and act
            Assert.Throws<KeyNotFoundException>(() => _repo.UpdateTicket(t));
        }

        [Fact]
        public void UpdateTicketTicketIsNullThrowsArgumentNullException()
        {
            //Arrange
            _repo = new TicketRepository();
            
            //Assert and act           
            Assert.Throws<ArgumentNullException>(() => _repo.UpdateTicket(null));
        }

        [Fact]
        public void UpdateTicketStateToClosedOnExistingTicketSetsStateToClosed()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;

            //Assert
            Assert.NotEqual(_repo.ReadTicket(id).State, TicketState.Closed);

            //Act
            _repo.UpdateTicketStateToClosed(id);

            //Assert
            Assert.Equal(_repo.ReadTicket(id).State, TicketState.Closed);
        }

        [Fact]
        public void UpdateTicketStateToClosedOnNonExistingIdThrowsKeyNotFoundException()
        {
            //Arrange
            _repo = new TicketRepository();

            //Assert and act
            Assert.Throws<KeyNotFoundException>(() =>_repo.UpdateTicketStateToClosed(0));
        }

        //Werkt niet en geen idee hoe de try catch te doen
        [Fact]
        public void DeleteTicketWithExistingTicketRemovesTicketFromDatabase()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            try
            {
                _repo.DeleteTicket(1);
            }
            catch (Exception)
            {
                //Assert.Fail("This should not throw an excepion yet");
            }
            //_repo.ReadTicket(1);

            //Assert
            Assert.Throws<KeyNotFoundException>(() =>_repo.ReadTicket(1));
        }

        [Fact]
        public void DeleteTicketWithNonExistingIdThrowsKeyNotFoundException()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            //_repo.DeleteTicket(0);

            //Assert
            Assert.Throws<KeyNotFoundException>(() => _repo.DeleteTicket(0));
        }

        [Fact]
        public void ReadTicketResponsesOfTicketWithIdOneReturnsAListOfThreeTicketResponses()
        {
            //Arrange
            _repo = new TicketRepository();

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
            Assert.Contains(tr, result);
        }

        [Fact]
        public void ReadTicketResponsesOfNonExistingTicketReturnsEmptyList()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            var tickets = _repo.ReadTicketResponsesOfTicket(0).ToList();

            //Assert
            Assert.Equal(0, tickets.Count);
        }

        [Fact]
        public void CreateTicketResponseWithNullAsTicketResponseThrowsException()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            //_repo.CreateTicketResponse(null);

            //Assert
            Assert.Throws<ArgumentNullException>(() => _repo.CreateTicketResponse(null));
        }

    }
}
