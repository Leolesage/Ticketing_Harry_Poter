using System;
using System.Collections.Generic;

namespace Ticketing_Harry_Poter.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public int IncidentLevel { get; set; }    // <— nouveau
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
