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

        [Fact]
        public void ChangeTicketWithInvalidTicketThrowsValidationException()
        {
            _repository = Substitute.For<ITicketRepository>();
            _mgr = new TicketManager(_repository);
            Ticket invalidTicket = new Ticket {
                AccountId = 1,
                TicketNumber = 1,
                State = TicketState.Open,
                DateOpened = DateTime.Now,
                Responses = new List<TicketResponse>()
            };

            Assert.Throws<ValidationException>(() => _mgr.ChangeTicket(invalidTicket));
        }

        [Fact]
        public void ChangeTicketWithNullAsParameterThrowsNullReferenceException()
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

        [Theory, MemberData(nameof(AddTicketResponseWithInvalidResponseThrowsValidationExceptionMemberData))]
        public void AddTicketResponseWithInvalidResponseThrowsValidationException(string useCase, string response)
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

        public static IEnumerable<object[]> AddTicketResponseWithInvalidResponseThrowsValidationExceptionMemberData()
        {
            yield return new[] { "AddTicketResponseWithEmptyStringAsResponse", "" };
            yield return new[] { "AddTicketResponseWithTooLongStringAsResponse", "This response should be way too long, I have no clue what to type so I just keep typing some random words. Testing with xUnit is a lot easier than testin with MUTF, because you can use a Theory with MemberData that makes it possible to run a test multiple times with different parameters. This comes in handy when testing edge cases." };
            yield return new[] { "AddTicketResponseWithNullAsResponse", null };
        }
    }
}