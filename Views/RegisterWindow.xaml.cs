using System;
using System.Windows;
using PensionBase.Data;

namespace PensionBase.Views
{
    public partial class RegisterWindow : Window
    {
        private const int MinPasswordLength = 6;

        public string RegisteredLogin { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameBox.Text.Trim();
            var login = LoginBox.Text.Trim();

            if (fullName.Length == 0 || login.Length == 0)
            {
                ShowError("Заполните ФИО и логин.");
                return;
            }
            if (PasswordInput.Password.Length < MinPasswordLength)
            {
                ShowError("Пароль должен быть не короче " + MinPasswordLength + " символов.");
                return;
            }
            if (PasswordInput.Password != ConfirmInput.Password)
            {
                ShowError("Пароли не совпадают.");
                return;
            }

            try
            {
                if (UserRepository.LoginExists(login))
                {
                    ShowError("Такой логин уже занят.");
                    return;
                }

                UserRepository.Register(login, fullName, PasswordInput.Password);
                RegisteredLogin = login;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка базы данных: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visibility = Visibility.Visible;
        }
    }
}
