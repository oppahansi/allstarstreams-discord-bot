using allstars.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace allstars.Repositories
{
    public interface IAutoMessageRepository
    {
        Task AddAutoMessageAsync(AutoMessage autoMessage);

        Task<AutoMessage> GetAutoMessageAsync(ulong channelId);
        
        Task<AutoMessage> GetAutoMessageAsync(string message);

        Task<AutoMessage> GetAutoMessageAsync(DateTime expiration);

        Task<IEnumerable<AutoMessage>> GetAllAutoMessagesAsync();

        void UpdateAutoMessage(AutoMessage autoMessage);

        void DeleteAutoMessage(AutoMessage autoMessage);

        void DeleteManyAutoMessages(IEnumerable<AutoMessage> autoMessages);

        void SaveChanges();
    }
}