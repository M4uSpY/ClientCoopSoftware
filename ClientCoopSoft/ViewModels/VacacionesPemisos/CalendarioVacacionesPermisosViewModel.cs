using ClientCoopSoft.Models;
using ClientCoopSoft.ViewModels.VacacionesPermisos;
using ClientCoopSoft.Views.VacacionesPermisos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.Scheduler;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientCoopSoft.ViewModels.VacacionesPemisos
{
    public partial class CalendarioVacacionesPermisosViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajadorActual;

        [ObservableProperty]
        private ScheduleAppointmentCollection eventos = new();

        [ObservableProperty]
        private UserControl? contenidoActual; // Solo para mostrar la lista

        [ObservableProperty]
        private bool esAdmin;

        public CalendarioVacacionesPermisosViewModel(ApiClient apiClient, int idTrabajadorActual, bool esAdmin)
        {
            _apiClient = apiClient;
            _idTrabajadorActual = idTrabajadorActual;
            EsAdmin = esAdmin;

            _ = CargarEventosAsync();
        }

        public async Task CargarEventosAsync()
        {
            var lista = await _apiClient.ObtenerVacacionesPermisosAsync();
            Eventos.Clear();

            if (lista == null)
                return;

            foreach (var s in lista)
            {
                var appt = new ScheduleAppointment
                {
                    StartTime = s.FechaInicio,
                    EndTime = s.FechaFin,
                    Subject = $"{s.TipoSolicitud} - {s.Trabajador}",
                    IsAllDay = true
                };

                appt.AppointmentBackground = s.EstadoSolicitud switch
                {
                    "Pendiente" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                    "Aprobado" => new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };

                Eventos.Add(appt);
            }
        }

        [RelayCommand]
        private async Task SolicitudesAsync()
        {
            var vm = new ListarSolicitudesViewModel(_apiClient, () => ContenidoActual = null);

            await vm.CargarSolicitudesListaAsync();

            ContenidoActual = new SolicitudesVacPerView
            {
                DataContext = vm
            };
        }

        [RelayCommand]
        private async Task SolicitarAsync()
        {
            var vm = new CrearSolicitudVacPermViewModel(_apiClient, _idTrabajadorActual);

            await vm.CargarCombosAsync();

            var win = new CrearSolicitudVacPermWindow
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
    }
}
