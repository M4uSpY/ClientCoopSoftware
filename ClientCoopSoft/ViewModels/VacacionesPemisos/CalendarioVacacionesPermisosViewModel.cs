using ClientCoopSoft.Models;
using ClientCoopSoft.Views.InformacionPersonal;
using ClientCoopSoft.Views.VacacionesPermisos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.Scheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientCoopSoft.ViewModels.VacacionesPemisos
{
    public partial class CalendarioVacacionesPermisosViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ScheduleAppointmentCollection eventos = new();
        [ObservableProperty] private UserControl? contenidoActual;

        public CalendarioVacacionesPermisosViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
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
                // Mapeo DTO -> ScheduleAppointment
                var appt = new ScheduleAppointment
                {
                    StartTime = s.FechaInicio,
                    EndTime = s.FechaFin,   // o s.FechaFin.AddDays(1) si quieres incluir el último día completo
                    Subject = $"{s.TipoSolicitud} - {s.Trabajador}",
                    Location = "",           // si luego quieres sucursal/oficina, etc.
                    IsAllDay = true          // vacaciones/permisos normalmente son de todo el día
                };

                switch (s.EstadoSolicitud)
                {
                    case "Pendiente":
                        appt.AppointmentBackground = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Amarillo
                        break;

                    case "Aprobado":
                        appt.AppointmentBackground = new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Verde
                        break;

                    case "Rechazado":
                        appt.AppointmentBackground = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Rojo
                        break;

                    default:
                        appt.AppointmentBackground = new SolidColorBrush(Colors.LightGray);
                        break;
                }

                Eventos.Add(appt);
            }
        }

        [RelayCommand]
        private async Task Solicitudes()
        {
            // 1. Crear el ViewModel de la lista
            var vm = new ListarSolicitudesViewModel(_apiClient,() => ContenidoActual = null);

            // 2. Cargar los datos
            await vm.CargarSolicitudesListaAsync();

            // 3. Asignarlo como DataContext de la vista
            ContenidoActual = new SolicitudesVacPerView
            {
                DataContext = vm
            };
        }

    }
}
