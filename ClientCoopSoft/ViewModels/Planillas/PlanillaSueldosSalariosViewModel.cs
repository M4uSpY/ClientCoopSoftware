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
        // NUEVO: método para refrescar resumen (gestión, mes, estado)
        private async Task ActualizarResumenPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
                return;

            var resumen = await _api.ObtenerResumenPlanillaSueldosAsync(IdPlanillaActual);
            if (resumen != null)
            {
                Gestion = resumen.Gestion;
                Mes = resumen.Mes;
                EstaCerrada = resumen.EstaCerrada;
            }
        }

        // Crear nueva planilla (para gestión/mes), generar trabajadores, calcular y cargar
        [RelayCommand]
        private async Task CrearYCalcularPlanillaAsync()
        {
            try
            {
                // 1) Crear encabezado
                var resumen = await _api.CrearPlanillaSueldosAsync(Gestion, Mes);
                if (resumen == null)
                {
                    MessageBox.Show(
                        "No se pudo crear la planilla de Sueldos y Salarios.\n\n" +
                        "Verifique la gestión y mes, y que no exista ya una planilla para ese periodo.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                IdPlanillaActual = resumen.IdPlanilla;
                EstaCerrada = resumen.EstaCerrada;

                // 2) Generar Trabajador_Planilla
                var genOk = await _api.GenerarTrabajadoresPlanillaAsync(IdPlanillaActual);
                if (!genOk)
                {
                    MessageBox.Show(
                        "No se pudieron generar las filas de trabajadores para la planilla.\n" +
                        "Revise que existan trabajadores activos.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3) Calcular planilla
                var calcOk = await _api.CalcularPlanillaSueldosAsync(IdPlanillaActual);
                if (!calcOk)
                {
                    MessageBox.Show(
                        "No se pudo calcular la planilla.\n" +
                        "Verifique que existan conceptos configurados en el sistema.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 4) Actualizar resumen y cargar filas
                await ActualizarResumenPlanillaAsync();
                await CargarPlanillaAsync();

                MessageBox.Show(
                    $"Planilla {IdPlanillaActual} creada y calculada correctamente.\n\n" +
                    "Revise los montos en la tabla inferior.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear/cargar la planilla: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task CargarPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show(
                    "Indique un Id de planilla válido.\n\n" +
                    "Ejemplo: primero cree una planilla con 'Crear y calcular' " +
                    "y anote el Id que se muestra.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lista = await _api.ObtenerDatosPlanillaSueldosAsync(IdPlanillaActual);
                PlanillaSueldos = new ObservableCollection<PlanillaSueldosFilaModel>(lista ?? new());

                // NUEVO: refrescar gestión/mes/estado
                await ActualizarResumenPlanillaAsync();

                if (PlanillaSueldos.Count == 0)
                {
                    MessageBox.Show(
                        "La planilla existe pero no tiene trabajadores asociados.\n" +
                        "Ejecute 'Crear y calcular' nuevamente o revise el backend.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la planilla: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Recalcular planilla (por si cambias manuales en otra pantalla)
        [RelayCommand]
        private async Task RecalcularPlanillaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show(
                    "No hay una planilla seleccionada.\n\n" +
                    "Cargue primero una planilla (Id Planilla) o cree una nueva.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show(
                    "No hay una planilla seleccionada para cerrar.\n\n" +
                    "Cargue primero la planilla que desea cerrar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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
