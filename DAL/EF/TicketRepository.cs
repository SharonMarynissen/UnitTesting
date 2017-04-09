﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC.BL.Domain;
using System.Data.Entity;

namespace SC.DAL.EF
{
    public class TicketRepository : ITicketRepository
    {
        private SupportCenterDbContext ctx;

        public TicketRepository()
        {
            ctx = new SupportCenterDbContext();
            //ctx.Database.Initialize(false); //initializatie enkel uitvoeren indien nog niet eerder uitgevoerd.
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            ctx.Tickets.Add(ticket);
            ctx.SaveChanges();
            return ticket;
        }

        public TicketResponse CreateTicketResponse(TicketResponse response)
        {
            ctx.TicketResponses.Add(response);
            ctx.SaveChanges();
            return response;
        }

        public void DeleteTicket(int ticketNumber)
        {
            Ticket t = this.ReadTicket(ticketNumber);
            ctx.Tickets.Remove(t);
            ctx.SaveChanges();
        }

        public Ticket ReadTicket(int ticketNumber)
        {
            return ctx.Tickets.Find(ticketNumber);
        }

        public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
        {
            return ctx.TicketResponses.Where(r => r.Ticket.TicketNumber == ticketNumber);
        }

        public IEnumerable<Ticket> ReadTickets()
        {
            //return ctx.Tickets.ToList();  //Lazy loading : Responses virtual gemaakt in Ticket
            return ctx.Tickets.Include(t => t.Responses);

        }

        public void UpdateTicket(Ticket ticket)
        {
            Ticket t = this.ReadTicket(ticket.TicketNumber);
            t = ticket;
            //Zeker zijn dat ticket gekend is door context en dat die de staat 
            //Modified krijgt vooraleer de database aan te passen.
            ctx.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
            ctx.SaveChanges();
        }

        public void UpdateTicketStateToClosed(int ticketNumber)
        {
            Ticket t = this.ReadTicket(ticketNumber);
            t.State = TicketState.Closed;
            ctx.SaveChanges();
        }
    }
}