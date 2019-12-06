using allstars.Contexts;
using allstars.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class TicketRepository : RepositoryBase<Ticket>, ITicketRepository
    {
        public TicketRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<Ticket> GetTicketByIdAsync(int id)
        {
            var tickets = await FindByConditionAsync(x => x.Id == id);
            return tickets.DefaultIfEmpty(new Ticket()).FirstOrDefault();
        }

        public async Task<Ticket> GetTicketByAuthorIdAsync(ulong authorId)
        {
            var tickets = await FindByConditionAsync(x => x.AuthorId == authorId);
            return tickets.DefaultIfEmpty(new Ticket()).FirstOrDefault();
        }

        public async Task<Ticket> GetOpenTicketByAuthorIdAsync(ulong authorId)
        {
            var tickets = await FindByConditionAsync(x => x.AuthorId == authorId && x.ClosedBy == 0);
            return tickets.DefaultIfEmpty(new Ticket()).FirstOrDefault();
        }

        public async Task<IEnumerable<Ticket>> GetAllOpenTicketsAsync()
        {
            var tickets = await FindByConditionAsync(x => x.ClosedBy == 0);
            return tickets;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            await CreateAsync(ticket);
        }

        public async Task SaveChangesAsync()
        {
            await SaveAsync();
        }

        public void SaveChanges()
        {
            Save();
        }

        public void UpdateTicket(Ticket ticket)
        {
            Update(ticket);
        }
    }
}