using SC.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NSubstitute.Core;
using SC.BL.Domain;
using SC.DAL;
using SC.DAL.EF;
using Xunit;

namespace Tests
{
    
    public class TicketManagerTests
    {
        private ITicketRepository _repository;
        private ITicketManager _mgr;

        [Fact]
        public void GetTicketsCallsRepositoryReadTicketsExactlyOnce()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);
            _mgr.GetTickets();
            _repository.Received(1).ReadTickets();
        }

        [Theory, MemberData(nameof(GetTicketWithCorrectIdReturnsExpectedResultMemberData))]
        public void GetTicketWithCorrectIdReturnsExpectedResult(string useCase, int id)
        {
            _repository = Substitute.For<ITicketRepository>();
            
        }

        public static IEnumerable<object[]> GetTicketWithCorrectIdReturnsExpectedResultMemberData()
        {
            var expectedTicket = Builder<Ticket>.CreateNew()
                .With(t=>t.TicketNumber, 1)
                .With(t=>t.Text, "Ticket 1")
                .Build();
            yield return new object[] {"GetTicketWithId1ReturnsExpectedTicket", 1, expectedTicket};
        }
    }
}