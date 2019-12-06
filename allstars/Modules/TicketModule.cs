using allstars.Extensions;
using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace allstars.Modules
{
    [RequireContext(ContextType.Guild)]
    public class TicketModule : ModuleBase<SocketCommandContext>
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IRepositoryWrapper RepositoryWrapper;
        private readonly IConfigurationRoot Config;

        public TicketModule(IRepositoryWrapper repositoryWrapper, IConfigurationRoot config)
        {
            RepositoryWrapper = repositoryWrapper;
            Config = config;
        }

        [Command("submit")]
        [Summary("Submit a ticket in discord.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task SubmitTicketAsync([Remainder] string problem = null)
        {
            if (string.IsNullOrEmpty(problem))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Submitting ticket failed:\n\n**Problem description not provided.**",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                var message = Context.Message;
                var author = message.Author as IGuildUser;
                var guild = Context.Guild;
                var dmChannel = await message.Author.GetOrCreateDMChannelAsync();

                await Context.Message.DeleteAsync().ConfigureAwait(false);

                var ticket = await RepositoryWrapper.TicketRepository.GetOpenTicketByAuthorIdAsync(author.Id);
                if (ticket != null && !ticket.IsEmpty())
                {
                    try
                    {
                        var embOpen = new EmbedBuilder()
                        {
                            Color = Constants.FailureColor,
                            Description = $"You, **{author.Username}**, already have an open ticket.\n\nYou can check your ticket status using the **{Config[Constants.ConfigCommandPrefix]}status** command.\nIf you want to update your ticket, use **{Config[Constants.ConfigCommandPrefix]}update** command.\nIf you want to cancel your ticket, use **{Config[Constants.ConfigCommandPrefix]}cancel** command.\n\n{Config[Constants.ConfigCommandPrefix]}status\n{Config[Constants.ConfigCommandPrefix]}cancel\n{Config[Constants.ConfigCommandPrefix]}update Update message goes here.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await dmChannel.SendMessageAsync("", false, embOpen.Build()).ConfigureAwait(false);
                    }
                    catch
                    {
                        var embDmNull = new EmbedBuilder()
                        {
                            Color = Constants.FailureColor,
                            Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                    }

                    return;
                }

                ticket = new Ticket()
                {
                    Description = problem,
                    Guild = guild.Id,
                    ClosedBy = 0,
                    CanceledBy = 0,
                    AnsweredBy = 0,
                    EscalatedBy = 0,
                    AuthorId = author.Id,
                    AuthorName = author.Username,
                    AuthorAvatarUrl = author.GetAvatarUrl(),
                    CreatedAt = DateTime.UtcNow,
                    UpdateMessage = "",
                    CancelReason = "",
                    AnswerMessage = ""
                };

                await RepositoryWrapper.TicketRepository.AddTicketAsync(ticket);
                RepositoryWrapper.TicketRepository.SaveChanges();

                var embTeam = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("New Ticket").WithIconUrl(ticket.AuthorAvatarUrl).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.SuccessColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                }
                    .AddField(new EmbedFieldBuilder().WithName("Ticket ID").WithIsInline(true).WithValue(ticket.Id.ToString()))
                    .AddField(new EmbedFieldBuilder().WithName("Created by").WithIsInline(true).WithValue(ticket.AuthorName))
                    .AddField(new EmbedFieldBuilder().WithName("Created On").WithIsInline(true).WithValue(ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));

                if (ticket.Description.Length > 1023)
                    embTeam.Description = ticket.Description;
                else
                    embTeam.AddField(new EmbedFieldBuilder().WithName("Issue").WithIsInline(false).WithValue(ticket.Description));

                try
                {
                    var embDm = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName($"{ticket.AuthorName}").WithIconUrl(ticket.AuthorAvatarUrl),
                        Description = $"Ticket successfully submitted. Ticket id : **{ticket.Id}**.\n\nYou can check your ticket status using the **{Config[Constants.ConfigCommandPrefix]}status** command.\nYou can update your ticket using the **{Config[Constants.ConfigCommandPrefix]}update** command.\nYou can cancel your ticket using the **{Config[Constants.ConfigCommandPrefix]}cancel** command.\n\n{Config[Constants.ConfigCommandPrefix]}status\n{Config[Constants.ConfigCommandPrefix]}cancel\n{Config[Constants.ConfigCommandPrefix]}update Update message goes here.",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embDm.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }

                var ticketSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("tickets");
                if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                {
                    Log.Error($"Tickets channel has not been set. Trying to find log channel.");

                    var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
                    if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                        Log.Error($"Log channel has not been set. Saving ticket to db.");
                    else
                    {
                        if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                        {
                            if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                                await guildLogChannel.SendMessageAsync("", false, embTeam.Build()).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    if (guild.GetChannel(ticketSpecialChannel.Id) is ITextChannel guildTicketChannel)
                        await guildTicketChannel.SendMessageAsync("", false, embTeam.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("list")]
        [Alias("l")]
        [Summary("Sows current tickets.")]
        [PermissionCheck]
        public async Task ShowTicketsAsync()
        {
            var tickets = await RepositoryWrapper.TicketRepository.GetAllOpenTicketsAsync();

            if (tickets.Count() == 0)
            {
                var embEmpty = new EmbedBuilder()
                {
                    Description = $"Ticket list is empty.",
                    Color = Constants.InfoColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embEmpty.Build()).ConfigureAwait(false);
            }
            else
            {
                var ticketListReply = new StringBuilder();
                var embs = new List<Embed>();
                var embList = new EmbedBuilder();

                foreach (var ticket in tickets)
                {
                    var answeredBy = Context.Guild.GetUser(ticket.AnsweredBy);
                    var escalatedBy = Context.Guild.GetUser(ticket.EscalatedBy);

                    var newTicketLine = $"**{ticket?.Id}** -- {ticket?.AuthorName} -- Answered: {(answeredBy != null ? answeredBy.Username : "")} -- Escalated: {(escalatedBy != null ? escalatedBy.Username : "")} -- Created on: {ticket?.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}\n";

                    if (ticketListReply.Length + newTicketLine.Length <= 2000)
                        ticketListReply.Append(value: newTicketLine);
                    else
                    {
                        embList = new EmbedBuilder()
                        {
                            Description = ticketListReply.ToString(),
                            Color = Constants.SuccessColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embs.Add(embList.Build());
                        ticketListReply.Clear();
                    }
                }

                embList = new EmbedBuilder()
                {
                    Description = ticketListReply.ToString(),
                    Color = Constants.SuccessColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embs.Add(embList.Build());

                foreach (var emb in embs)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
        }

        [Command("open")]
        [Alias("o")]
        [Summary("Open ticket with given id.")]
        [PermissionCheck]
        public async Task OpenTicketsAsync(int ticketId = 0)
        {
            if (ticketId <= 0)
            {
                var embZero = new EmbedBuilder()
                {
                    Description = "Ticked Id invalid.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embZero.Build()).ConfigureAwait(false);
            }
            else
            {
                var ticket = await RepositoryWrapper.TicketRepository.GetTicketByIdAsync(ticketId);
                if (ticket.IsObjectNull() || ticket.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"Ticked with id **{ticketId}** could not be found.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                else
                {
                    var answeredBy = Context.Guild.GetUser(ticket.AnsweredBy);
                    var escalatedBy = Context.Guild.GetUser(ticket.EscalatedBy);

                    var embTicket = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName($"Summary for Ticket : {ticket.Id}" + (ticket.ClosedBy != 0 ? "  - CLOSED" : "")).WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    }
                        .AddField(new EmbedFieldBuilder().WithName("Created by").WithIsInline(true).WithValue(ticket.AuthorName))
                        .AddField(new EmbedFieldBuilder().WithName("Created On").WithIsInline(true).WithValue(ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)))
                        .AddField(new EmbedFieldBuilder().WithName("Answered by").WithIsInline(true).WithValue(answeredBy != null ? answeredBy.Username : "---"))
                        .AddField(new EmbedFieldBuilder().WithName("Escalated by").WithIsInline(true).WithValue(escalatedBy != null ? escalatedBy.Username : "---"))
                        .AddField(new EmbedFieldBuilder().WithName("Canceled?").WithIsInline(true).WithValue(ticket.CanceledBy != 0 ? "User canceled his ticket or left the discord server." : "---"));

                    if (ticket.Description.Length > 1023)
                        embTicket.Description = ticket.Description;
                    else
                        embTicket.AddField(new EmbedFieldBuilder().WithName("Issue").WithIsInline(false).WithValue(ticket.Description));

                    embTicket.AddField(new EmbedFieldBuilder().WithName("User update message").WithIsInline(false).WithValue(string.IsNullOrEmpty(ticket.UpdateMessage) ? "---" : ticket.UpdateMessage));
                    embTicket.AddField(new EmbedFieldBuilder().WithName("Answer message").WithIsInline(false).WithValue(string.IsNullOrEmpty(ticket.AnswerMessage) ? "---" : ticket.AnswerMessage));
                    embTicket.AddField(new EmbedFieldBuilder().WithName("Close message").WithIsInline(false).WithValue(string.IsNullOrEmpty(ticket.CloseMessage) ? "---" : ticket.CloseMessage));

                    await ReplyAsync("", false, embTicket.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("answer")]
        [Alias("a")]
        [Summary("Answer ticket with given id.")]
        [PermissionCheck]
        public async Task AnswerTicketsAsync(int ticketId = 0, [Remainder] string answerMsg = null)
        {
            if (ticketId <= 0)
            {
                var embZero = new EmbedBuilder()
                {
                    Description = "Ticked Id invalid.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embZero.Build()).ConfigureAwait(false);
            }
            else
            {
                var ticket = await RepositoryWrapper.TicketRepository.GetTicketByIdAsync(ticketId);
                if (ticket.IsObjectNull() || ticket.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"Ticked with id **{ticketId}** could not be found.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                else
                {
                    var ticketAuthor = Context.Guild.GetUser(ticket.AuthorId);
                    if (ticketAuthor == null)
                    {
                        var embAuthorNull = new EmbedBuilder()
                        {
                            Color = Constants.FailureColor,
                            Description = $"User {ticketAuthor.Username} could not be found on this discord server. Ticket is now closed.",
                            Footer = new EmbedFooterBuilder()
                        };

                        ticket.AnsweredBy = Context.Message.Author.Id;
                        ticket.ClosedBy = ticket.AuthorId;
                        ticket.CanceledBy = ticket.AuthorId;
                        ticket.UpdateMessage = "Closed due to author leaving the discord server.";

                        RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                        RepositoryWrapper.TicketRepository.SaveChanges();

                        await ReplyAsync("", false, embAuthorNull.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        ticket.AnsweredBy = Context.Message.Author.Id;
                        ticket.AnswerMessage = answerMsg != null ? answerMsg : "";

                        RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                        RepositoryWrapper.TicketRepository.SaveChanges();

                        var dmChannel = await ticketAuthor.GetOrCreateDMChannelAsync();

                        var embTeam = new EmbedBuilder()
                        {
                            Color = Constants.SuccessColor,
                            Description = $"User **{ticket.AuthorName}** (ticket id: **{ticket.Id}**) has been notified.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        try
                        {
                            var embContact = new EmbedBuilder()
                            {
                                Author = new EmbedAuthorBuilder().WithName(ticket.AuthorName).WithIconUrl(ticket.AuthorAvatarUrl).WithUrl(Constants.AllStarsUrl),
                                Color = Constants.SuccessColor,
                                Description = $"**{Context.Message.Author.Username}** is now processing your ticket.\nYou will receive a message shortly..",
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };

                            if (!string.IsNullOrEmpty(answerMsg))
                                embContact.AddField(new EmbedFieldBuilder().WithName("Answer notice:").WithIsInline(false).WithValue(ticket.AnswerMessage));

                            await dmChannel.SendMessageAsync("", false, embContact.Build()).ConfigureAwait(false);
                            await ReplyAsync("", false, embTeam.Build()).ConfigureAwait(false);
                        }
                        catch
                        {
                            var embDmNull = new EmbedBuilder()
                            {
                                Color = Constants.InfoColor,
                                Description = $"Notifying user **{ticket.AuthorName}** (ticket id: **{ticket.AuthorId}**) failed.\nUser does not allow direct messages anymore.\nYou will have to contact the user manually.",
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };

                            await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        [Command("close")]
        [Alias("c")]
        [Summary("Close ticket with given id.")]
        [PermissionCheck]
        public async Task CloseTicketsAsync(int ticketId = 0, [Remainder] string closeMsg = null)
        {
            if (ticketId <= 0)
            {
                var embZero = new EmbedBuilder()
                {
                    Description = "Ticked Id invalid.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embZero.Build()).ConfigureAwait(false);
            }
            else
            {
                var ticket = await RepositoryWrapper.TicketRepository.GetTicketByIdAsync(ticketId);
                if (ticket.IsObjectNull() || ticket.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"Ticked with id **{ticketId}** could not be found.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                else
                {
                    var author = Context.Guild.GetUser(ticket.AuthorId);
                    var mod = Context.Guild.GetUser(Context.Message.Author.Id);

                    if (author == null)
                    {
                        var embAuthorNull = new EmbedBuilder()
                        {
                            Color = Constants.FailureColor,
                            Description = $"User {author.Username} could not be found on this discord server. Ticket is now closed.",
                            Footer = new EmbedFooterBuilder()
                        };

                        ticket.ClosedBy = mod.Id;
                        ticket.CloseMessage = string.IsNullOrEmpty(closeMsg) ? "---" : closeMsg;

                        RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                        RepositoryWrapper.TicketRepository.SaveChanges();

                        await ReplyAsync("", false, embAuthorNull.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        ticket.ClosedBy = mod.Id;
                        ticket.CloseMessage = string.IsNullOrEmpty(closeMsg) ? "---" : closeMsg;

                        RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                        RepositoryWrapper.TicketRepository.SaveChanges();

                        var dmChannel = await author.GetOrCreateDMChannelAsync();

                        try
                        {
                            var embNotify = new EmbedBuilder()
                            {
                                Author = new EmbedAuthorBuilder().WithName(ticket.AuthorName).WithIconUrl(ticket.AuthorAvatarUrl).WithUrl(Constants.AllStarsUrl),
                                Color = Constants.SuccessColor,
                                Description = $"Your ticket with id **{ticket.Id}** has been closed by **{mod.Username}**.",
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };

                            if (!string.IsNullOrEmpty(closeMsg))
                                embNotify.AddField(new EmbedFieldBuilder().WithName("Closing notice:").WithIsInline(true).WithValue(closeMsg));

                            await dmChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
                        }
                        catch
                        {
                            var embDmNull = new EmbedBuilder()
                            {
                                Color = Constants.InfoColor,
                                Description = $"Notifying user **{ticket.AuthorName}** (ticket id: **{ticket.AuthorId}**) failed.\nUser does not allow direct messages anymore.\nYou will have to contact the user manually.",
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };

                            await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                        }

                        var embTeam = new EmbedBuilder()
                        {
                            Color = Constants.SuccessColor,
                            Description = $"Ticket with id **{ticket.Id}** from user **{ticket.AuthorName}** has been closed by **{mod.Username}**.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embTeam.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("status")]
        [Summary("Show current ticket status.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task TicketStatusAsync()
        {
            var author = Context.Message.Author;
            var ticket = await RepositoryWrapper.TicketRepository.GetOpenTicketByAuthorIdAsync(author.Id);

            await Context.Message.DeleteAsync().ConfigureAwait(false);

            var dmChannel = await author.GetOrCreateDMChannelAsync();

            if (ticket.IsObjectNull() || ticket.IsEmpty())
            {
                try
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"You don't have an open ticket.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                try
                {
                    var answeredBy = Context.Guild.GetUser(ticket.AnsweredBy);
                    var escalatedBy = Context.Guild.GetUser(ticket.EscalatedBy);

                    var embTicket = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName($"Ticket status:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    }
                        .AddField(new EmbedFieldBuilder().WithName("Created On").WithIsInline(true).WithValue(ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)))
                        .AddField(new EmbedFieldBuilder().WithName("Update message:").WithIsInline(false).WithValue(string.IsNullOrEmpty(ticket.UpdateMessage) ? "---" : ticket.UpdateMessage))
                        .AddField(new EmbedFieldBuilder().WithName("Answered by").WithIsInline(true).WithValue(answeredBy != null ? answeredBy.Username : "---"))
                        .AddField(new EmbedFieldBuilder().WithName("Answer message").WithIsInline(false).WithValue(string.IsNullOrEmpty(ticket.AnswerMessage) ? "---" : ticket.AnswerMessage));

                    if (ticket.Description.Length > 1023)
                        embTicket.Description = ticket.Description;
                    else
                        embTicket.AddField(new EmbedFieldBuilder().WithName("Issue").WithIsInline(false).WithValue(ticket.Description));

                    await dmChannel.SendMessageAsync("", false, embTicket.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("update")]
        [Summary("Show current ticket status.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task UpdateTicketAsync([Remainder] string updateMsg = null)
        {
            var guild = Context.Guild;
            var author = Context.Message.Author;
            var dmChannel = await author.GetOrCreateDMChannelAsync();
            var ticket = await RepositoryWrapper.TicketRepository.GetOpenTicketByAuthorIdAsync(author.Id);

            await Context.Message.DeleteAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(updateMsg) || updateMsg.Length > 1023)
            {
                try
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = "Update message is empty **OR** your message is too long (1000 characters max).",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embNull.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }
            }
            else if (ticket.IsObjectNull() || ticket.IsEmpty())
            {
                try
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"You don't have an open ticket.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                try
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Description = $"Your ticket has been updated.",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);

                    return;
                }

                ticket.UpdateMessage = updateMsg;

                RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                RepositoryWrapper.TicketRepository.SaveChanges();

                var embTeam = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(ticket.AuthorName).WithIconUrl(ticket.AuthorAvatarUrl).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.SuccessColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (ticket.Description.Length > 1023)
                    embTeam.Description = ticket.Description;
                else
                    embTeam.AddField(new EmbedFieldBuilder().WithName("Issue").WithIsInline(false).WithValue(ticket.Description));

                embTeam.AddField(new EmbedFieldBuilder().WithName("Update message:").WithIsInline(true).WithValue(ticket.UpdateMessage));

                var ticketSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("tickets");
                if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                {
                    Log.Error($"Tickets channel has not been set. Trying to find log channel.");

                    var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
                    if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                        Log.Error($"Log channel has not been set. Saving ticket to db.");
                    else
                    {
                        if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                        {
                            if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                                await guildLogChannel.SendMessageAsync($"Ticket with id **{ticket.Id}** has been updated:", false, embTeam.Build()).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    if (guild.GetChannel(ticketSpecialChannel.Id) is ITextChannel guildTicketChannel)
                        await guildTicketChannel.SendMessageAsync($"Ticket with id **{ticket.Id}** has been updated:", false, embTeam.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("cancel")]
        [Summary("Show current ticket status.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task CancelTicketAsync([Remainder] string cancelReason = null)
        {
            var guild = Context.Guild;
            var author = Context.Message.Author;
            var dmChannel = await author.GetOrCreateDMChannelAsync();
            var ticket = await RepositoryWrapper.TicketRepository.GetOpenTicketByAuthorIdAsync(author.Id);

            await Context.Message.DeleteAsync().ConfigureAwait(false);

            if (ticket.IsObjectNull() || ticket.IsEmpty())
            {
                try
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"You don't have an open ticket.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                try
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Description = $"Your ticket has been canceled.",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await dmChannel.SendMessageAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
                catch
                {
                    var embDmNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"The allstars bot cannot send direct messages to you, **{author.Username}**.\nAllow direct messages and try again.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDmNull.Build()).ConfigureAwait(false);
                    return;
                }

                ticket.CanceledBy = author.Id;
                ticket.ClosedBy = author.Id;
                ticket.CancelReason = cancelReason;
                ticket.UpdateMessage = "User canceled his own ticket.";

                RepositoryWrapper.TicketRepository.UpdateTicket(ticket);
                RepositoryWrapper.TicketRepository.SaveChanges();

                var embTeam = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName($"{ticket.AuthorName}").WithIconUrl(ticket.AuthorAvatarUrl).WithUrl(Constants.AllStarsUrl),
                    Description = $"Has cancelled his ticket with id : **{ticket.Id}**. Ticket is now closed.",
                    Color = Constants.SuccessColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                var ticketSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("tickets");
                if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                {
                    Log.Error($"Tickets channel has not been set. Trying to find log channel.");

                    var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
                    if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                        Log.Error($"Log channel has not been set. Saving ticket to db.");
                    else
                    {
                        if (ticketSpecialChannel.IsObjectNull() || ticketSpecialChannel.IsEmpty())
                        {
                            if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                                await guildLogChannel.SendMessageAsync("", false, embTeam.Build()).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    if (guild.GetChannel(ticketSpecialChannel.Id) is ITextChannel guildTicketChannel)
                        await guildTicketChannel.SendMessageAsync("", false, embTeam.Build()).ConfigureAwait(false);
                }
            }
        }
    }
}