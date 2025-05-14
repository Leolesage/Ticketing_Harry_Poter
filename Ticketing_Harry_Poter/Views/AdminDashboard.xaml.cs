using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Views
{
    public partial class AdminDashboard : Window
    {
        private readonly int _adminLevel;
        private IQueryable<Ticket> _tickets;

        public AdminDashboard(int adminLevel)
        {
            InitializeComponent();
            _adminLevel = adminLevel;

            // Init filtres et dropdowns
            StatusFilter.ItemsSource = new[] { "Tous", "Ouvert", "En attente", "Clos" };
            StatusFilter.SelectedIndex = 0;
            DetailCategory.ItemsSource = new[] { "Incident", "Réseau", "Prêt de matériel", "Dossier partagé" };
            DetailPriority.ItemsSource = new[] { "Basse", "Normale", "Élevée" };
            DetailStatus.ItemsSource = new[] { "Ouvert", "En attente", "Clos" };

            LoadTickets();
        }

        private void LoadTickets()
        {
            using var db = new AppDbContext();
            var all = db.Tickets.Include(t => t.User).ToList();
            _tickets = all
                .Where(t => t.IncidentLevel <= _adminLevel)
                .AsQueryable();
            RefreshView();
        }

        private void RefreshView()
        {
            if (_tickets == null) return;
            var view = _tickets;

            // Filtre statut
            var statut = StatusFilter.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(statut) && statut != "Tous")
            {
                if (statut == "En attente") statut = "EnAttente";
                view = view.Where(t => t.Status == statut);
            }

            // Filtre recherche
            var q = SearchBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(q))
                view = view.Where(t =>
                    t.Title.ToLower().Contains(q) ||
                    t.Description.ToLower().Contains(q));

            TicketsGrid.ItemsSource = view.ToList();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadTickets();
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshView();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => RefreshView();

        private void TicketsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;

            DetailTitle.Text = t.Title;
            DetailDesc.Text = t.Description;
            DetailCategory.SelectedItem = t.Category;
            DetailPriority.SelectedItem = t.Priority;
            DetailStatus.SelectedItem = t.Status;
            DetailLevel.Text = t.IncidentLevel.ToString();

            // Charge les commentaires
            using var db = new AppDbContext();
            var comments = db.Comments
                             .Where(c => c.TicketId == t.Id)
                             .OrderBy(c => c.CreatedAt)
                             .ToList();
            CommentsList.ItemsSource = comments;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id);
            if (ticket == null) return;

            ticket.Title = DetailTitle.Text;
            ticket.Description = DetailDesc.Text;
            ticket.Category = DetailCategory.SelectedItem?.ToString() ?? ticket.Category;
            ticket.Priority = DetailPriority.SelectedItem?.ToString() ?? ticket.Priority;
            ticket.Status = DetailStatus.SelectedItem?.ToString() ?? ticket.Status;
            ticket.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            LoadTickets();
        }

        private void Escalate_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            if (t.IncidentLevel >= 3)
            {
                MessageBox.Show("Ce ticket est déjà au niveau maximal.",
                                "Escalade impossible",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Voulez-vous vraiment passer le ticket '{t.Title}' du niveau {t.IncidentLevel} au niveau {t.IncidentLevel + 1} ?",
                "Confirmation d'escalade",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id);
            if (ticket == null) return;

            ticket.IncidentLevel++;
            ticket.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            LoadTickets();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            var result = MessageBox.Show(
                $"Voulez-vous vraiment supprimer le ticket « {t.Title} » ?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id);
            if (ticket != null)
            {
                db.Tickets.Remove(ticket);
                db.SaveChanges();
            }

            LoadTickets();
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            var content = NewCommentBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(content)) return;

            using var db = new AppDbContext();
            var comment = new Comment
            {
                TicketId = t.Id,
                Author = $"Admin{_adminLevel}",
                Content = content,
                CreatedAt = DateTime.Now
            };
            db.Comments.Add(comment);
            db.SaveChanges();

            // Recharge
            var comments = db.Comments
                             .Where(c => c.TicketId == t.Id)
                             .OrderBy(c => c.CreatedAt)
                             .ToList();
            CommentsList.ItemsSource = comments;

            NewCommentBox.Clear();
        }

        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            // Stub admin
        }
    }
}
