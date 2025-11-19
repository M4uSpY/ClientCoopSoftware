using ClientCoopSoft.ViewModels;
using System.Windows;

namespace ClientCoopSoft.Views
{
    /// <summary>
    /// Lógica de interacción para DashboardMain.xaml
    /// </summary>
    public partial class DashboardMain : Window
    {
        public DashboardMain(ApiClient apiClient, string rol, string nombreCompleto, int idPersonaActual, int idUsuarioActual)
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(apiClient, rol, nombreCompleto, idPersonaActual, idUsuarioActual);
        }

    }
}
