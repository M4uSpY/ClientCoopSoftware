using ClientCoopSoft.DTO.Horarios;
using ClientCoopSoft.DTO.Trabajadores;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Trabajadores
{
    public partial class EditarTrabajadorViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Trabajador _trabajador;   // <-- modelo original (como Usuario en tu ejemplo)

        [ObservableProperty] private ObservableCollection<Persona> personas = new();
        [ObservableProperty] private Persona? personaSeleccionada;

        [ObservableProperty] private decimal haberBasico;
        [ObservableProperty] private DateTime fechaIngreso;

        [ObservableProperty] private ObservableCollection<Cargo> cargos = new();
        [ObservableProperty] private Cargo? cargoSeleccionado;

        [ObservableProperty] private ObservableCollection<HorarioDia> horarios = new();

        // Constructor igual estilo que EditarUsuario
        public EditarTrabajadorViewModel(Trabajador trabajador, ApiClient apiClient)
        {
            _trabajador = trabajador;
            _apiClient = apiClient;

            HaberBasico = trabajador.HaberBasico;      
            FechaIngreso = trabajador.FechaIngreso;    

            InicializarHorariosDesdeModelo(trabajador.Horarios);

            _ = CargarPersonasAsync(trabajador.IdPersona);  // <-- asumiendo que Trabajador tiene IdPersona
            _ = CargarCargosAsync(trabajador.Cargo);        // usamos el nombre del cargo actual
        }

        private async Task CargarPersonasAsync(int idPersona)
        {
            var lista = await _apiClient.ObtenerPersonasAsync() ?? new List<Persona>();
            Personas = new ObservableCollection<Persona>(lista);
            PersonaSeleccionada = Personas.FirstOrDefault(p => p.IdPersona == idPersona);
        }

        private async Task CargarCargosAsync(string nombreCargoActual)
        {
            var lista = await _apiClient.ObtenerCargosAsync() ?? new List<Cargo>();
            Cargos = new ObservableCollection<Cargo>(lista);
            CargoSeleccionado = Cargos
                .FirstOrDefault(c => c.NombreCargo.Equals(nombreCargoActual, StringComparison.OrdinalIgnoreCase));
        }

        private void InicializarHorariosDesdeModelo(List<HorarioDTO> horariosModelo)
        {
            Horarios.Clear();

            if (horariosModelo != null && horariosModelo.Any())
            {
                foreach (var h in horariosModelo)
                {
                    Horarios.Add(new HorarioDia
                    {
                        DiaSemana = h.DiaSemana,
                        HoraEntrada = h.HoraEntrada.ToString(@"hh\:mm"),
                        HoraSalida = h.HoraSalida.ToString(@"hh\:mm")
                    });
                }
            }
            else
            {
                // Si viniera vacío, ponemos lunes a viernes sin horas:
                Horarios = new ObservableCollection<HorarioDia>
                {
                    new() { DiaSemana = "Lunes"     },
                    new() { DiaSemana = "Martes"    },
                    new() { DiaSemana = "Miércoles" },
                    new() { DiaSemana = "Jueves"    },
                    new() { DiaSemana = "Viernes"   }
                };
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

            // (Opcional) Validar formato HH:mm
            foreach (var h in horariosValidos)
            {
                if (!TimeSpan.TryParse(h.HoraEntrada, out _) ||
                    !TimeSpan.TryParse(h.HoraSalida, out _))
                {
                    MessageBox.Show($"Formato de hora inválido en el día {h.DiaSemana}. Usa HH:mm",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var dto = new TrabajadorEditarDTO
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

            bool ok = await _apiClient.EditarTrabajadorAsync(_trabajador.IdTrabajador, dto);

            if (ok)
            {
                MessageBox.Show("Trabajador actualizado correctamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Error al actualizar el trabajador", "Error",
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
