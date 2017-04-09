using System;
using SC.BL.Domain;
using SC.BL;
using SC.UI.CA.ExtensionMethods;
using System.Collections.Generic;

//This class just excists to illustrate the way of how we used to test a method
namespace SC.UI.CA
{
    class ProgramForTesting2
    {
        private static readonly ITicketManager mgr = new TicketManager();

        static void Main(string[] args)
        {
            PrintAllTickets();

            mgr.RemoveTicket(1);
            Ticket t = mgr.GetTicket(1);
            if (t != null)
                Console.WriteLine("Ticket {0} has not been removed!");

            Console.WriteLine("Ticket has been removed!\n");

            PrintAllTickets();
            Console.ReadLine();
        }

        private static void PrintAllTickets()
        {
            foreach (var t in mgr.GetTickets())
                Console.WriteLine(t.GetInfo());
        }
    }
}
