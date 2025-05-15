using System;
using System.Linq;
using System.Windows;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Views
{
    public partial class NewTicketWindow : Window
    {
        private readonly User _currentUser;

        public NewTicketWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            // Initialiser les listes de catégories et priorités
            CategoryBox.ItemsSource = new[] { "Incident", "Réseau", "Prêt de matériel", "Dossier partagé" };
            CategoryBox.SelectedIndex = 0;

            PriorityBox.ItemsSource = new[] { "Basse", "Normale", "Élevée" };
            PriorityBox.SelectedIndex = 1;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            // Récupération des valeurs
            var title = TitleBox.Text.Trim();
            var category = CategoryBox.SelectedItem as string;
            var priority = PriorityBox.SelectedItem as string;
            var description = DescBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(priority) ||
                string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Tous les champs sont requis.", "Erreur",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sauvegarde en base
            using var db = new AppDbContext();
            db.Tickets.Add(new Ticket
            {
                Title = title,
                Category = category,
                Priority = priority,
                Description = description,
                UserId = _currentUser.Id,
                Status = "Ouvert",
                IncidentLevel = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            db.SaveChanges();

            DialogResult = true; // ferme la fenêtre avec résultat "OK"
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
