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
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF1cX2hIfEx3QXxbf1x1ZFRMZFVbRnRPIiBoS35Rc0RiWHtfc3FVQ2NbUEBzVEFc");
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _apiClient = new ApiClient("http://localhost:5000/");

            var loginVm = new LoginViewModel(_apiClient);
            var loginView = new MainWindow { DataContext = loginVm };
            loginView.Show();
        }
    }


}
