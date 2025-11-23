using ClientCoopSoft.DTO.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Licencias
{
    public partial class CrearLicenciaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        // Fecha y hora de inicio / fin (usamos DateTime completos para el binding)
        [ObservableProperty]
        private DateTime fechaHoraInicio = DateTime.Today.AddHours(8.5); // 08:30

        [ObservableProperty]
        private DateTime fechaHoraFin = DateTime.Today.AddHours(16.5);   // 16:30

        [ObservableProperty]
        private ObservableCollection<TipoLicencia> tiposLicencia = new();

        [ObservableProperty]
        private TipoLicencia? tipoSeleccionado;

        [ObservableProperty]
        private string motivo = string.Empty;

        [ObservableProperty]
        private string? observacion;

        // evento para cerrar ventana y refrescar calendario
        public event Action? LicenciaCreada;

        public CrearLicenciaViewModel(ApiClient apiClient, int idTrabajador)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
        }

        public async Task CargarTiposLicenciaAsync()
        {
            var lista = await _apiClient.ObtenerTiposLicenciaAsync();
            if (lista != null)
                TiposLicencia = new ObservableCollection<TipoLicencia>(lista);
        }

        [RelayCommand]
        private async Task EnviarAsync()
        {
            if (TipoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar el tipo de licencia.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MessageBox.Show("Debe ingresar un motivo.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FechaHoraFin < FechaHoraInicio)
            {
                MessageBox.Show("La fecha y hora de fin no pueden ser menores que las de inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new LicenciaCrearDTO
            {
                IdTrabajador = _idTrabajador,
                IdTipoLicencia = TipoSeleccionado.IdClasificador,

                FechaInicio = FechaHoraInicio.Date,
                FechaFin = FechaHoraFin.Date,

                HoraInicio = FechaHoraInicio.TimeOfDay,
                HoraFin = FechaHoraFin.TimeOfDay,

                Motivo = Motivo.Trim(),
                Observacion = Observacion
            };

            var (ok, error) = await _apiClient.CrearLicenciaAsync(dto);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo registrar la licencia."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Licencia registrada correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LicenciaCreada?.Invoke();
        }

        [RelayCommand]
        private void Cancelar()
        {
            LicenciaCreada?.Invoke();
        }
    }
}
