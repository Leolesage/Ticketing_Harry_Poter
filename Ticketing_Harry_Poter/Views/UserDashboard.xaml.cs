using System;
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


        // Charge les tickets de l'utilisateur
        private void LoadMyTickets()
        {
            using var db = new AppDbContext();
            MyTicketsGrid.ItemsSource = db.Tickets
                                          .Where(t => t.UserId == _currentUser.Id)
                                          .OrderByDescending(t => t.CreatedAt)
                                          .ToList();
            // Vide la zone commentaires si on recharge la grille
            CommentsListUser.ItemsSource = null;
        }

            private void BtnHome_Click(object sender, RoutedEventArgs e)
            {
                // Ouvre la fenêtre de login
                var login = new LoginWindow();
                login.Show();

                // Ferme la fenêtre actuelle
                this.Close();
            }
 

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMyTickets();
        }

        // Quand on sélectionne un ticket, on charge ses commentaires
        private void MyTicketsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(MyTicketsGrid.SelectedItem is Ticket t))
            {
                CommentsListUser.ItemsSource = null;
                return;
            }

            using var db = new AppDbContext();
            var comments = db.Comments
                             .Where(c => c.TicketId == t.Id)
                             .OrderBy(c => c.CreatedAt)
                             .ToList();
            CommentsListUser.ItemsSource = comments;
        }

        // Ajout d'un nouveau commentaire sous le ticket sélectionné
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
                CreatedAt = DateTime.Now
            };
            db.Comments.Add(comment);
            db.SaveChanges();

            // On remet à jour la liste des commentaires directement
            NewCommentBoxUser.Clear();
            MyTicketsGrid_SelectionChanged(null, null);
        }

        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewTicketWindow(_currentUser) { Owner = this };
            if (win.ShowDialog() == true)
                LoadMyTickets();
        }

    }
}