using ClientCoopSoft.ViewModels.Asistencia;
using ClientCoopSoft.ViewModels.BoletasPago;
using ClientCoopSoft.ViewModels.Faltas;
using ClientCoopSoft.ViewModels.InformacionPersonal;
using ClientCoopSoft.ViewModels.Inicio;
using ClientCoopSoft.ViewModels.LogsAcceso;
using ClientCoopSoft.ViewModels.Planillas;
using ClientCoopSoft.ViewModels.Trabajadores;
using ClientCoopSoft.ViewModels.VacacionesPemisos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using ClientCoopSoft;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idPersonaActual;
        private readonly int _idUsuarioActual;


        [ObservableProperty] private string bienvenida = string.Empty;
        [ObservableProperty] private string rol = string.Empty;
        [ObservableProperty] private string info = string.Empty;
        [ObservableProperty] private ObservableObject? currentView;
        [ObservableProperty] private string menuSeleccionado = string.Empty;
        [ObservableProperty]
        private bool isPlanillasExpandida;


        [ObservableProperty] private BitmapImage? fotoPerfil;

        public bool IsAdmin => string.Equals(Rol, "Administrador", System.StringComparison.OrdinalIgnoreCase) || string.Equals(Rol, "Admin", System.StringComparison.OrdinalIgnoreCase);

        public DashboardViewModel(ApiClient apiClient, string rolName, string NombreCompleto, int idPersonaActual, int idUsuarioActual)
        {
            _apiClient = apiClient;
            _idPersonaActual = idPersonaActual;
            _idUsuarioActual = idUsuarioActual;

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
        // Abrir / cerrar el submenú de planillas
        [RelayCommand]
        private void TogglePlanillas()
        {
            IsPlanillasExpandida = !IsPlanillasExpandida;
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
        private async Task AbrirVacacionesPermisosAsync()
        {
            MenuSeleccionado = "VacacionesPermisos";

            var persona = await _apiClient.ObtenerPersonaAsync(_idPersonaActual);
            if (persona?.Trabajador == null)
            {
                MessageBox.Show("No se encontró el trabajador asociado a la persona actual.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var idTrabajadorActual = persona.Trabajador.IdTrabajador;

            var calendarioVM = new CalendarioVacacionesPermisosViewModel(_apiClient, idTrabajadorActual, IsAdmin);
            await calendarioVM.CargarEventosAsync();
            CurrentView = calendarioVM;
        }


        [RelayCommand]
        private async Task AbrirAsistenciasAsync()
        {
            MenuSeleccionado = "Asistencias";
            var listaAsistenciasVM = new ListarAsistenciasViewModel(_apiClient);

            await listaAsistenciasVM.CargarAsistenciasAsync();
            CurrentView = listaAsistenciasVM;
        }

        [RelayCommand]
        private async Task AbrirBoletasPagoAsync()
        {
            MenuSeleccionado = "BoletasPago";
            var listaBoletasPagoVM = new ListaBoletasPagoViewModel(_apiClient);

            //await listaAsistenciasVM.CargarAsistenciasAsync();
            CurrentView = listaBoletasPagoVM;
        }

        [RelayCommand]
        private async Task AbrirFaltasAsync()
        {
            MenuSeleccionado = "Faltas";
            var listaFaltasVM = new ListaFaltasViewModel(_apiClient);

            await listaFaltasVM.CargarFaltasAsync();
            CurrentView = listaFaltasVM;
        }
        // PLANILLA DE SUELDOS Y SALARIOS
        [RelayCommand]
        private async Task AbrirPlanillaSueldosAsync()
        {
            if (!IsAllowed())
            {
                MessageBox.Show("No tiene permiso para ver Planillas.", "Acceso denegado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MenuSeleccionado = "PlanillaSueldos";
            var vm = new PlanillaSueldosSalariosViewModel(_apiClient);
            CurrentView = vm;

            await Task.CompletedTask;
        }

        // PLANILLA DE APORTES Y BENEFICIOS (por ahora solo pantalla informativa)
        [RelayCommand]
        private async Task AbrirPlanillaAportesAsync()
        {
            if (!IsAllowed())
            {
                MessageBox.Show("No tiene permiso para ver Planillas.", "Acceso denegado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MenuSeleccionado = "PlanillaAportes";
            var vm = new PlanillaAPatronalesBSocialesViewModel(_apiClient);
            CurrentView = vm;

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AbrirLogsAsync()
        {
            MenuSeleccionado = "Logs";
            var listaLogsVM = new LogsViewModel(_apiClient);

            await listaLogsVM.CargarLogsAccesoAsync();
            CurrentView = listaLogsVM;
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

        [RelayCommand]
        private async Task CerrarSesionAsync()
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // Llamada al backend para registrar logout
            var ok = await _apiClient.LogoutAsync(_idUsuarioActual);

            if (!ok)
            {
                MessageBox.Show("No se pudo cerrar sesión correctamente.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var ventanaActual = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            var loginWindow = new MainWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            if(ventanaActual != null)
            {
                ventanaActual.Close();
            }
                

            // Cerrar la ventana principal
            //Application.Current.MainWindow.Close();
        }

    }
}
