using ClientCoopSoft.ViewModels;
using System.Windows;

namespace ClientCoopSoft.Views
{
    /// <summary>
    /// Lógica de interacción para DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : Window
    {
        public DashboardMain(ApiClient apiClient, string rol, string nombreCompleto, int idPersonaActual)
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(apiClient, rol, nombreCompleto, idPersonaActual);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            
            //Application.Current.Shutdown();
            
        }

        private void btnExit_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
