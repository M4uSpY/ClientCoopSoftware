using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Planillas
{
    public partial class PlanillaSueldosSalariosViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public PlanillaSueldosSalariosViewModel(ApiClient api)
        {
            _api = api;

            Gestion = DateTime.Now.Year;
            Mes = DateTime.Now.Month;
            PlanillaSueldos = new ObservableCollection<PlanillaSueldosFilaModel>();
        }

        // =======================
        // PROPIEDADES
        // =======================

        [ObservableProperty]
        private int gestion;

        [ObservableProperty]
        private int mes;

        [ObservableProperty]
        private int idPlanillaActual;

        [ObservableProperty]
        private bool estaCerrada;

        [ObservableProperty]
        private ObservableCollection<PlanillaSueldosFilaModel> planillaSueldos;

        // =======================
        // COMANDOS
        // =======================

        // Crear nueva planilla (para gestión/mes), generar trabajadores, calcular y cargar
        [RelayCommand]
        private async Task CrearYCalcularPlanillaAsync()
        {
            try
            {
                // 1) Crear encabezado de planilla
                var resumen = await _api.CrearPlanillaSueldosAsync(Gestion, Mes);
                if (resumen == null)
                {
                    MessageBox.Show("No se pudo crear la planilla de sueldos y salarios.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                IdPlanillaActual = resumen.IdPlanilla;
                EstaCerrada = resumen.EstaCerrada;

                // 2) Generar Trabajador_Planilla
                var genOk = await _api.GenerarTrabajadoresPlanillaAsync(IdPlanillaActual);
                if (!genOk)
                {
                    MessageBox.Show("No se pudieron generar los trabajadores para la planilla.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3) Calcular planilla
                var calcOk = await _api.CalcularPlanillaSueldosAsync(IdPlanillaActual);
                if (!calcOk)
                {
                    MessageBox.Show("No se pudo calcular la planilla.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 4) Cargar filas en la grilla
                await CargarPlanillaAsync();

                MessageBox.Show($"Planilla {IdPlanillaActual} creada y calculada correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear/cargar la planilla: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cargar una planilla existente por IdPlanillaActual
        [RelayCommand]
        private async Task CargarPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show("Indique un Id de planilla válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lista = await _api.ObtenerDatosPlanillaSueldosAsync(IdPlanillaActual);
                PlanillaSueldos = new ObservableCollection<PlanillaSueldosFilaModel>(lista ?? new());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la planilla: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Recalcular planilla (por si cambias manuales en otra pantalla)
        [RelayCommand]
        private async Task RecalcularPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show("No hay una planilla seleccionada.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ok = await _api.CalcularPlanillaSueldosAsync(IdPlanillaActual);
                if (!ok)
                {
                    MessageBox.Show("No se pudo recalcular la planilla.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await CargarPlanillaAsync();
                MessageBox.Show("Planilla recalculada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recalcular la planilla: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cerrar planilla (no más modificaciones)
        [RelayCommand]
        private async Task CerrarPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show("No hay una planilla seleccionada.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ok = await _api.CerrarPlanillaSueldosAsync(IdPlanillaActual);
                if (!ok)
                {
                    MessageBox.Show("No se pudo cerrar la planilla.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EstaCerrada = true;
                MessageBox.Show("Planilla cerrada correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar la planilla: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            // TODO: implementación real de exportación
            MessageBox.Show("Exportar a Excel aún no está implementado.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ExportarPdf()
        {
            // TODO: implementación real de exportación
            MessageBox.Show("Exportar a PDF aún no está implementado.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
