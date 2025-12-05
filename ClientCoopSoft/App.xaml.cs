using ClientCoopSoft.ViewModels;
using Syncfusion.Licensing;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ClientCoopSoft
{
    public partial class App : Application
    {
        private ApiClient _apiClient = null!;

        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF1cXGFCf0x/WmFZfVhgdVRMYFtbRHNPIiBoS35Rc0RhWH9cc3VQRmJZVUN/VEFc");
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _apiClient = new ApiClient("https://localhost:7084/");

            var loginVm = new LoginViewModel(_apiClient);
            var loginView = new MainWindow { DataContext = loginVm };
            loginView.Show();
        }
    }


}
