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

            // Remplissage des filtres
            StatusFilter.ItemsSource = new[] { "Tous", "Ouvert", "En attente", "Clos" };
            StatusFilter.SelectedIndex = 0;

            // Remplissage des listes de détails
            DetailCategory.ItemsSource = new[] { "Incident", "Réseau", "Prêt de matériel", "Dossier partagé" };
            DetailPriority.ItemsSource = new[] { "Basse", "Normale", "Élevée" };
            DetailStatus.ItemsSource = new[] { "Ouvert", "En attente", "Clos" };

            LoadTickets();
        }

        private void LoadTickets()
        {
            using var db = new AppDbContext();
            var all = db.Tickets.Include(t => t.User).ToList();
            _tickets = all.Where(t => t.IncidentLevel <= _adminLevel).AsQueryable();
            RefreshView();
        }

        private void RefreshView()
        {
            if (_tickets == null) return;
            var view = _tickets;

            // Filtre sur le statut
            var statut = StatusFilter.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(statut) && statut != "Tous")
            {
                // On compare directement à "En attente", "Ouvert" ou "Clos"
                view = view.Where(t => t.Status == statut);
            }

            // Filtre sur la recherche texte
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

            using var db = new AppDbContext();
            CommentsList.ItemsSource = db.Comments
                                          .Where(c => c.TicketId == t.Id)
                                          .OrderBy(c => c.CreatedAt)
                                          .ToList();
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

            MessageBox.Show("Le ticket a été sauvegardé.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTickets();
        }

        private void Escalate_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            if (t.IncidentLevel >= 3)
            {
                MessageBox.Show("Niveau maximal atteint.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show(
                $"Escalader «{t.Title}» de niveau {t.IncidentLevel} à {t.IncidentLevel + 1} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id);
            ticket.IncidentLevel++;
            ticket.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            MessageBox.Show("Escalade réussie.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTickets();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            var res = MessageBox.Show($"Supprimer «{t.Title}» ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id);
            if (ticket != null)
            {
                db.Tickets.Remove(ticket);
                db.SaveChanges();
            }

            MessageBox.Show("Ticket supprimé.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTickets();
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            var content = NewCommentBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(content)) return;

            using var db = new AppDbContext();
            db.Comments.Add(new Comment
            {
                TicketId = t.Id,
                Author = $"Admin{_adminLevel}",
                Content = content,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            NewCommentBox.Clear();
            LoadTickets();
        }


        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewTicketWindow(null) { Owner = this };
            if (win.ShowDialog() == true)
                LoadTickets();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
