using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using PensionBase.Data;
using PensionBase.Models;

namespace PensionBase.Views
{
    public partial class PensionerEditWindow : Window
    {
        private readonly Pensioner _pensioner;

        // pensioner == null означает добавление новой записи
        public PensionerEditWindow(List<PensionType> pensionTypes, Pensioner pensioner)
        {
            InitializeComponent();
            TypeBox.ItemsSource = pensionTypes;

            _pensioner = pensioner;
            if (pensioner == null)
            {
                Title = "Новый пенсионер";
                TypeBox.SelectedIndex = 0;
                AssignedDatePicker.SelectedDate = DateTime.Today;
                return;
            }

            Title = "Карточка: " + pensioner.FullName;
            LastNameBox.Text = pensioner.LastName;
            FirstNameBox.Text = pensioner.FirstName;
            MiddleNameBox.Text = pensioner.MiddleName;
            BirthDatePicker.SelectedDate = pensioner.BirthDate;
            GenderBox.SelectedIndex = pensioner.Gender == "М" ? 0 : 1;
            SnilsBox.Text = pensioner.Snils;
            TypeBox.SelectedItem = pensionTypes.First(t => t.Id == pensioner.PensionTypeId);
            AmountBox.Text = pensioner.PensionAmount.ToString("0.00", CultureInfo.CurrentCulture);
            AssignedDatePicker.SelectedDate = pensioner.AssignedDate;
            PhoneBox.Text = pensioner.Phone;
            AddressBox.Text = pensioner.Address;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var error = Validate(out var amount);
            if (error != null)
            {
                ShowError(error);
                return;
            }

            var pensioner = new Pensioner
            {
                Id = _pensioner?.Id ?? 0,
                LastName = LastNameBox.Text.Trim(),
                FirstName = FirstNameBox.Text.Trim(),
                MiddleName = MiddleNameBox.Text,
                BirthDate = BirthDatePicker.SelectedDate.Value,
                Gender = (string)((ComboBoxItem)GenderBox.SelectedItem).Content,
                Snils = SnilsBox.Text.Trim(),
                Address = AddressBox.Text,
                Phone = PhoneBox.Text,
                PensionTypeId = ((PensionType)TypeBox.SelectedItem).Id,
                PensionAmount = amount,
                AssignedDate = AssignedDatePicker.SelectedDate.Value
            };

            try
            {
                if (PensionerRepository.SnilsExists(pensioner.Snils, pensioner.Id))
                {
                    ShowError("Запись с таким СНИЛС уже есть.");
                    return;
                }

                if (_pensioner == null)
                    PensionerRepository.Insert(pensioner);
                else
                    PensionerRepository.Update(pensioner);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка базы данных: " + ex.Message);
            }
        }

        // Возвращает текст ошибки или null, если всё заполнено правильно
        private string Validate(out decimal amount)
        {
            amount = 0;

            if (string.IsNullOrWhiteSpace(LastNameBox.Text) || string.IsNullOrWhiteSpace(FirstNameBox.Text))
                return "Заполните фамилию и имя.";
            if (BirthDatePicker.SelectedDate == null)
                return "Укажите дату рождения.";
            if (BirthDatePicker.SelectedDate.Value > DateTime.Today)
                return "Дата рождения не может быть в будущем.";
            if (GenderBox.SelectedItem == null)
                return "Выберите пол.";
            if (!Regex.IsMatch(SnilsBox.Text.Trim(), @"^\d{3}-\d{3}-\d{3} \d{2}$"))
                return "СНИЛС нужно ввести в формате 000-000-000 00.";
            if (TypeBox.SelectedItem == null)
                return "Выберите вид пенсии.";

            // Принимаем и запятую, и точку в качестве разделителя
            var amountText = AmountBox.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                || amount < 0 || amount > 99999999.99m)
                return "Размер пенсии должен быть неотрицательным числом.";

            if (AssignedDatePicker.SelectedDate == null)
                return "Укажите дату назначения.";
            if (AssignedDatePicker.SelectedDate.Value < BirthDatePicker.SelectedDate.Value)
                return "Дата назначения не может быть раньше даты рождения.";

            return null;
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visibility = Visibility.Visible;
        }
    }
}
