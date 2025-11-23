using ClientCoopSoft.DTO.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Licencias
{
    public partial class CrearLicenciaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        // FECHAS (solo fecha)
        [ObservableProperty]
        private DateTime fechaInicio = DateTime.Today;

        [ObservableProperty]
        private DateTime fechaFin = DateTime.Today;

        // HORAS como TEXTO (para evitar que WPF cambie la fecha)
        [ObservableProperty]
        private string horaInicioTexto = "08:30";

        [ObservableProperty]
        private string horaFinTexto = "16:30";

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

            // Parsear horas en formato HH:mm
            if (!TimeSpan.TryParseExact(HoraInicioTexto, "hh\\:mm", CultureInfo.InvariantCulture, out var horaInicio))
            {
                MessageBox.Show("La hora de inicio no es válida. Use el formato HH:mm (ej. 08:30).",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(HoraFinTexto, "hh\\:mm", CultureInfo.InvariantCulture, out var horaFin))
            {
                MessageBox.Show("La hora de fin no es válida. Use el formato HH:mm (ej. 16:30).",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FechaFin < FechaInicio)
            {
                MessageBox.Show("La fecha de fin no puede ser menor que la fecha de inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (horaFin <= horaInicio)
            {
                MessageBox.Show("La hora de fin debe ser mayor a la hora de inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new LicenciaCrearDTO
            {
                IdTrabajador = _idTrabajador,
                IdTipoLicencia = TipoSeleccionado.IdClasificador,

                FechaInicio = FechaInicio.Date,
                FechaFin = FechaFin.Date,

                HoraInicio = horaInicio,
                HoraFin = horaFin,

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
