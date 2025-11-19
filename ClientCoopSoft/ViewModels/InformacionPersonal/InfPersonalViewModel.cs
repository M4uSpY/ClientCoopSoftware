using ClientCoopSoft.Models;
using ClientCoopSoft.ViewModels.Capacitaciones;
using ClientCoopSoft.ViewModels.Contratacion;
using ClientCoopSoft.ViewModels.FormacionAcademica;
using ClientCoopSoft.Views.Capacitaciones;
using ClientCoopSoft.Views.Contrato;
using ClientCoopSoft.Views.FormacionAcademica;
using ClientCoopSoft.Views.InformacionPersonal;
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

namespace ClientCoopSoft.ViewModels.InformacionPersonal
{
    public partial class InfPersonalViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Persona _persona;

        // Campos existentes
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
        [ObservableProperty] private string huella = string.Empty;

        // NUEVO: contenido dinámico para navegación de pestañas
        [ObservableProperty] private UserControl? contenidoActual;

        [ObservableProperty]
        private bool mostrarEncabezado;


        public InfPersonalViewModel(Persona persona, ApiClient apiCilent)
        {
            _apiClient = apiCilent;
            _persona = persona;

            // Inicializar campos
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

            // Inicializar contenido con la vista principal
            ContenidoActual = new InformacionPersonalFormView { DataContext = this };
            MostrarEncabezado = true;
        }

        // Métodos existentes
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
        private async Task GuardarAsync(Window window)
        {
            // Validaciones existentes...
            if (GeneroSeleccionado is null || NacionalidadSeleccionada is null ||
                string.IsNullOrWhiteSpace(PrimerNombre) || string.IsNullOrWhiteSpace(SegundoNombre) ||
                string.IsNullOrWhiteSpace(ApellidoPaterno) || string.IsNullOrWhiteSpace(ApellidoMaterno) ||
                string.IsNullOrWhiteSpace(CarnetIdentidad) || string.IsNullOrWhiteSpace(Telefono) ||
                string.IsNullOrWhiteSpace(Direccion) || string.IsNullOrWhiteSpace(Email) ||
                FechaNacimiento > DateTime.Today)
            {
                MessageBox.Show("Por favor completa todos los campos correctamente.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personaDTO = new Persona
            {
                IdNacionalidad = NacionalidadSeleccionada!.IdClasificador,
                PrimerNombre = PrimerNombre,
                SegundoNombre = string.IsNullOrWhiteSpace(SegundoNombre) ? null : SegundoNombre,
                ApellidoPaterno = ApellidoPaterno,
                ApellidoMaterno = ApellidoMaterno,
                Genero = GeneroSeleccionado!.Value,
                CarnetIdentidad = CarnetIdentidad,
                FechaNacimiento = FechaNacimiento,
                Telefono = Telefono,
                Direccion = Direccion,
                Email = Email,
                Foto = FotoBytes,
            };

            bool exito = await _apiClient.EditarPersonaAsync(_persona.IdPersona, personaDTO);
            MessageBox.Show(exito ? "Informacion personal actualizada correctamente" : "Error al actualizar la informacion personal",
                            exito ? "Éxito" : "Error", MessageBoxButton.OK,
                            exito ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        [RelayCommand]
        private void Cancelar(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }





        // NUEVOS: comandos de navegación
        [RelayCommand]
        private void MostrarInformacionPersonal()
        {
            ContenidoActual = new InformacionPersonalFormView { DataContext = this };
            MostrarEncabezado = true;
        }

        [RelayCommand]
        private void MostrarFormacionAcademica()
        {
            if(_persona.Trabajador != null)
            {
                var vm = new FormacionAcademicaViewModel(_persona.Trabajador.IdTrabajador, _apiClient);

                ContenidoActual = new FormacionAcademicaView
                {
                    DataContext = vm
                };
                MostrarEncabezado = false;
            }
            return;
        }

        [RelayCommand]
        private void MostrarCapacitaciones()
        {
            if (_persona.Trabajador != null)
            {
                var vm = new CapacitacionResumenViewModel(_persona.Trabajador.IdTrabajador, _apiClient);

                ContenidoActual = new CapacitacionResumenView
                {
                    DataContext = vm
                };
                MostrarEncabezado = false;
            }
            return;
        }
        [RelayCommand]
        private void MostrarContrato()
        {
            if (_persona.Trabajador != null)
            {
                var vm = new ContratoViewModel(_persona.Trabajador.IdTrabajador, _apiClient);

                ContenidoActual = new ContratoView
                {
                    DataContext = vm
                };
                MostrarEncabezado = false;
            }
            return;
        }
    }
}
