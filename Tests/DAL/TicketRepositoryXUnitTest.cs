using System;
using System.Text;
using System.Collections.Generic;
using SC.DAL;
using Xunit;
using SC.DAL.EF;
using SC.BL.Domain;

namespace Tests.DAL
{
    //Test class for the TicketRepository written with the XUnit Testing Framework
    public class TicketRepositoryXUnitTest
    {
        private ITicketRepository _repo;

        [Fact]
        public void CreateTicketWithValidTicketAddsTicketToDb()
        {
            //Arrange
            _repo = new TicketRepository();

            //Act
            Ticket t = new Ticket()
            {
                DateOpened = DateTime.Now,
                State = TicketState.Closed,
                Text = "Test ticket 1"
            };

            Ticket added = _repo.CreateTicket(t);

            //Assert
            Assert.Same(t, added);
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
            var create = _repo.CreateTicket(null);

            //Assert
            Assert.Throws<ArgumentNullException>(() => create);
        }
    }
}
