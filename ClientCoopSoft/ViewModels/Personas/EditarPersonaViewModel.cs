using ClientCoopSoft.DTO.Huellas;
using ClientCoopSoft.DTO.Personas;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels.Personas
{
    public partial class EditarPersonaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Persona _persona;
        private readonly LectorHuellaService _lectorHuellaService = new();

        [ObservableProperty] private string primerNombre = string.Empty;
        [ObservableProperty] private string? segundoNombre;
        [ObservableProperty] private string apellidoPaterno = string.Empty;
        [ObservableProperty] private string apellidoMaterno = string.Empty;
        [ObservableProperty] private ObservableCollection<bool> generos = new();
        [ObservableProperty] private bool? generoSeleccionado;
        [ObservableProperty] private ObservableCollection<Nacionalidad> nacionalidades = new();
        [ObservableProperty] private Nacionalidad? nacionalidadSeleccionada;
        [ObservableProperty] private string carnetIdentidad = string.Empty;
        [ObservableProperty] private DateTime fechaNacimiento = DateTime.Today;
        [ObservableProperty] private string telefono = string.Empty;
        [ObservableProperty] private string direccion = string.Empty;
        [ObservableProperty] private string email = string.Empty;

        [ObservableProperty] private byte[]? fotoBytes;
        [ObservableProperty] private BitmapImage? fotoPreview;

        [ObservableProperty] private BitmapImage? imagenHuella1;
        [ObservableProperty] private BitmapImage? imagenHuella2;

        [ObservableProperty] private string huella1Xml = string.Empty;
        [ObservableProperty] private string huella2Xml = string.Empty;

        [ObservableProperty] private bool huella1Registrada;
        [ObservableProperty] private bool huella2Registrada;

        [ObservableProperty] private string mensajeHuella1 = "Huella 1 no registrada";
        [ObservableProperty] private string mensajeHuella2 = "Huella 2 no registrada";


        public EditarPersonaViewModel(Persona persona, ApiClient apiCilent)
        {
            _apiClient = apiCilent;
            _persona = persona;

            PrimerNombre = persona.PrimerNombre;
            SegundoNombre = persona.SegundoNombre ?? string.Empty;
            ApellidoPaterno = persona.ApellidoPaterno;
            ApellidoMaterno = persona.ApellidoMaterno;
            CarnetIdentidad = persona.CarnetIdentidad;
            FechaNacimiento = persona.FechaNacimiento;
            Telefono = persona.Telefono;
            Direccion = persona.Direccion;
            Email = persona.Email;
            FotoBytes = persona.Foto;

            _ = CargarGenerosAsync(persona.Genero);
            _ = CargarNacionalidadesAsync(persona.IdNacionalidad);
            _ = CargarHuellasAsync();

        }
        private async Task CargarHuellasAsync()
        {
            try
            {
                var huellas = await _apiClient.ObtenerHuellasPersonaAsync(_persona.IdPersona)
                              ?? new List<HuellaRespuestaDTO>();

                var h1 = huellas.FirstOrDefault(h => h.IndiceDedo == 1);
                if (h1 != null)
                {
                    Huella1Xml = h1.TemplateXml;
                    Huella1Registrada = true;
                    MensajeHuella1 = "Huella 1 registrada ✔";
                }
                else
                {
                    Huella1Registrada = false;
                    MensajeHuella1 = "Huella 1 no registrada";
                }

                var h2 = huellas.FirstOrDefault(h => h.IndiceDedo == 2);
                if (h2 != null)
                {
                    Huella2Xml = h2.TemplateXml;
                    Huella2Registrada = true;
                    MensajeHuella2 = "Huella 2 registrada ✔";
                }
                else
                {
                    Huella2Registrada = false;
                    MensajeHuella2 = "Huella 2 no registrada";
                }
            }
            catch (Exception ex)
            {
                Huella1Registrada = Huella2Registrada = false;
                MensajeHuella1 = MensajeHuella2 = $"Error al cargar huellas: {ex.Message}";
            }
        }


        private async Task CargarGenerosAsync(bool genero)
        {
            var listaGeneros = await _apiClient.ObtenerGenerosAsync() ?? new List<bool>();
            Generos = new ObservableCollection<bool>(listaGeneros);
            GeneroSeleccionado = Generos.FirstOrDefault(g => g.Equals(genero));
        }
        private async Task CargarNacionalidadesAsync(int idNacionalidad)
        {
            var listaNacionalidades = await _apiClient.ObtenerNacionalidadesAsync() ?? new List<Nacionalidad>();
            Nacionalidades = new ObservableCollection<Nacionalidad>(listaNacionalidades);
            NacionalidadSeleccionada = Nacionalidades.FirstOrDefault(n => n.IdClasificador == idNacionalidad);
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

                    var fileInfo = new FileInfo(filePath);
                    const long maxBytes = 2 * 1024 * 1024; // 2 MB
                    if (fileInfo.Length > maxBytes)
                    {
                        MessageBox.Show(
                            "La foto no puede ser mayor a 2 MB.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    FotoBytes = File.ReadAllBytes(filePath);

                    // Crear previsualización
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
        private async Task GuardarAsync(Window window)
        {
            if (GeneroSeleccionado is null)
            {
                MessageBox.Show("Selecciona una persona.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NacionalidadSeleccionada is null)
            {
                MessageBox.Show("Selecciona una nacionalidad.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(PrimerNombre))
            {
                MessageBox.Show("Ingresa el primer nombre", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ApellidoPaterno))
            {
                MessageBox.Show("Ingresa el apellido paterno", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(ApellidoMaterno))
            {
                MessageBox.Show("Ingresa el apellido materno", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(CarnetIdentidad))
            {
                MessageBox.Show("Ingresa el carnet de identidad", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (FechaNacimiento > DateTime.Today)
            {
                MessageBox.Show("La fecha de nacimiento no puede ser futura.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Telefono))
            {
                MessageBox.Show("Ingresa el telefono", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Direccion))
            {
                MessageBox.Show("Ingresa la direccion", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Ingresa el email", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personaDTO = new Persona
            {
                IdNacionalidad = NacionalidadSeleccionada.IdClasificador,
                PrimerNombre = PrimerNombre,
                SegundoNombre = string.IsNullOrWhiteSpace(SegundoNombre) ? null : SegundoNombre,
                ApellidoPaterno = ApellidoPaterno,
                ApellidoMaterno = ApellidoMaterno,
                Genero = GeneroSeleccionado.Value,
                CarnetIdentidad = CarnetIdentidad,
                FechaNacimiento = FechaNacimiento,
                Telefono = Telefono,
                Direccion = Direccion,
                Email = Email,
                Foto = FotoBytes,
            };
            bool exito = await _apiClient.EditarPersonaAsync(_persona.IdPersona, personaDTO);
            if (exito)
            {
                MessageBox.Show("Usuario actualizado correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Error al actualizar el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        [RelayCommand]
        private async Task CapturarHuella1Async()
        {
            var resultado = await _lectorHuellaService.CapturarHuellaAsync();
            if (resultado != null)
            {
                if (string.IsNullOrWhiteSpace(resultado.TemplateXml))
                {
                    MessageBox.Show("No se obtuvo un template de huella válido.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImagenHuella1 = resultado.ImagenHuella;
                });

                Huella1Xml = resultado.TemplateXml;

                var ok = await _apiClient.RegistrarHuellaAsync(_persona.IdPersona, 1, Huella1Xml);

                if (ok)
                {
                    Huella1Registrada = true;
                    MensajeHuella1 = "Huella 1 actualizada ✔";
                }
                else
                {
                    MessageBox.Show("Error al registrar/actualizar la Huella 1.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task CapturarHuella2Async()
        {
            var resultado = await _lectorHuellaService.CapturarHuellaAsync();
            if (resultado != null)
            {
                if (string.IsNullOrWhiteSpace(resultado.TemplateXml))
                {
                    MessageBox.Show("No se obtuvo un template de huella válido.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImagenHuella2 = resultado.ImagenHuella;
                });

                Huella2Xml = resultado.TemplateXml;

                var ok = await _apiClient.RegistrarHuellaAsync(_persona.IdPersona, 2, Huella2Xml);

                if (ok)
                {
                    Huella2Registrada = true;
                    MensajeHuella2 = "Huella 2 actualizada ✔";
                }
                else
                {
                    MessageBox.Show("Error al registrar/actualizar la Huella 2.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
