using allstars.Models;

namespace allstars.Extensions
{
    public static class TicketExtensions
    {
        public static bool IsEmpty(this Ticket ticket)
        {
            return ticket.Id == 0 && ticket.AuthorId == 0 && ticket.Guild == 0 && string.IsNullOrEmpty(ticket.Description);
        }

        public static bool IsObjectNull(this Ticket ticket)
        {
            return ticket == null;
        }
    }
}