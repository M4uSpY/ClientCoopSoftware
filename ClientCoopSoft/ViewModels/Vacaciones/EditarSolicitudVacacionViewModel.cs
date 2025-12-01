using ClientCoopSoft.DTO.Vacaciones;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Vacaciones
{
    public partial class EditarSolicitudVacacionViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idVacacion;
        private readonly int _idTrabajador;

        private readonly DateTime _fechaInicioOriginal;
        private readonly DateTime _fechaFinOriginal;

        [ObservableProperty] private DateTime fechaInicio;
        [ObservableProperty] private DateTime fechaFin;

        [ObservableProperty] private string motivo = string.Empty;
        [ObservableProperty] private string? observacion;

        [ObservableProperty] private int diasAnuales;
        [ObservableProperty] private int diasUsados;
        [ObservableProperty] private int diasDisponibles;

        [ObservableProperty] private int antiguedadAnios;
        [ObservableProperty] private int gestion;

        public event Action? SolicitudEditada;

        public EditarSolicitudVacacionViewModel(
            ApiClient apiClient,
            SolicitudVacListarDTO solicitud)
        {
            _apiClient = apiClient;
            _idVacacion = solicitud.IdVacacion;
            _idTrabajador = solicitud.IdTrabajador;        // 👈 viene del DTO

            _fechaInicioOriginal = solicitud.FechaInicio;
            _fechaFinOriginal = solicitud.FechaFin;

            FechaInicio = solicitud.FechaInicio;
            FechaFin = solicitud.FechaFin;
            Motivo = solicitud.Motivo ?? string.Empty;
            // Observacion: cuando la traigas en el DTO o desde un GET por Id, la setéas aquí.

            _ = CargarResumenVacacionesAsync();
        }

        public async Task CargarResumenVacacionesAsync()
        {
            try
            {
                var resumen = await _apiClient.ObtenerResumenVacacionesAsync(_idTrabajador);
                if (resumen is null)
                    return;

                Gestion = resumen.Gestion;
                AntiguedadAnios = resumen.AntiguedadAnios;
                DiasAnuales = resumen.DiasDerecho;
                DiasUsados = resumen.DiasUsados;
                DiasDisponibles = resumen.DiasDisponibles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando resumen vacaciones: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MessageBox.Show("Debe ingresar un motivo.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FechaFin.Date < FechaInicio.Date)
            {
                MessageBox.Show("La fecha fin no puede ser menor que la fecha inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 👉 Si quieres forzar "solo acortar", descomenta esto:
            // if (FechaFin.Date > _fechaFinOriginal.Date)
            // {
            //     MessageBox.Show("Solo se permite acortar la vacación, no extenderla.",
            //         "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return;
            // }

            var dto = new SolicitudVacEditarDTO
            {
                FechaInicio = FechaInicio.Date,
                FechaFin = FechaFin.Date,
                Motivo = Motivo.Trim(),
                Observacion = Observacion
            };

            var (ok, error) = await _apiClient.ActualizarSolicitudVacacionAsync(_idVacacion, dto);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo guardar la edición de la solicitud."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Solicitud de vacación actualizada correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);

            SolicitudEditada?.Invoke();
        }

        [RelayCommand]
        private void Cancelar()
        {
            SolicitudEditada?.Invoke();
        }
    }
}
