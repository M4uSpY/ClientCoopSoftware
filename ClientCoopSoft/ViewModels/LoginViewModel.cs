using ClientCoopSoft.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiClient _api;
        public LoginViewModel(ApiClient apiClient)
        {
            _api = apiClient;
        }


        [ObservableProperty]
        private string username = string.Empty;

        public string Password { get; set; } = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Ingrese usuario y password";
                return;
            }
            try
            {
                var resp = await _api.LoginAsync(Username, Password);
                if (resp is null)
                {
                    ErrorMessage = "Credenciales incorrectas";
                    return;
                }

                // abrir dashboard
                var dash = new DashboardMain(_api, resp.Rol, resp.NombreCompleto, resp.IdPersona, resp.IdUsuario);

                // reemplazar la ventana principal actual
                System.Windows.Application.Current.MainWindow = dash;

                // mostrar el dashboard
                dash.Show();

                // cerrar el login actual
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is ClientCoopSoft.MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "No se pudo conectar al servidorffadsf";

                // Ventana emergente con el detalle
                System.Windows.MessageBox.Show(
                    $"No se pudo conectar al servidor:\n\n{ex.Message}",
                    "Error de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Si quieres ver TODO el stack:
                System.Diagnostics.Debug.WriteLine("ERROR LOGIN: " + ex);
            }
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
