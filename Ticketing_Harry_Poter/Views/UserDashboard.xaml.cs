using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Views
{
    public partial class UserDashboard : Window
    {
        private readonly User _currentUser;

        public UserDashboard(User user)
        {
            InitializeComponent();
            _currentUser = user;
            UserNameText.Text = user.Username;
            LoadMyTickets();
        }

        private void LoadMyTickets()
        {
            using var db = new AppDbContext();
            var tickets = db.Tickets
                            .Where(t => t.UserId == _currentUser.Id)
                            .OrderByDescending(t => t.CreatedAt)
                            .ToList();
            MyTicketsGrid.ItemsSource = tickets;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadMyTickets();

        private void MyTicketsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(MyTicketsGrid.SelectedItem is Ticket t)) return;

            // Charge les commentaires de l'utilisateur
            using var db = new AppDbContext();
            var comments = db.Comments
                             .Where(c => c.TicketId == t.Id)
                             .OrderBy(c => c.CreatedAt)
                             .ToList();
            CommentsListUser.ItemsSource = comments;
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (!(MyTicketsGrid.SelectedItem is Ticket t)) return;
            var content = NewCommentBoxUser.Text.Trim();
            if (string.IsNullOrWhiteSpace(content)) return;

            using var db = new AppDbContext();
            var comment = new Comment
            {
                TicketId = t.Id,
                Author = _currentUser.Username,
                Content = content,
                CreatedAt = System.DateTime.Now
            };
            db.Comments.Add(comment);
            db.SaveChanges();

            // Recharger la liste
            var comments = db.Comments
                             .Where(c => c.TicketId == t.Id)
                             .OrderBy(c => c.CreatedAt)
                             .ToList();
            CommentsListUser.ItemsSource = comments;
            NewCommentBoxUser.Clear();
        }

        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewTicketWindow(_currentUser) { Owner = this };
            if (win.ShowDialog() == true)
                LoadMyTickets();
        }
    }
}
