using ClientCoopSoft.DTO.Capacitaciones;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels.Capacitaciones
{
    public partial class CapacitacionEditarViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        public event EventHandler<bool>? CerrarVentanaSolicitado;

        [ObservableProperty] private int idCapacitacion;
        [ObservableProperty] private int idTrabajador;
        [ObservableProperty] private string titulo = string.Empty;
        [ObservableProperty] private string institucion = string.Empty;
        [ObservableProperty] private int cargaHoraria;
        [ObservableProperty] private DateTime fecha = DateTime.Today;
        [ObservableProperty] private byte[]? fotoBytes;
        [ObservableProperty] private BitmapImage? fotoPreview;

        public string TituloVentana => "Editar capacitacion";

        public CapacitacionEditarViewModel(ApiClient apiClient, CapacitacionEditarDTO dto)
        {
            _apiClient = apiClient;

            IdCapacitacion = dto.IdCapacitacion;
            IdTrabajador = dto.IdTrabajador;
            Titulo = dto.Titulo;
            Institucion = dto.Institucion;
            CargaHoraria = dto.CargaHoraria;
            Fecha = dto.Fecha;
            FotoBytes = dto.ArchivoCertificado;
        }
        [RelayCommand]
        private void SubirFoto()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Seleccionar Foto"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    FotoBytes = File.ReadAllBytes(openFileDialog.FileName);

                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(FotoBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    FotoPreview = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
            }
        }
        [RelayCommand]
        private async Task GuardarAsync()
        {
            var dto = new CapacitacionEditarDTO
            {
                IdCapacitacion = IdCapacitacion,
                IdTrabajador = IdTrabajador,
                Titulo = Titulo,
                Institucion = Institucion,
                CargaHoraria = CargaHoraria,
                Fecha = Fecha,
                ArchivoCertificado = FotoBytes
            };
            var creada = await _apiClient.ActualizarCapacitacionAsync(IdCapacitacion, dto);
            if (creada)
            {
                CerrarVentanaSolicitado?.Invoke(this, true);
            }
        }
        [RelayCommand]
        private async Task EliminarAsync()
        {
            if (IdCapacitacion <= 0)
            {
                CerrarVentanaSolicitado?.Invoke(this, false);
                return;
            }

            var resp = MessageBox.Show(
                "¿Seguro que deseas eliminar esta capacitacion?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resp != MessageBoxResult.Yes)
                return;

            var ok = await _apiClient.EliminarCapacitacionAsync(IdCapacitacion);
            if (ok)
            {
                CerrarVentanaSolicitado?.Invoke(this, true);
            }
        }
        [RelayCommand]
        private void Cancelar()
        {
            CerrarVentanaSolicitado?.Invoke(this, false);
        }
    }
}
