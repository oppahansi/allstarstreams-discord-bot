using allstars.Contexts;
using allstars.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Repositories.Impl
{
    public class AutoMessageRepository : RepositoryBase<AutoMessage>, IAutoMessageRepository
    {
        public AutoMessageRepository(BotDbContext dbContext)
            : base(dbContext)
        {
        }

        public void SaveChanges()
        {
            Save();
        }

        public async Task AddAutoMessageAsync(AutoMessage autoMessage)
        {
            await CreateAsync(autoMessage);
        }

        public async Task<AutoMessage> GetAutoMessageAsync(ulong channelId)
        {
            var autoMessages = await FindByConditionAsync(x => x.Channel == channelId);
            return autoMessages.DefaultIfEmpty(new AutoMessage()).FirstOrDefault();
        }

        public async Task<AutoMessage> GetAutoMessageAsync(string message)
        {
            var autoMessages = await FindByConditionAsync(x => x.Message.CompareTo(message) == 0);
            return autoMessages.DefaultIfEmpty(new AutoMessage()).FirstOrDefault();
        }

        public async Task<AutoMessage> GetAutoMessageAsync(DateTime expiration)
        {
            var autoMessages = await FindByConditionAsync(x => x.Expiration == expiration);
            return autoMessages.DefaultIfEmpty(new AutoMessage()).FirstOrDefault();
        }

        public async Task<IEnumerable<AutoMessage>> GetAllAutoMessagesAsync()
        {
            return await FindAllAsync();
        }

        public void UpdateAutoMessage(AutoMessage autoMessage)
        {
            Update(autoMessage);
        }

        public void DeleteAutoMessage(AutoMessage autoMessage)
        {
            Delete(autoMessage);
        }

        public void DeleteManyAutoMessages(IEnumerable<AutoMessage> autoMessages)
        {
            DeleteMany(autoMessages);
        }
    }
}