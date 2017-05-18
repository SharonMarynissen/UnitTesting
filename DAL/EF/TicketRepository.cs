using System;
using System.Collections.Generic;
using System.Linq;
using SC.BL.Domain;
using System.Data.Entity;

namespace SC.DAL.EF
{
    public class TicketRepository : ITicketRepository
    {
        private readonly SupportCenterDbContext _ctx;

        public TicketRepository()
        {
            _ctx = new SupportCenterDbContext();
            _ctx.Database.Initialize(false); //initializatie enkel uitvoeren indien nog niet eerder uitgevoerd.
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            _ctx.Tickets.Add(ticket);
            _ctx.SaveChanges();
            return ticket;
        }

        public TicketResponse CreateTicketResponse(TicketResponse response)
        {
            _ctx.TicketResponses.Add(response);
            _ctx.SaveChanges();
            return response;
        }

        public void DeleteTicket(int ticketNumber)
        {
            var t = ReadTicket(ticketNumber);
            _ctx.Tickets.Remove(t);
            _ctx.SaveChanges();
        }

        public Ticket ReadTicket(int ticketNumber)
        {
            Ticket t = _ctx.Tickets.Find(ticketNumber);
            if (t == null) throw new KeyNotFoundException(String.Format("Ticket with id {0} was not found", ticketNumber));
            return t;
        }

        public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
        {
            return _ctx.TicketResponses.Where(r => r.Ticket.TicketNumber == ticketNumber);
        }

        public IEnumerable<Ticket> ReadTickets()
        {
            return _ctx.Tickets.Include(t => t.Responses);
        }

        public void UpdateTicket(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException();
            if (ReadTicket(ticket.TicketNumber) == null) throw new KeyNotFoundException();

            _ctx.Entry(ticket).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

        public void UpdateTicketStateToClosed(int ticketNumber)
        {
            var ticket = _ctx.Tickets.Find(ticketNumber);
            if (Equals(null, ticket))
            {
                throw new KeyNotFoundException("Ticket not found");
            }
            else
            {
                ticket.State = TicketState.Closed;
            }
            _ctx.SaveChanges();
        }

        public void ClearDatabase()
        {
            _ctx.TicketResponses.RemoveRange(_ctx.TicketResponses);
            _ctx.Tickets.RemoveRange(_ctx.Tickets);
            _ctx.HardwareTickets.RemoveRange(_ctx.HardwareTickets);
            _ctx.SaveChanges();
        }
    }
}
