using System;
using System.Windows;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Views
{
    public partial class NewTicketWindow : Window
    {
        private readonly User _user;

        public NewTicketWindow(User user)
        {
            InitializeComponent();
            _user = user;

            // Remplissage des dropdowns
            CategoryBox.ItemsSource = new[]
            {
                "Incident",
                "Réseau",
                "Prêt de matériel",
                "Dossier partagé"
            };
            PriorityBox.ItemsSource = new[]
            {
                "Basse",
                "Normale",
                "Élevée"
            };

            // Sélection par défaut
            CategoryBox.SelectedIndex = 0;
            PriorityBox.SelectedIndex = 1;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var ticket = new Ticket
            {
                UserId = _user.Id,
                Title = TitleBox.Text.Trim(),
                Description = DescBox.Text.Trim(),
                Category = CategoryBox.SelectedItem.ToString(),
                Priority = PriorityBox.SelectedItem.ToString(),
                Status = "Ouvert",
                IncidentLevel = 1,                  // ← Niveau initial forcé à 1
                CreatedAt = DateTime.Now
            };

            using var db = new AppDbContext();
            db.Tickets.Add(ticket);
            db.SaveChanges();

            DialogResult = true;
            Close();
        }
    }
}
