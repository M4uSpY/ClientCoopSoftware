using ClientCoopSoft.DTO.FormacionAcademica;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels.FormacionAcademica
{
    public partial class FormacionAcademicaCrearViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        public event EventHandler<bool>? CerrarVentanaSolicitado;

        [ObservableProperty] private string nivelEstudios = string.Empty;
        [ObservableProperty] private string tituloObtenido = string.Empty;
        [ObservableProperty] private string institucion = string.Empty;
        [ObservableProperty] private int anioGraduacion = DateTime.Now.Year;
        [ObservableProperty] private string? nroRegistroProfesional;
        [ObservableProperty] private byte[]? fotoBytes;
        [ObservableProperty] private BitmapImage? fotoPreview;

        public string TituloVentana => "Nueva formación académica";

        public FormacionAcademicaCrearViewModel(ApiClient apiClient, int idTrabajador)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
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
                    var filePath = openFileDialog.FileName;

                    // 🔴 LIMITE 2 MB
                    var info = new FileInfo(filePath);
                    const long maxBytes = 2 * 1024 * 1024; // 2 MB
                    if (info.Length > maxBytes)
                    {
                        MessageBox.Show(
                            "El archivo del certificado no puede ser mayor a 2 MB.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    FotoBytes = File.ReadAllBytes(filePath);

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
            var dto = new FormacionAcademicaCrearDTO
            {
                IdTrabajador = _idTrabajador,
                NivelEstudios = NivelEstudios,
                TituloObtenido = TituloObtenido,
                Institucion = Institucion,
                AnioGraduacion = AnioGraduacion,
                NroRegistroProfesional = NroRegistroProfesional,
                ArchivoPdf = FotoBytes
            };

            var creada = await _apiClient.CrearFormacionAcademicaAsync(dto);
            if (creada != null)
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
