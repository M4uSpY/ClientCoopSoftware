using ClientCoopSoft.DTO.Personas;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.ViewModels.Personas
{
    public partial class CrearPersonaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
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

        [ObservableProperty] private BitmapImage? imagenHuella;
        [ObservableProperty] private string? huellaXml;

        public CrearPersonaViewModel(ApiClient apiCilent)
        {
            _apiClient = apiCilent;
            _ = CargarGenerosAsync();
            _ = CargarNacionalidadesAsync();
        }

        private async Task CargarGenerosAsync()
        {
            var listaGeneros = await _apiClient.ObtenerGenerosAsync() ?? new List<bool>();
            Generos = new ObservableCollection<bool>(listaGeneros);
        }
        private async Task CargarNacionalidadesAsync()
        {
            var listaNacionalidades = await _apiClient.ObtenerNacionalidadesAsync() ?? new List<Nacionalidad>();
            Nacionalidades = new ObservableCollection<Nacionalidad>(listaNacionalidades);
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
            if(GeneroSeleccionado is null)
            {
                MessageBox.Show("Selecciona una persona.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(NacionalidadSeleccionada is null)
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

            var personaDTO = new PersonaCrearDTO
            {
                IdNacionalidad = NacionalidadSeleccionada.IdClasificador,
                PrimerNombre = PrimerNombre,
                SegundoNombre = SegundoNombre,
                ApellidoPaterno = ApellidoMaterno,
                ApellidoMaterno = ApellidoMaterno,
                Genero = GeneroSeleccionado.Value,
                CarnetIdentidad = CarnetIdentidad,
                FechaNacimiento = FechaNacimiento,
                Telefono = Telefono,
                Direccion = Direccion,
                Email = Email,
                Foto = FotoBytes,
                Huella = HuellaXml
            };
            bool exito = await _apiClient.CrearPersonaAsync(personaDTO);
            if (exito)
            {
                MessageBox.Show("Usuario creado correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Error al crear el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                // Antes: HuellaBytes = resultado.TemplateBytes;
                HuellaXml = resultado.TemplateXml;

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
