using System;
using SC.BL;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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

        [Theory, MemberData(nameof(GetTicketWithIdReturnsExpectedResultMemberData))]
        public void GetTicketWithCorrectIdReturnsExpectedResult(string useCase, int id)
        {
            Ticket t = new Ticket();
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(id).ReturnsForAnyArgs(t);
            _mgr = new TicketManager(_repository);
            Assert.Equal(t, _mgr.GetTicket(id));
        }

        [Theory, MemberData(nameof(GetTicketWithIdReturnsExpectedResultMemberData))]
        public void GetTicketWithUnknownIdThrowsException(string useCase, int id)
        {
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(Arg.Any<int>()).ReturnsNullForAnyArgs();
            _mgr = new TicketManager(_repository);

            Assert.Throws<KeyNotFoundException>(()=>_mgr.GetTicket(id));
        }

        [Fact]
        public void GetTicketWithNegativeNumberAsTicketNumberThrowsArgumentException()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            Assert.Throws<ArgumentException>(()=>_mgr.GetTicket(-1));
        }

        [Theory, MemberData(nameof(AddTicketWithAccountIdAndQuestionReturnsNewTicketMemberData))]
        public void AddTicketWithAccountIdAndQuestionReturnsNewTicket(string question, int accountId)
        {
            _repository = Substitute.For<ITicketRepository>();
            _repository.CreateTicket(Arg.Any<Ticket>()).Returns(new Ticket
            {
                Text = question,
                AccountId = accountId,
                TicketNumber = 1
            });
            _mgr = new TicketManager(_repository);

            Ticket t = _mgr.AddTicket(accountId, question);

            Assert.Equal(question, t.Text);
            Assert.Equal(accountId, t.AccountId);
        }

        [Theory, MemberData(nameof(AddTicketWithInvalidQuestionThrowsValidationExceptionMemberData))]
        public void AddTicketWithInvalidQuestionThrowsValidationException(string useCase, string question)
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            Assert.Throws<ValidationException>(() => _mgr.AddTicket(1, question));

        }

//Theory MemberData methods
        public static IEnumerable<object[]> GetTicketWithIdReturnsExpectedResultMemberData() {
            yield return new object[] { "GetTicketWithId1ReturnsExpectedTicket", 1 };
            yield return new object[] { "GetTicketWithId2ReturnsExpectedTicket", 2 };
            yield return new object[] { "GetTicketWithId3ReturnsExpectedTicket", 3 };
            yield return new object[] { "GetTicketWithId4ReturnsExpectedTicket", 4 };
            yield return new object[] { "GetTicketWithId5ReturnsExpectedTicket", 5 };
        }

        public static IEnumerable<object[]> AddTicketWithAccountIdAndQuestionReturnsNewTicketMemberData()
        {
            yield return new object[] { "Dit is een stomme vraag", 1};
            yield return new object[] { "Dit is ook een stomme vraag", 1 };
            yield return new object[] { "Dit is ook een stomme vraag", 15 };
            yield return new object[] { "Dit is ook een stomme vraag", 0 };
        }

        public static IEnumerable<object[]> AddTicketWithInvalidQuestionThrowsValidationExceptionMemberData() {
            yield return new object[] { "AddTicketWithTooLongQuestionThrowsValidationException","Deze vraag is hoe dan ook veel te lang en zou daarom een validatiefout moeten opgooien. Ik ben eens nieuwsgierig of dit inderdaad het geval zal zijn, maar het zou eigenlijk wel moeten." };
            yield return new object[] { "AddTicketWithEmptyStringAsQuestionThrowsValidationException","" };
            yield return new object[] { "AddTicketWithNullAsQuestionThrowsValidationException",null };
        }
    }
}