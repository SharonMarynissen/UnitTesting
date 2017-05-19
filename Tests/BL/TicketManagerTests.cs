using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SC.BL;
using SC.BL.Domain;
using SC.DAL;
using Xunit;

namespace Tests.BL
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

        [Theory] 
        [MemberData(nameof(GetTicketWithIdReturnsExpectedResultMemberData))]
        public void GetTicketWithCorrectIdReturnsExpectedResult(int id)
        {
            Ticket t = new Ticket();
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(id).ReturnsForAnyArgs(t);
            _mgr = new TicketManager(_repository);
            Assert.Equal(t, _mgr.GetTicket(id));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GetTicketWithCorrectIdReturnsExpectedResultWithInlineData(int id) {
            Ticket t = new Ticket();
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(id).ReturnsForAnyArgs(t);
            _mgr = new TicketManager(_repository);
            Assert.Equal(t, _mgr.GetTicket(id));
        }

        [Theory]
        [MemberData(nameof(GetTicketWithIdReturnsExpectedResultMemberData))]
        public void GetTicketWithUnknownIdThrowsException(int id)
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
        public void AddTicketWithInvalidQuestionThrowsValidationException(string question)
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            Assert.Throws<ValidationException>(() => _mgr.AddTicket(1, question));

        }

        [Fact]
        public void ChangeTicketWithValidTicketCallsRepositoryUpdateTicket()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);
            Ticket validTicket = new Ticket
            {
                Text = "This should be valid",
                AccountId = 1,
                TicketNumber = 1,
                State = TicketState.Open,
                DateOpened = DateTime.Now,
                Responses = new List<TicketResponse>()
            };

            _mgr.ChangeTicket(validTicket);

            _repository.Received(1).UpdateTicket(Arg.Is(validTicket));
        }

        [Theory, MemberData(nameof(ChangeTicketWithInvalidTicketThrowsValidationExceptionMemberData))]
        public void ChangeTicketWithInvalidTicketThrowsValidationException(string text)
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);
            Ticket invalidTicket = new Ticket {
                Text = text,
                AccountId = 1,
                TicketNumber = 1,
                State = TicketState.Open,
                DateOpened = DateTime.Now,
                Responses = new List<TicketResponse>()
            };

            Assert.Throws<ValidationException>(() => _mgr.ChangeTicket(invalidTicket));
        }

        [Fact]
        public void ChangeTicketWithNullAsParameterThrowsArgumentNullException()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            Assert.Throws<ArgumentNullException>(() => _mgr.ChangeTicket(null));
        }

        [Fact]
        public void RemoveTicketCallsRepositoryDeleteTicketWithTheSameTicketNumber()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            _mgr.RemoveTicket(5);
            _repository.Received(1).DeleteTicket(5);
        }

        [Fact]
        public void GetTicketResponsesCallsRepositoryReadTicketResponsesOfTicket()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            _mgr.GetTicketResponses(5);

            _repository.Received(1).ReadTicketResponsesOfTicket(5);
        }

        [Theory] 
        [MemberData(nameof(AddTicketResponseWithInvalidResponseThrowsValidationExceptionMemberData))]
        public void AddTicketResponseWithInvalidResponseThrowsValidationException(string response)
        {
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(5).Returns(new Ticket());
            _mgr = new TicketManager(_repository);

            Assert.Throws<ValidationException>(() => _mgr.AddTicketResponse(5, response, true));
        }

        [Fact]
        public void AddTicketResponseToUnknownTicketThrowsArgumentException()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);

            Assert.Throws<KeyNotFoundException>(() => _mgr.AddTicketResponse(0, "Some response", false));
        }

        [Fact]
        public void AddTicketResponseWithValidTicketResponseCallsRepoMethodsCreateTicketResponseAndUpdateTicket()
        {
            Ticket t = new Ticket
            {
                State = TicketState.Answered,
                Text = "Some fake ticket",
                AccountId = 23,
                TicketNumber = 1,
                DateOpened = DateTime.Now
            };
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(1).Returns(t);
            _mgr = new TicketManager(_repository);

            _mgr.AddTicketResponse(1, "Valid response", false);

            _repository.Received(1).CreateTicketResponse(Arg.Any<TicketResponse>());
            _repository.Received(1).UpdateTicket(t);
        }

        [Fact]
        public void AddTicketResponseWithValidTicketResponseSetsTicketStateToClientAnswerIfItWasAClientResponse()
        {
            Ticket t = new Ticket {
                State = TicketState.Answered,
                Text = "Some fake ticket",
                AccountId = 23,
                TicketNumber = 1,
                DateOpened = DateTime.Now
            };
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(1).Returns(t);
            _mgr = new TicketManager(_repository);

            _mgr.AddTicketResponse(1, "Valid response", true);

            Assert.Equal(TicketState.ClientAnswer, t.State);
        }

        [Fact]
        public void AddTicketResponseWithValidTicketResponseSetsTicketStateToAnsweredIfItWasNotAClientResponse() {
            Ticket t = new Ticket {
                State = TicketState.Answered,
                Text = "Some fake ticket",
                AccountId = 23,
                TicketNumber = 1,
                DateOpened = DateTime.Now
            };
            _repository = Substitute.For<ITicketRepository>();
            _repository.ReadTicket(1).Returns(t);
            _mgr = new TicketManager(_repository);

            _mgr.AddTicketResponse(1, "Valid response", false);

            Assert.Equal(TicketState.Answered, t.State);
        }


        //Theory MemberData methods
        public static IEnumerable<object[]> GetTicketWithIdReturnsExpectedResultMemberData() {
            yield return new object[] { 1 };
            yield return new object[] { 2 };
            yield return new object[] { 3 };
            yield return new object[] { 4 };
            yield return new object[] { 5 };
        }

        public static IEnumerable<object[]> AddTicketWithAccountIdAndQuestionReturnsNewTicketMemberData()
        {
            yield return new object[] { "This is a stupid question", 1};
            yield return new object[] { "This is also a stupid question", 1 };
            yield return new object[] { "This is also a stupid question", 15 };
            yield return new object[] { "This is also a stupid question", 0 };
        }

        public static IEnumerable<object[]> AddTicketWithInvalidQuestionThrowsValidationExceptionMemberData() {
            yield return new object[] { "This question is way to long and because of that it should throw a validation error. I am curious or this will be the case indeed, but it actually should." };          
            yield return new object[] { "" };
            yield return new object[] { null };
        }

        public static IEnumerable<object[]> AddTicketResponseWithInvalidResponseThrowsValidationExceptionMemberData()
        {
            yield return new object[] { "" };
            yield return new object[] { "This response should be way too long, I have no clue what to type so I just keep typing some random words. Testing with xUnit is a lot easier than testin with MS UTF, because you can use a Theory with MemberData that makes it possible to run a test multiple times with different parameters. This comes in handy when testing edge cases." };
            yield return new object[] { null };
        }

        public static IEnumerable<object[]> ChangeTicketWithInvalidTicketThrowsValidationExceptionMemberData() {
            yield return new object[] { "" };
            yield return new object[] { "This response should be way too long, I have no clue what to type so I just keep typing some random words. Testing with xUnit is a lot easier than testin with MUTF, because you can use a Theory with MemberData that makes it possible to run a test multiple times with different parameters. This comes in handy when testing edge cases." };
            yield return new object[] { null };
        }
    }
}