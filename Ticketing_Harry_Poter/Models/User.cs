using System.Collections.Generic;

namespace Ticketing_Harry_Poter.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public List<Ticket> Tickets { get; set; }
    }
}
