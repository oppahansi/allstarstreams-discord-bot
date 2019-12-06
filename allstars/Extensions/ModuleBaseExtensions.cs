using allstars.Models;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace allstars.Extensions
{
    public static class ModuleBaseExtensions
    {
        public static List<VadertvAccInfo> MapVaders(this ModuleBase<SocketCommandContext> baseModule, IConfigurationRoot config)
        {
            var currentVaderstvFile = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, config[Constants.ConfigVaderstvAccInfoFile]));
            var currentVadersList = new List<string>(currentVaderstvFile);
            var accListInfoList = new List<VadertvAccInfo>();
            var dateFormat = "MM-dd-yyyy";

            foreach (var acc in currentVadersList)
            {
                if (acc.StartsWith("ERROR"))
                    continue;

                var stringPieces = acc.Split(";");
                var expirationDate = new DateTime();
                DateTime.TryParseExact(stringPieces[2], dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out expirationDate);

                var status = "";

                if (stringPieces[3].CompareTo("close") == 0)
                    status = "disabled";

                if (string.IsNullOrEmpty(status) && stringPieces[3].CompareTo("check") == 0)
                {
                    if (expirationDate != (new DateTime()) && expirationDate.CompareTo(DateTime.UtcNow) <= 0)
                        status = "expired";
                    else
                        status = "enabled";
                }

                accListInfoList.Add(new VadertvAccInfo()
                {
                    UserName = stringPieces[0],
                    Email = stringPieces[1],
                    Expiration = expirationDate,
                    Status = status,
                });
            }

            return accListInfoList.Distinct().ToList();
        }

        public static List<LightStreamAccInfo> MapLightStreams(this ModuleBase<SocketCommandContext> baseModule, IConfigurationRoot config)
        {
            var currentLightstreamsFile = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, config[Constants.ConfigLightStreamsAccInfoFile]));
            var currentLightstreamsList = new List<string>(currentLightstreamsFile);
            var accListInfoList = new List<LightStreamAccInfo>();
            var dateFormat = "dd/MM/yyyy HH:mm";

            foreach (var acc in currentLightstreamsList)
            {
                var stringPieces = acc.Split(";");

                var expirationDate = new DateTime();
                DateTime.TryParseExact(stringPieces[4], dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out expirationDate);

                accListInfoList.Add(new LightStreamAccInfo()
                {
                    UserName = stringPieces[2],
                    Password = stringPieces[3],
                    Expiration = expirationDate,
                    Status = stringPieces[0],
                    Owner = stringPieces[1],
                    MaxConnections = stringPieces[5],
                    Notes = stringPieces[6],
                });
            }

            return accListInfoList.Distinct().ToList();
        }

        public static List<LightStreamAccInfo> MapBeastTv(this ModuleBase<SocketCommandContext> baseModule, IConfigurationRoot config)
        {
            var currentLightstreamsFile = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, config[Constants.ConfigBeastTvAccInfoFile]));
            var currentLightstreamsList = new List<string>(currentLightstreamsFile);
            var accListInfoList = new List<LightStreamAccInfo>();
            var dateFormat = "dd/MM/yyyy HH:mm";

            foreach (var acc in currentLightstreamsList)
            {
                var stringPieces = acc.Split(";");

                var expirationDate = new DateTime();
                DateTime.TryParseExact(stringPieces[4], dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out expirationDate);

                accListInfoList.Add(new LightStreamAccInfo()
                {
                    UserName = stringPieces[2],
                    Password = stringPieces[3],
                    Expiration = expirationDate,
                    Status = stringPieces[0],
                    Owner = stringPieces[1],
                    MaxConnections = stringPieces[5],
                    Notes = stringPieces[6],
                });
            }

            return accListInfoList.Distinct().ToList();
        }

        public static async Task SendMail(this ModuleBase<SocketCommandContext> baseModule, IConfigurationRoot config, ITextChannel guildMailChannel, List<EmailInfo> emailInfo, int noEmailAddressCount = 0)
        {
            SmtpClient client = new SmtpClient
            {
                Port = 587,
                Host = config[Constants.ConfigMailHost],
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(config[Constants.ConfigBillingEmailAddress], config[Constants.ConfigBillingEmailAddressPassword])
            };

            var message = $"Starting sendind reminder mails from this email address **{emailInfo[0].From}**.";

            if (noEmailAddressCount > 0)
                message += $"\n\n{noEmailAddressCount} accounts were without an email address.";

            var embSendingMails = new EmbedBuilder()
            {
                Color = Constants.InfoColor,
                Description = message,
                Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
            };

            await guildMailChannel.SendMessageAsync("", false, embSendingMails.Build()).ConfigureAwait(false);

            var mailSentList = new List<string>();

            foreach (var mail in emailInfo)
            {
                MailMessage mm = new MailMessage(mail.From, mail.To)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,

                    Subject = mail.Subject,
                    Body = mail.Body,
                    IsBodyHtml = true
                };

                if (!mailSentList.Contains(mail.To))
                {
                    mailSentList.Add(mail.To);
                    client.Send(mm);
                }
                else
                    continue;

                var embMailSent = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Mail has been sent to **{mail.Username}** via **{mail.To}**.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await guildMailChannel.SendMessageAsync("", false, embMailSent.Build()).ConfigureAwait(false);
                await Task.Delay(3000);
            }
        }
    }
}
