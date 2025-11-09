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

        [ObservableProperty] private string? huellaBase64;
        [ObservableProperty] private bool huellaRegistrada;
        [ObservableProperty] private BitmapImage? huellaPreview;

        [ObservableProperty] private BitmapImage? imagenHuella;
        [ObservableProperty] private byte[]? huellaBytes;

        [ObservableProperty] private string mensajeHuella = "No hay huella registrada";

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
            HuellaBytes = persona.Huella;

            _ = CargarGenerosAsync(persona.Genero);
            _ = CargarNacionalidadesAsync(persona.IdNacionalidad);
            _ = CargarHuellaAsync();

        }
        private async Task CargarHuellaAsync()
        {
            try
            {
                var huellaBytes = await _apiClient.ObtenerHuellaAsync(_persona.IdPersona);
                if (huellaBytes != null && huellaBytes.Length > 0)
                {
                    HuellaBase64 = Convert.ToBase64String(huellaBytes);
                    HuellaRegistrada = true;
                    MensajeHuella = "Huella registrada ✔";
                }
                else
                {
                    HuellaRegistrada = false;
                    MensajeHuella = "No hay huella registrada";
                }
            }
            catch (Exception ex)
            {
                HuellaRegistrada = false;
                MensajeHuella = $"Error al cargar huella: {ex.Message}";
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
            if (string.IsNullOrWhiteSpace(SegundoNombre))
            {
                MessageBox.Show("Ingresa el segundo nombre", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                Huella = HuellaBytes
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
        private async Task CapturarHuellaAsync()
        {
            var resultado = await _lectorHuellaService.CapturarHuellaAsync();
            if (resultado != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImagenHuella = resultado.ImagenHuella; // Preview
                });

                HuellaBytes = resultado.TemplateBytes; // <-- enviar byte[] directo al backend
                MessageBox.Show("Huella capturada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
