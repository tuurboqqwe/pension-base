using System;
using System.Windows;
using PensionBase.Data;

namespace PensionBase.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;

            var login = LoginBox.Text.Trim();
            if (login.Length == 0 || PasswordInput.Password.Length == 0)
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                var user = UserRepository.Authenticate(login, PasswordInput.Password);
                if (user == null)
                {
                    ShowError("Неверный логин или пароль.");
                    PasswordInput.Clear();
                    return;
                }

                new MainWindow(user).Show();
                Close();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка базы данных: " + ex.Message);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoginBox.Text = window.RegisteredLogin;
                PasswordInput.Clear();
                PasswordInput.Focus();
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visibility = Visibility.Visible;
        }
    }
}
