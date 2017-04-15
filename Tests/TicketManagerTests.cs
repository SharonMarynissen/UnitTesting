using SC.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NSubstitute.Core;
using SC.BL.Domain;
using SC.DAL;
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


    }
}