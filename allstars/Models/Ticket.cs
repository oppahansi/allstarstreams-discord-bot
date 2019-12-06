using System;
using System.ComponentModel.DataAnnotations;

namespace allstars.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public ulong Guild { get; set; }
        public ulong ClosedBy { get; set; }
        public ulong CanceledBy { get; set; }
        public ulong AnsweredBy { get; set; }
        public ulong EscalatedBy { get; set; }
        public ulong AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public string UpdateMessage { get; set; }
        public string CancelReason { get; set; }
        public string AnswerMessage { get; set; }
        public string CloseMessage { get; set; }
        public string AuthorAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}