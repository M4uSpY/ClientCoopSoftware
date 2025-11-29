using ClientCoopSoft.DTO.Contratacion;
using ClientCoopSoft.Models;
using ClientCoopSoft.Views.Contrato;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels.Contratacion
{
    public partial class ContratoViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        private ContratoDTO? _contrato;

        [ObservableProperty] private string numeroContrato = string.Empty;
        [ObservableProperty] private ObservableCollection<TipoContrato> tipoContratos = new();
        [ObservableProperty] private TipoContrato? tipoContratoSeleccionado;
        [ObservableProperty] private ObservableCollection<PeriodoPago> periodosPagos = new();
        [ObservableProperty] private PeriodoPago? periodosPagoSeleccionado;
        [ObservableProperty] private DateTime fechaInicioContrato = DateTime.Today;
        [ObservableProperty] private DateTime fechaFinContrato = DateTime.Today;

        [ObservableProperty] private byte[]? fotoBytes;
        [ObservableProperty] private BitmapImage? fotoPreview;

        [ObservableProperty] private BitmapImage? archivoIconoPreview;
        [ObservableProperty] private string archivoNombre = "Sin archivo seleccionado";

        [ObservableProperty]
        private string mensajeArchivo = string.Empty;



        // Contenido dinámico para navegación de pestañas
        [ObservableProperty] private UserControl? contenidoActual;

        public ContratoViewModel(int idTrabajador, ApiClient apiClient)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;

            ContenidoActual = new ContratoView { DataContext = this };

            // Cargar datos iniciales de forma asíncrona
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            var contrato = await ObtenerTrabajador(_idTrabajador);
            if (contrato is null)
            {
                MensajeArchivo = "No hay archivo cargado.";
                MessageBox.Show("No se encontró información de contrato para este trabajador.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _contrato = contrato;

            NumeroContrato = contrato.NumeroContrato;
            FechaInicioContrato = contrato.FechaInicio;
            FechaFinContrato = contrato.FechaFin;
            FotoBytes = contrato.ArchivoPdf;

            // Mensaje según si hay archivo desde la API
            if (FotoBytes is not null && FotoBytes.Length > 0)
            {
                MensajeArchivo = "Archivo de contrato registrado.";
            }
            else
            {
                MensajeArchivo = "No hay archivo cargado.";
            }

            // Como ahora solo manejamos PDF, no usamos preview de imagen
            FotoPreview = null;


            await CargarTiposContrato(contrato.IdTipoContrato);
            await CargarPeriodoPagos(contrato.IdPeriodoPago);
        }

        private async Task<ContratoDTO?> ObtenerTrabajador(int idTrabajador)
        {
            var contratoTrab = await _apiClient.ObtenerContratoUltimoPorTrabajadorAsync(idTrabajador);
            return contratoTrab;
        }

        private async Task CargarTiposContrato(int idTipoContrato)
        {
            var listaContratos = await _apiClient.ObtenerTipoContratosAsync() ?? new List<TipoContrato>();
            TipoContratos = new ObservableCollection<TipoContrato>(listaContratos);
            TipoContratoSeleccionado = TipoContratos.FirstOrDefault(t => t.IdClasificador == idTipoContrato);
        }

        private async Task CargarPeriodoPagos(int idPeriodoPago)
        {
            var listaPeriodosPago = await _apiClient.ObtenerPeriodosPagoAsync() ?? new List<PeriodoPago>();
            PeriodosPagos = new ObservableCollection<PeriodoPago>(listaPeriodosPago);
            PeriodosPagoSeleccionado = PeriodosPagos.FirstOrDefault(t => t.IdClasificador == idPeriodoPago);
        }

        [RelayCommand]
        private void SubirFoto()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Documento PDF|*.pdf",
                    Title = "Seleccionar contrato (PDF)"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    FotoBytes = File.ReadAllBytes(filePath);

                    // Mostrar mensaje de éxito
                    MensajeArchivo = $"Archivo cargado: {Path.GetFileName(filePath)}";

                    // No preview porque es PDF
                    FotoPreview = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el archivo PDF: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                MensajeArchivo = "Error al cargar el archivo";
            }
        }




        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (_contrato is null)
            {
                MessageBox.Show("No se ha cargado la información del contrato.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PeriodosPagoSeleccionado is null ||
                TipoContratoSeleccionado is null ||
                string.IsNullOrWhiteSpace(NumeroContrato))
            {
                MessageBox.Show("Por favor completa todos los campos correctamente.",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FotoBytes is null || FotoBytes.Length == 0)
            {
                MessageBox.Show("Debe adjuntar el archivo del contrato.",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var contratoDTO = new ContratoActualizarDTO
            {
                IdContrato = _contrato.IdContrato,
                NumeroContrato = NumeroContrato,
                IdTipoContrato = TipoContratoSeleccionado.IdClasificador,
                IdPeriodoPago = PeriodosPagoSeleccionado.IdClasificador,
                FechaInicio = FechaInicioContrato,
                FechaFin = FechaFinContrato,
                ArchivoPdf = FotoBytes
            };

            bool exito = await _apiClient.ActualizarContratoAsync(_contrato.IdContrato, contratoDTO);

            MessageBox.Show(
                exito ? "Información del contrato actualizada correctamente" : "Error al actualizar el contrato",
                exito ? "Éxito" : "Error",
                MessageBoxButton.OK,
                exito ? MessageBoxImage.Information : MessageBoxImage.Warning
            );

        }
        [RelayCommand]
        private void DescargarContrato()
        {
            if (FotoBytes is null || FotoBytes.Length == 0)
            {
                MessageBox.Show(
                    "No hay ningún contrato PDF cargado para descargar.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Guardar contrato",
                    Filter = "Documento PDF|*.pdf",
                    FileName = $"Contrato_{NumeroContrato.Replace(" ", "_")}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, FotoBytes);
                    MessageBox.Show(
                        "Contrato guardado correctamente.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el contrato: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        [RelayCommand]
        private void Cancelar(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
