using ClientCoopSoft.ViewModels.InformacionPersonal;
using ClientCoopSoft.ViewModels.Inicio;
using ClientCoopSoft.ViewModels.Trabajadores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idPersonaActual;

        [ObservableProperty] private string bienvenida = string.Empty;
        [ObservableProperty] private string rol = string.Empty;
        [ObservableProperty] private string info = string.Empty;
        [ObservableProperty] private ObservableObject? currentView;
        [ObservableProperty] private string menuSeleccionado = string.Empty;

        [ObservableProperty] private BitmapImage? fotoPerfil;

        public bool IsAdmin => string.Equals(Rol, "Administrador", System.StringComparison.OrdinalIgnoreCase) || string.Equals(Rol, "Admin", System.StringComparison.OrdinalIgnoreCase);

        public DashboardViewModel(ApiClient apiClient, string rolName, string NombreCompleto, int idPersonaActual)
        {
            _apiClient = apiClient;
            _idPersonaActual = idPersonaActual;

            Rol = $"{rolName}";
            Bienvenida = $"Bienvenido";
            Info = $"{NombreCompleto}";

            MenuSeleccionado = "Inicio";
            CurrentView = new InicioViewModel();

            _ = CargarFotoPerfilAsync();
        }
        private bool IsAllowed()
        {
            return IsAdmin;
        }
        [RelayCommand]
        private Task AbrirInicioAsync() {
            MenuSeleccionado = "Inicio";
            CurrentView = new InicioViewModel();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task OpenUsuariosAsync()
        {
            if (!IsAllowed())
            {
                System.Windows.MessageBox.Show("No tiene permiso para ver Usuarios.", "Acceso denegado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            MenuSeleccionado = "Usuarios";
            var usuariosVM = new UsuariosViewModel(_apiClient);
            await usuariosVM.LoadUsuariosAsync();
            CurrentView = usuariosVM;
        }
        [RelayCommand]
        private async Task AbrirTrabajadoresAsync()
        {
            if (!IsAllowed())
            {
                System.Windows.MessageBox.Show("No tiene permiso para ver trabajadores.", "Acceso denegado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            MenuSeleccionado = "Trabajadores";
            var trabajadoresVM = new ListarTrabajadoresViewModel(_apiClient);
            await trabajadoresVM.CargarTrabajadoresAsync();
            CurrentView = trabajadoresVM;
        }

        [RelayCommand]
        private async Task AbrirPersonasAsync()
        {
            if (!IsAllowed())
            {
                System.Windows.MessageBox.Show("No tiene permiso para ver Personas.", "Acceso denegado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            MenuSeleccionado = "Personas";
            var personasVM = new PersonasViewModel(_apiClient);
            await personasVM.CargarPersonasAsync();
            CurrentView = personasVM;
        }
        [RelayCommand]
        private async Task AbrirInformacionPersonalAsync()
        {
            MenuSeleccionado = "InformacionPersonal";
            var persona = await _apiClient.ObtenerPersonaAsync(_idPersonaActual);
            if (persona == null)
            {
                System.Windows.MessageBox.Show("No se pudo cargar la información personal.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var infpersonalVM = new InfPersonalViewModel(persona, _apiClient);
            CurrentView = infpersonalVM;
        }

        [RelayCommand]
        private void CambiarMenu(string menu)
        {
            MenuSeleccionado = menu;
        }

        private async Task CargarFotoPerfilAsync()
        {
            try
            {
                var persona = await _apiClient.ObtenerPersonaAsync(_idPersonaActual);
                if (persona != null && persona.PrevisualizarImagen != null)
                {
                    FotoPerfil = persona.PrevisualizarImagen;
                }
                else
                {
                    FotoPerfil = CargarFromResource("/Assets/fotoIcon.png");
                }
            }
            catch
            {
                FotoPerfil = CargarFromResource("/Assets/fotoIcon.png");
            }
        }

        private BitmapImage? CargarFromResource(string resourcePath)
        {
            try
            {
                var uri = resourcePath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                          ? new Uri(resourcePath, UriKind.Absolute)
                          : new Uri($"pack://application:,,,/{resourcePath.TrimStart('/')}", UriKind.Absolute);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                return bmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
