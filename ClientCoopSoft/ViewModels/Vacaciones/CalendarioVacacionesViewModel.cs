using ClientCoopSoft.Views.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.Scheduler;
using System.Windows.Controls;
using System.Windows.Media;
using ClientCoopSoft.ViewModels.Licencias;
using ClientCoopSoft.Views.Vacaciones;

namespace ClientCoopSoft.ViewModels.Vacaciones
{
    public partial class CalendarioVacacionesViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajadorActual;

        [ObservableProperty]
        private ScheduleAppointmentCollection eventos = new();

        [ObservableProperty]
        private UserControl? contenidoActual; // Solo para mostrar la lista

        [ObservableProperty]
        private bool puedeAdministrar;

        [ObservableProperty]
        private bool puedeSolicitar;

        public CalendarioVacacionesViewModel(ApiClient apiClient, int idTrabajadorActual, bool puedeAdministrar,bool puedeSolicitar)
        {
            _apiClient = apiClient;
            _idTrabajadorActual = idTrabajadorActual;
            PuedeAdministrar = puedeAdministrar;
            PuedeSolicitar = puedeSolicitar;

            _ = CargarEventosAsync();
        }

        public async Task CargarEventosAsync()
        {
            Eventos.Clear();

            // 1) Vacaciones (Solicitudes)
            var solicitudes = await _apiClient.ObtenerVacacionesAsync();

            if (solicitudes != null)
            {
                foreach (var s in solicitudes)
                {
                    var appt = new ScheduleAppointment
                    {
                        StartTime = s.FechaInicio,
                        EndTime = s.FechaFin,
                        Subject = $"Vacación - {s.Trabajador}",
                        IsAllDay = true
                    };

                    appt.AppointmentBackground = s.EstadoSolicitud switch
                    {
                        "Pendiente" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // amarillo
                        "Aprobado" => new SolidColorBrush(Color.FromRgb(46, 204, 113)),  // verde
                        "Rechazado" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),   // rojo
                        _ => new SolidColorBrush(Colors.LightGray)
                    };

                    Eventos.Add(appt);
                }
            }

            // 2) Licencias
            var licencias = await _apiClient.ObtenerLicenciasAsync();

            if (licencias != null)
            {
                foreach (var l in licencias)
                {
                    // En el calendario mostramos solo fechas (IsAllDay),
                    // pero si quieres puedes diferenciar con horas más adelante.
                    var apptLic = new ScheduleAppointment
                    {
                        StartTime = l.FechaInicio,
                        EndTime = l.FechaFin,
                        Subject = $"Licencia - {l.ApellidosNombres} ({l.TipoLicencia})",
                        IsAllDay = true
                    };

                    // Color distinto para licencias (ej. azul)
                    apptLic.AppointmentBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219));

                    Eventos.Add(apptLic);
                }
            }
        }


        [RelayCommand]
        private async Task SolicitudesAsync()
        {
            if (!PuedeAdministrar)
                return;

            var vm = new ListarSolicitudesViewModel(_apiClient, () => ContenidoActual = null);

            await vm.CargarSolicitudesListaAsync();

            ContenidoActual = new SolicitudesVacView
            {
                DataContext = vm
            };
        }

        [RelayCommand]
        private async Task SolicitarAsync()
        {
            if (!PuedeSolicitar)
                return;

            var vm = new CrearSolicitudVacacionViewModel(_apiClient, _idTrabajadorActual);

            await vm.CargarResumenVacacionesAsync();

            var win = new CrearSolicitudVacView
            {
                DataContext = vm,
                Owner = App.Current.MainWindow
            };

            vm.SolicitudCreada += async () =>
            {
                win.Close();
                await CargarEventosAsync();
            };

            win.ShowDialog();
        }

        [RelayCommand]
        private async Task SolicitarLicenciaAsync()
        {
            if (!PuedeSolicitar)
                return;

            var vm = new CrearLicenciaViewModel(_apiClient, _idTrabajadorActual);

            await vm.CargarTiposLicenciaAsync();

            var win = new CrearLicenciaWindow
            {
                DataContext = vm,
                Owner = App.Current.MainWindow
            };

            vm.LicenciaCreada += async () =>
            {
                win.Close();
                // Si más adelante también pintas licencias en el calendario, refrescas acá:
                await CargarEventosAsync();
            };

            win.ShowDialog();
        }
        [RelayCommand]
        private async Task LicenciasAsync()
        {
            if (!PuedeAdministrar)
                return;

            var vm = new ListarLicenciasViewModel(_apiClient, () => ContenidoActual = null);

            await vm.CargarLicenciasListaAsync();

            ContenidoActual = new SolicitudesLicenciasView
            {
                DataContext = vm
            };
        }


    }
}
