using ClientCoopSoft.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ClientCoopSoft
{
    public partial class App : Application
    {
        private ApiClient _apiClient = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _apiClient = new ApiClient("https://localhost:7084/");

            var loginVm = new LoginViewModel(_apiClient);
            var loginView = new MainWindow { DataContext = loginVm };
            loginView.Show();
        }
    }


}
