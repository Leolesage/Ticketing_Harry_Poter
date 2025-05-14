using System.Linq;
using System.Windows;
using Ticketing_Harry_Poter.Data;
using Ticketing_Harry_Poter.Models;
using Ticketing_Harry_Poter.Views;

namespace Ticketing_Harry_Poter.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Admin niveau 1
        private void Admin_Click1(object s, RoutedEventArgs e) => OpenAdmin(1);
        // Admin niveau 2
        private void Admin_Click2(object s, RoutedEventArgs e) => OpenAdmin(2);
        // Admin niveau 3
        private void Admin_Click3(object s, RoutedEventArgs e) => OpenAdmin(3);

        private void OpenAdmin(int level)
        {
            var win = new AdminDashboard(level);
            win.Show();
            Close();
        }

        private void User_Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            var prenom = FirstNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(prenom))
            {
                ErrorText.Text = "Veuillez saisir votre prénom.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            using var db = new AppDbContext();
            var user = db.Users
                         .FirstOrDefault(u => u.Username.ToLower() == prenom.ToLower()
                                           && u.Role == "User");
            if (user == null)
            {
                ErrorText.Text = $"Prénom '{prenom}' non reconnu.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            new UserDashboard(user).Show();
            Close();
        }
    }
}
