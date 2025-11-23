using ClientCoopSoft.DTO.VacacionesPermisos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace ClientCoopSoft.ViewModels.VacacionesPermisos
{
    public partial class CrearSolicitudVacPermViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        [ObservableProperty] private DateTime fechaInicio = DateTime.Today;
        [ObservableProperty] private DateTime fechaFin = DateTime.Today;

        [ObservableProperty] private string motivo = string.Empty;
        [ObservableProperty] private string? observacion;

        [ObservableProperty] private int diasAnuales;
        [ObservableProperty] private int diasUsados;
        [ObservableProperty] private int diasDisponibles;

        [ObservableProperty] private int antiguedadAnios;
        [ObservableProperty] private int gestion;

        public event Action? SolicitudCreada;

        public CrearSolicitudVacPermViewModel(ApiClient apiClient, int idTrabajador)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
        }

        [RelayCommand]
        private async Task EnviarAsync()
        {
            // Validaciones básicas
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

            var dto = new SolicitudVacPermCrearDTO
            {
                IdTrabajador = _idTrabajador,
                FechaInicio = FechaInicio.Date,
                FechaFin = FechaFin.Date,
                Motivo = Motivo.Trim(),
                Observacion = Observacion
            };

            var (ok, error) = await _apiClient.CrearSolicitudVacPermAsync(dto);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo enviar la solicitud."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Solicitud de vacación enviada correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);

            SolicitudCreada?.Invoke();
        }

        public async Task CargarResumenVacacionesAsync()
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

        [RelayCommand]
        private void Cancelar()
        {
            SolicitudCreada?.Invoke();
        }
    }
}
