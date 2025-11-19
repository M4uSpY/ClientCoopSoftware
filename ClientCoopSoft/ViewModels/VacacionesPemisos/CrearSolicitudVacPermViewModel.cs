using ClientCoopSoft.DTO.VacacionesPermisos;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.VacacionesPermisos
{
    public partial class CrearSolicitudVacPermViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        [ObservableProperty] private DateTime fechaInicio = DateTime.Today;
        [ObservableProperty] private DateTime fechaFin = DateTime.Today;

        [ObservableProperty] private ObservableCollection<TipoSolicitud> tiposSolicitud = new();
        [ObservableProperty] private TipoSolicitud? tipoSeleccionado;

        [ObservableProperty] private string motivo = string.Empty;
        [ObservableProperty] private string? observacion;

        public event Action? SolicitudCreada;

        public CrearSolicitudVacPermViewModel(ApiClient apiClient, int idTrabajador)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
        }

        public async Task CargarCombosAsync()
        {
            var listaTipos = await _apiClient.ObtenerClasificadorPorTipoSolicitudAsync();

            if (listaTipos != null)
                TiposSolicitud = new ObservableCollection<TipoSolicitud>(listaTipos);
        }

        [RelayCommand]
        private async Task EnviarAsync()
        {
            if (TipoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar el tipo de solicitud.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MessageBox.Show("Debe ingresar un motivo.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FechaFin < FechaInicio)
            {
                MessageBox.Show("La fecha fin no puede ser menor que la fecha inicio.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new SolicitudVacPermCrearDTO
            {
                IdTrabajador = _idTrabajador,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                IdTipoSolicitud = TipoSeleccionado.IdClasificador,
                Motivo = Motivo.Trim(),
                Observacion = Observacion
            };

            var ok = await _apiClient.CrearSolicitudVacPermAsync(dto);

            if (!ok)
            {
                MessageBox.Show("No se pudo enviar la solicitud.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Solicitud enviada correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);

            SolicitudCreada?.Invoke();
        }

        [RelayCommand]
        private void Cancelar()
        {
            SolicitudCreada?.Invoke();
        }
    }
}
