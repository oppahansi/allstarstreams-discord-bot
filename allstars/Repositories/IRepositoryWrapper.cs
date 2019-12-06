namespace allstars.Repositories
{
    public interface IRepositoryWrapper
    {
        ITicketRepository TicketRepository { get; }
        ISpecialChannelRepository SpecialChannelRepository { get; }
        IQuickHelpRepository QuickHelpRepository { get; }
        IUserRepository UserRepository { get; }
        IMuteRepository MuteRepository { get; }
        ICmdChannelRepository CmdChannelRepository { get; }
        ICmdRoleRepository CmdRoleRepository { get; }
        ICmdUserCdRepository CmdUserCdRepository { get; }
        IUpcomingReleaseRepository UpcomingReleaseRepository { get; }
        IAutoMessageRepository AutoMessageRepository { get; }
    }
}