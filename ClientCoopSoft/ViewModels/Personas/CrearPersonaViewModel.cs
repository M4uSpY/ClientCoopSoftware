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

        [ObservableProperty] private BitmapImage? imagenHuella1;
        [ObservableProperty] private BitmapImage? imagenHuella2;

        [ObservableProperty] private string? huella1Xml;
        [ObservableProperty] private string? huella2Xml;


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
            var idPersona = await _apiClient.CrearPersonaYObtenerIdAsync(personaDTO);

            if (idPersona is null)
            {
                MessageBox.Show("Error al crear la persona.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool okH1 = true, okH2 = true;

            // Registrar huella 1 si existe
            if (!string.IsNullOrWhiteSpace(Huella1Xml))
                okH1 = await _apiClient.RegistrarHuellaAsync(idPersona.Value, 1, Huella1Xml);

            // Registrar huella 2 si existe
            if (!string.IsNullOrWhiteSpace(Huella2Xml))
                okH2 = await _apiClient.RegistrarHuellaAsync(idPersona.Value, 2, Huella2Xml);

            if (!okH1 || !okH2)
            {
                MessageBox.Show(
                    "La persona se creó, pero hubo problemas al registrar alguna huella.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    "Persona y huellas registradas correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            window.DialogResult = true;
            window.Close();
        }

        [RelayCommand]
        private async Task CapturarHuella1Async()
        {
            var resultado = await _lectorHuellaService.CapturarHuellaAsync();
            if (resultado != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImagenHuella1 = resultado.ImagenHuella;
                });

                Huella1Xml = resultado.TemplateXml;
                MessageBox.Show("Huella 1 capturada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task CapturarHuella2Async()
        {
            var resultado = await _lectorHuellaService.CapturarHuellaAsync();
            if (resultado != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImagenHuella2 = resultado.ImagenHuella;
                });

                Huella2Xml = resultado.TemplateXml;
                MessageBox.Show("Huella 2 capturada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
