using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PensionBase.Data;
using PensionBase.Models;

namespace PensionBase.Views
{
    public partial class MainWindow : Window
    {
        private readonly List<PensionType> _pensionTypes;
        private bool _loaded;

        public MainWindow(User user)
        {
            InitializeComponent();
            UserLabel.Text = user.FullName;

            _pensionTypes = PensionerRepository.GetPensionTypes();

            // Первый пункт фильтра с Id = 0 означает «показать все»
            var filterItems = new List<PensionType> { new PensionType { Id = 0, Name = "Все виды" } };
            filterItems.AddRange(_pensionTypes);
            TypeFilter.ItemsSource = filterItems;
            TypeFilter.SelectedIndex = 0;

            _loaded = true;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var selected = TypeFilter.SelectedItem as PensionType;
                int? typeId = selected != null && selected.Id != 0 ? selected.Id : (int?)null;

                var items = PensionerRepository.Search(SearchBox.Text, typeId);
                PensionersGrid.ItemsSource = items;

                SummaryLabel.Text = items.Count == 0
                    ? "Записей не найдено"
                    : string.Format("Записей: {0}   Средний размер пенсии: {1:N2} руб.",
                        items.Count, items.Average(p => p.PensionAmount));
            }
            catch (Exception ex)
            {
                ShowDbError(ex);
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_loaded)
                LoadData();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var window = new PensionerEditWindow(_pensionTypes, null) { Owner = this };
            if (window.ShowDialog() == true)
                LoadData();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditSelected();
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Двойной клик по заголовку столбца не должен открывать редактирование
            if (e.OriginalSource is FrameworkElement element && element.DataContext is Pensioner)
                EditSelected();
        }

        private void EditSelected()
        {
            var pensioner = PensionersGrid.SelectedItem as Pensioner;
            if (pensioner == null)
            {
                MessageBox.Show("Сначала выберите запись в таблице.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new PensionerEditWindow(_pensionTypes, pensioner) { Owner = this };
            if (window.ShowDialog() == true)
                LoadData();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var pensioner = PensionersGrid.SelectedItem as Pensioner;
            if (pensioner == null)
            {
                MessageBox.Show("Сначала выберите запись в таблице.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show("Удалить запись «" + pensioner.FullName + "»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                PensionerRepository.Delete(pensioner.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowDbError(ex);
            }
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new StatisticsWindow { Owner = this }.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowDbError(ex);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void ShowDbError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
