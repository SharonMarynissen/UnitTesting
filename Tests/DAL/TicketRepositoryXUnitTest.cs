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

        public TicketRepositoryXUnitTest()
        {
            _repo = new TicketRepository();
            _repo.ClearDatabase();
        }

        [Fact]
        public void CreateTicketWithValidTicketAddsTicketToDb()
        {
            //Arrange
            Ticket t = new Ticket()
            {
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
                Text = "Test ticket 1"
            };

            //Act
            Ticket added = _repo.CreateTicket(t);
            
            //Assert
            Assert.Contains(t, _repo.ReadTickets());
            Assert.Equal(t.DateOpened, added.DateOpened);
            Assert.Equal(t.State, added.State);
            Assert.Equal(t.Text, added.Text);
        }

        [Fact]
        public void CreateTicketTicketIsNullThrowsArgumentNullException()
        {
            //Act + Assert
            Assert.Throws<ArgumentNullException>(() => _repo.CreateTicket(null));
        }

        [Fact]
        public void ReadTicketsReturnsNonEmptyList()
        {
            //Act
            var tickets = _repo.ReadTickets();

            //Assert
            Assert.Empty(tickets);
        }

        //Weet niet welke assert te gebruiken
        [Fact]
        public void ReadTicketsAllItemsInReturnedListAreOfTypeTicket()
        {
            //Arragnge
            _repo.CreateTicket(new Ticket
            {
                Text = "Dummy ticket",
                AccountId = 1,
                State = TicketState.Answered,
                DateOpened = DateTime.Now
            });

            _repo.CreateTicket(new Ticket
            {
                Text = "Dummy ticket 2",
                AccountId = 2,
                State = TicketState.Answered,
                DateOpened = DateTime.Now
            });

            _repo.CreateTicket(new HardwareTicket()
            {
                DeviceName = "PC-1234",
                Text = "Dummy ticket",
                AccountId = 1,
                State = TicketState.Answered,
                DateOpened = DateTime.Now
            });

            //Act 
            var tickets = _repo.ReadTickets().ToList();

            //Assert
            tickets.ForEach(t=>Assert.IsAssignableFrom<Ticket>(t));
        }

        [Fact]
        public void ReadTicketWithExistingIdReturnsExcpectedTicket()
        {
            //Arrange
            String expected = "This should be the message";

            //Act
            var id = _repo.CreateTicket(new Ticket() { Text = expected, DateOpened = DateTime.Now }).TicketNumber;

            //Assert
            Assert.Equal(_repo.ReadTicket(id).Text, expected);
        }
        
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
            var originalText = "This was the original message";
            var newText = "This is the new message";
            var date = DateTime.Now;
            var id = _repo.CreateTicket(new Ticket() { DateOpened = date, Text = originalText }).TicketNumber;
            var ticketToUpdate = _repo.ReadTicket(id);

            //Assert (to verify starting conditions)
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
            //Assert and act           
            Assert.Throws<ArgumentNullException>(() => _repo.UpdateTicket(null));
        }

        [Fact]
        public void UpdateTicketStateToClosedOnExistingTicketSetsStateToClosed()
        {
            //Arrange
            var id = _repo.CreateTicket(new Ticket() { DateOpened = DateTime.Now, Text = "Some text" }).TicketNumber;

            //Assert (to verify starting conditions)
            Assert.NotEqual(_repo.ReadTicket(id).State, TicketState.Closed);

            //Act
            _repo.UpdateTicketStateToClosed(id);

            //Assert
            Assert.Equal(_repo.ReadTicket(id).State, TicketState.Closed);
        }

        [Fact]
        public void UpdateTicketStateToClosedOnNonExistingIdThrowsKeyNotFoundException()
        {
            //Assert and act
            Assert.Throws<KeyNotFoundException>(() =>_repo.UpdateTicketStateToClosed(0));
        }

        //Geen idee hoe de try catch te doen
        [Fact]
        public void DeleteTicketWithExistingTicketRemovesTicketFromDatabase()
        {
            //Arrange
            int id = _repo.CreateTicket(new Ticket {
                Text = "Dummy ticket",
                AccountId = 1,
                State = TicketState.Answered,
                DateOpened = DateTime.Now
            }).TicketNumber;

            //Verify starting conditions
            Assert.NotNull(_repo.ReadTicket(id));

            //Act
            _repo.DeleteTicket(id);

            //Assert
            Assert.Throws<KeyNotFoundException>(()=>_repo.ReadTicket(id));
        }

        [Fact]
        public void DeleteTicketWithNonExistingIdThrowsKeyNotFoundException()
        {
            //Act + Assert
            Assert.Throws<KeyNotFoundException>(() => _repo.DeleteTicket(0));
        }

        [Fact]
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
            var id = _repo.CreateTicket(testTicket).TicketNumber;
            _repo.CreateTicketResponse(tr);

            //Act
            List<TicketResponse> result = _repo.ReadTicketResponsesOfTicket(id).ToList();

            //Assert
            Assert.Contains(tr, result);
        }

        [Fact]
        public void ReadTicketResponsesOfNonExistingTicketReturnsEmptyList()
        {
            //Act
            var tickets = _repo.ReadTicketResponsesOfTicket(0).ToList();

            //Assert
            Assert.Equal(0, tickets.Count);
        }

        [Fact]
        public void CreateTicketResponseWithNullAsTicketResponseThrowsException()
        {
            //Act + Assert
            Assert.Throws<ArgumentNullException>(() => _repo.CreateTicketResponse(null));
        }
    }
}
