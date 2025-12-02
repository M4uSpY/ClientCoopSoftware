using ClientCoopSoft.DTO.FormacionAcademica;
using ClientCoopSoft.Models;
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

namespace ClientCoopSoft.ViewModels.FormacionAcademica
{
    public partial class FormacionAcademicaEditarViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        public event EventHandler<bool>? CerrarVentanaSolicitado;

        [ObservableProperty] private int idFormacion;
        [ObservableProperty] private int idTrabajador;
        [ObservableProperty] private string nivelEstudios = string.Empty;
        [ObservableProperty] private string tituloObtenido = string.Empty;
        [ObservableProperty] private string institucion = string.Empty;
        [ObservableProperty] private int anioGraduacion;
        [ObservableProperty] private string? nroRegistroProfesional;
        [ObservableProperty] private byte[]? fotoBytes;
        [ObservableProperty] private BitmapImage? fotoPreview;

        public string TituloVentana => "Editar formación académica";

        public FormacionAcademicaEditarViewModel(ApiClient apiClient, FormacionAcademicaEditarDTO dto)
        {
            _apiClient = apiClient;

            IdFormacion = dto.IdFormacion;
            IdTrabajador = dto.IdTrabajador;
            NivelEstudios = dto.NivelEstudios;
            TituloObtenido = dto.TituloObtenido;
            Institucion = dto.Institucion;
            AnioGraduacion = dto.AnioGraduacion == 0 ? DateTime.Now.Year : dto.AnioGraduacion;
            NroRegistroProfesional = dto.NroRegistroProfesional;
            FotoBytes = dto.ArchivoPdf;
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
            if (IdFormacion <= 0)
                return;

            var dto = new FormacionAcademicaEditarDTO
            {
                IdFormacion = IdFormacion,
                IdTrabajador = IdTrabajador,
                NivelEstudios = NivelEstudios,
                TituloObtenido = TituloObtenido,
                Institucion = Institucion,
                AnioGraduacion = AnioGraduacion,
                NroRegistroProfesional = NroRegistroProfesional,
                ArchivoPdf = FotoBytes
            };

            var exito = await _apiClient.ActualizarFormacionAcademicaAsync(IdFormacion, dto);

            if (exito)
            {
                CerrarVentanaSolicitado?.Invoke(this, true);
            }
        }

        [RelayCommand]
        private async Task EliminarAsync()
        {
            if (IdFormacion <= 0)
            {
                CerrarVentanaSolicitado?.Invoke(this, false);
                return;
            }

            var resp = MessageBox.Show(
                "¿Seguro que deseas eliminar esta formación académica?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resp != MessageBoxResult.Yes)
                return;

            var ok = await _apiClient.EliminarFormacionAcademicaAsync(IdFormacion);
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
