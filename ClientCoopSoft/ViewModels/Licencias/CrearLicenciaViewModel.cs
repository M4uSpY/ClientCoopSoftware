using ClientCoopSoft.DTO.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
        [ObservableProperty] private TimeSpan horaInicio = TimeSpan.FromHours(8.5);

        [ObservableProperty] private TimeSpan horaFin = TimeSpan.FromHours(16.5);

        [ObservableProperty]
        private ObservableCollection<TipoLicencia> tiposLicencia = new();
        [ObservableProperty]
        private TipoLicencia? tipoSeleccionado;

        [ObservableProperty]
        private string motivo = string.Empty;
        [ObservableProperty]
        private string? observacion;

        [ObservableProperty]
        private string? nombreArchivoJustificativo;

        private string? rutaArchivoJustificativo;

        // Texto informativo según tipo (opcional)
        [ObservableProperty] private string infoTipoLicencia = string.Empty;

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

        partial void OnTipoSeleccionadoChanged(TipoLicencia? value)
        {
            if (value is null)
            {
                InfoTipoLicencia = string.Empty;
                return;
            }

            switch (value.ValorCategoria)
            {
                case "Maternidad":
                    InfoTipoLicencia = "Hasta 45 jornadas laborales (prenatal y postnatal).";
                    break;
                case "Paternidad":
                    InfoTipoLicencia = "Exactamente 3 jornadas laborales (3 días corridos).";
                    break;
                case "Matrimonio":
                    InfoTipoLicencia = "Exactamente 3 jornadas laborales.";
                    break;
                case "Luto / Duelo":
                    InfoTipoLicencia = "Exactamente 3 jornadas laborales.";
                    break;
                case "Cumpleaños":
                    InfoTipoLicencia = "Media jornada el día de tu cumpleaños, no acumulable.";
                    break;
                case "Permiso temporal":
                    InfoTipoLicencia = "Máximo 3 horas al mes (puede ser fraccionado).";
                    break;
                case "Capacitación / Formación profesional":
                    InfoTipoLicencia = "Máximo 2 horas por día (se compensa en la jornada).";
                    break;
                default:
                    InfoTipoLicencia = string.Empty;
                    break;
            }
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

            if (FechaFin < FechaInicio)
            {
                MessageBox.Show("La fecha fin no puede ser menor que la fecha inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HoraFin <= HoraInicio)
            {
                MessageBox.Show("La hora fin debe ser mayor que la hora inicio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new LicenciaCrearDTO
            {
                IdTrabajador = _idTrabajador,
                IdTipoLicencia = TipoSeleccionado.IdClasificador,

                FechaInicio = FechaInicio.Date,
                FechaFin = FechaFin.Date,

                HoraInicio = HoraInicio,
                HoraFin = HoraFin,

                Motivo = Motivo.Trim(),
                Observacion = Observacion
            };

            // Leer archivo a byte[] si se seleccionó
            if (!string.IsNullOrWhiteSpace(rutaArchivoJustificativo) &&
                File.Exists(rutaArchivoJustificativo))
            {
                dto.ArchivoJustificativo = File.ReadAllBytes(rutaArchivoJustificativo);
            }

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

        [RelayCommand]
        private void SeleccionarArchivo()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Seleccionar archivo justificativo (PDF)",
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                CheckFileExists = true,
                Multiselect = false
            };

            if (ofd.ShowDialog() == true)
            {
                rutaArchivoJustificativo = ofd.FileName;
                NombreArchivoJustificativo = Path.GetFileName(ofd.FileName);
            }
        }
    }
}
