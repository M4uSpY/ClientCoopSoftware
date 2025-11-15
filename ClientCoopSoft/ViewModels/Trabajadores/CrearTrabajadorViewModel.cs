using ClientCoopSoft.DTO.Horarios;
using ClientCoopSoft.DTO.Trabajadores;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Trabajadores
{
    public partial class CrearTrabajadorViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty] private ObservableCollection<Persona> personas = new();
        [ObservableProperty] private Persona? personaSeleccionada;
        [ObservableProperty] private decimal haberBasico;
        [ObservableProperty] private DateTime fechaIngreso;
        [ObservableProperty] private ObservableCollection<Cargo> cargos = new();
        [ObservableProperty] private Cargo? cargoSeleccionado;
        [ObservableProperty] private ObservableCollection<HorarioDia> horarios = new();

        public CrearTrabajadorViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            FechaIngreso = DateTime.Today;
            InicializarHorarios();
            _ = CargarCombosAsync();
        }

        private void InicializarHorarios()
        {
            Horarios = new ObservableCollection<HorarioDia>
            {
                new() { DiaSemana = "Lunes"},
                new() { DiaSemana = "Martes"},
                new() { DiaSemana = "Miercoles"},
                new() { DiaSemana = "Jueves"},
                new() { DiaSemana = "Viernes"},
            };
        }
        private async Task CargarCombosAsync()
        {
            var personasApi = await _apiClient.ObtenerPersonasAsync();
            if(personasApi != null)
            {
                Personas = new ObservableCollection<Persona>(personasApi);
            }

            var cargosApi = await _apiClient.ObtenerCargosAsync();
            if(cargosApi != null)
            {
                Cargos = new ObservableCollection<Cargo>(cargosApi);
            }
        }

        [RelayCommand]
        private async Task GuardarAsync(Window window)
        {
            if (PersonaSeleccionada is null)
            {
                MessageBox.Show("Selecciona una persona.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CargoSeleccionado is null)
            {
                MessageBox.Show("Selecciona un cargo.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Opcional: podrías validar que al menos una fila tenga horas
            var horariosValidos = Horarios
                .Where(h => !string.IsNullOrWhiteSpace(h.HoraEntrada)
                         && !string.IsNullOrWhiteSpace(h.HoraSalida))
                .ToList();

            if (!horariosValidos.Any())
            {
                MessageBox.Show("Ingresa al menos un horario de entrada y salida.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new TrabajadorCrearDTO
            {
                IdPersona = PersonaSeleccionada.IdPersona,
                HaberBasico = HaberBasico,
                FechaIngreso = FechaIngreso,
                IdCargo = CargoSeleccionado.IdCargo,
                Horarios = horariosValidos.Select(h => new HorarioCrearDTO
                {
                    DiaSemana = h.DiaSemana,
                    HoraEntrada = h.HoraEntrada,
                    HoraSalida = h.HoraSalida
                }).ToList()
            };
            var ok = await _apiClient.CrearTrabajadorAsync(dto);

            if (ok)
            {
                MessageBox.Show("Trabajador creado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("No se pudo crear el trabajador.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
