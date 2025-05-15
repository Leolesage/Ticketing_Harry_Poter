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

            // Initialisation filtres
            StatusFilter.ItemsSource = new[] { "Tous", "Ouvert", "En attente", "Clos" };
            StatusFilter.SelectedIndex = 0;

            // Listes catégories/priorités
            DetailCategory.ItemsSource = new[] { "Incident", "Réseau", "Prêt matériel", "Dossier partagé" };
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
            if (_tickets is null) return;
            var view = _tickets;

            // filtre statut
            var statut = StatusFilter.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(statut) && statut != "Tous")
                view = view.Where(t => t.Status == statut);

            TicketsGrid.ItemsSource = view.ToList();
        }

        // Événements XAML

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void NewTicket_Click(object sender, RoutedEventArgs e)
        {
            // on appelle effectivement la même logique qu’avant
            var win = new NewTicketWindow(null) { Owner = this };
            if (win.ShowDialog() == true) LoadTickets();
        }

        // Ce handler est attaché à votre ComboBox StatusFilter
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshView();
        }

        // Ce handler est attaché à votre TextBox SearchBox
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshView();
        }

        // Vous aviez déjà Refresh_Click
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshView();
        }

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
            if (ticket is null) return;

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
                MessageBox.Show("Niveau maximal atteint.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                    $"Escalader «{t.Title}» de {t.IncidentLevel} à {t.IncidentLevel + 1} ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var ticket = db.Tickets.Find(t.Id)!;
            ticket.IncidentLevel++;
            ticket.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            LoadTickets();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!(TicketsGrid.SelectedItem is Ticket t)) return;
            if (MessageBox.Show($"Supprimer «{t.Title}» ?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            db.Tickets.Remove(db.Tickets.Find(t.Id)!);
            db.SaveChanges();
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
            TicketsGrid_SelectionChanged(null, null);
        }
    }
}
