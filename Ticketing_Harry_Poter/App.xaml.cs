using System;
using System.Linq;
using System.Windows;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;
using Ticketing_Harry_Poter.Views;

namespace Ticketing_Harry_Poter
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // 1) Création / seed de la base si nécessaire
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                if (!db.Users.Any())
                {
                    var leo = new User { Username = "Léo", Role = "User" };
                    var antoine = new User { Username = "Antoine", Role = "User" };
                    var hugo = new User { Username = "Hugo", Role = "User" };
                    var admin1 = new User { Username = "Admin1", Role = "Admin1" };
                    var admin2 = new User { Username = "Admin2", Role = "Admin2" };
                    var admin3 = new User { Username = "Admin3", Role = "Admin3" };
                    db.Users.AddRange(leo, antoine, hugo, admin1, admin2, admin3);
                    db.SaveChanges();

                    db.Tickets.AddRange(
                        new Ticket
                        {
                            UserId = leo.Id,
                            Title = "Impossible VPN",
                            Description = "Le VPN saute toutes les 5 minutes",
                            Category = "Réseau",
                            Priority = "Élevée",
                            Status = "Ouvert",
                            IncidentLevel = 1,
                            CreatedAt = DateTime.Now.AddHours(-4)
                        },
                        new Ticket
                        {
                            UserId = antoine.Id,
                            Title = "Erreur SharePoint",
                            Description = "Page introuvable",
                            Category = "Incident",
                            Priority = "Normale",
                            Status = "EnAttente",
                            IncidentLevel = 2,
                            CreatedAt = DateTime.Now.AddDays(-1)
                        },
                        new Ticket
                        {
                            UserId = hugo.Id,
                            Title = "Demande vidéoprojecteur",
                            Description = "Vidéo pour réunion vendredi",
                            Category = "Prêt de matériel",
                            Priority = "Élevée",
                            Status = "Clos",
                            IncidentLevel = 3,
                            CreatedAt = DateTime.Now.AddDays(-2)
                        }
                    );
                    db.SaveChanges();
                }
            }

            // 2) Ouvre la fenêtre de login comme MainWindow
            var login = new Views.LoginWindow();
            MainWindow = login;
            login.Show();
        }
    }
}
