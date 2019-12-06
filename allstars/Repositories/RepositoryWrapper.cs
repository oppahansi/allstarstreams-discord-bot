using allstars.Contexts;
using allstars.Repositories.Impl;

namespace allstars.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly BotDbContext DbContext;

        private ITicketRepository ticketRepository;

        public ITicketRepository TicketRepository
        {
            get
            {
                if (ticketRepository == null)
                    ticketRepository = new TicketRepository(DbContext);

                return ticketRepository;
            }
        }

        private ISpecialChannelRepository specialChannelRepository;

        public ISpecialChannelRepository SpecialChannelRepository
        {
            get
            {
                if (specialChannelRepository == null)
                    specialChannelRepository = new SpecialChannelRepository(DbContext);

                return specialChannelRepository;
            }
        }

        private IQuickHelpRepository quickHelpRepository;

        public IQuickHelpRepository QuickHelpRepository
        {
            get
            {
                if (quickHelpRepository == null)
                    quickHelpRepository = new QuickHelpRepository(DbContext);

                return quickHelpRepository;
            }
        }

        private IUserRepository userRepository;

        public IUserRepository UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(DbContext);

                return userRepository;
            }
        }

        private IMuteRepository muteRepository;

        public IMuteRepository MuteRepository
        {
            get
            {
                if (muteRepository == null)
                    muteRepository = new MuteRepository(DbContext);

                return muteRepository;
            }
        }

        private ICmdChannelRepository cmdChannelRepository;

        public ICmdChannelRepository CmdChannelRepository
        {
            get
            {
                if (cmdChannelRepository == null)
                    cmdChannelRepository = new CmdChannelRepository(DbContext);

                return cmdChannelRepository;
            }
        }

        private ICmdRoleRepository cmdRoleRepository;

        public ICmdRoleRepository CmdRoleRepository
        {
            get
            {
                if (cmdRoleRepository == null)
                    cmdRoleRepository = new CmdRoleRepository(DbContext);

                return cmdRoleRepository;
            }
        }

        private ICmdUserCdRepository cmdUserCdRepository;

        public ICmdUserCdRepository CmdUserCdRepository
        {
            get
            {
                if (cmdUserCdRepository == null)
                    cmdUserCdRepository = new CmdUserCdRepository(DbContext);

                return cmdUserCdRepository;
            }
        }

        private IUpcomingReleaseRepository upcomingReleaseRepository;

        public IUpcomingReleaseRepository UpcomingReleaseRepository
        {
            get
            {
                if (upcomingReleaseRepository == null)
                    upcomingReleaseRepository = new UpcomingReleaseRepository(DbContext);

                return upcomingReleaseRepository;
            }
        }

        private IAutoMessageRepository autoMessageRepository;

        public IAutoMessageRepository AutoMessageRepository
        {
            get
            {
                if (autoMessageRepository == null)
                    autoMessageRepository = new AutoMessageRepository(DbContext);

                return autoMessageRepository;
            }
        }

        public RepositoryWrapper(BotDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}