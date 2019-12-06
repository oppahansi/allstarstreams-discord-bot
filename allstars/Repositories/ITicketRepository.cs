using allstars.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket> GetTicketByIdAsync(int id);

        Task<Ticket> GetTicketByAuthorIdAsync(ulong authorId);

        Task<Ticket> GetOpenTicketByAuthorIdAsync(ulong authorId);

        Task<IEnumerable<Ticket>> GetAllOpenTicketsAsync();

        Task AddTicketAsync(Ticket ticket);

        Task SaveChangesAsync();

        void SaveChanges();

        void UpdateTicket(Ticket ticket);
    }
}