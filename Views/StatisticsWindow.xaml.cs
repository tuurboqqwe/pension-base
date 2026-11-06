using System.Windows;
using PensionBase.Data;

namespace PensionBase.Views
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
            ByTypeGrid.ItemsSource = PensionerRepository.GetStatisticsByType().DefaultView;
            BelowAverageGrid.ItemsSource = PensionerRepository.GetBelowAverage().DefaultView;
        }
    }
}
