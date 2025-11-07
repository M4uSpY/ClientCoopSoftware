using ClientCoopSoft.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ClientCoopSoft
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;

        public MainWindow()
        {
            InitializeComponent();

            // Crear la instancia única para esta sesión (mismo ApiClient que usará Login y luego Dashboard)
            _apiClient = new ApiClient("https://localhost:7084/");

            // Pasar la misma instancia al LoginViewModel
            DataContext = new LoginViewModel(_apiClient);
        }

        private void pwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }
    }
}
