using System.Linq;
using System.Windows;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // L’utilisateur doit toujours entrer son prénom
        private void User_Click(object sender, RoutedEventArgs e)
        {
            var prenom = FirstNameBox.Text;
            if (string.IsNullOrEmpty(prenom))
            {
                ErrorText.Text = "Veuillez renseigner un prénom.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == prenom.ToLower() && u.Role == "User");
            if (user == null)
            {
                ErrorText.Text = $"Prénom '{prenom}' non reconnu.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            var uw = new UserDashboard(user);
            uw.Show();
            Close();
        }

        // Pour les admins, on ouvre directement sans prénom
        private void Admin_Click1(object sender, RoutedEventArgs e)
        {
            var aw = new AdminDashboard(1);
            aw.Show();
            Close();
        }

        private void Admin_Click2(object sender, RoutedEventArgs e)
        {
            var aw = new AdminDashboard(2);
            aw.Show();
            Close();
        }

        private void Admin_Click3(object sender, RoutedEventArgs e)
        {
            var aw = new AdminDashboard(3);
            aw.Show();
            Close();
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
