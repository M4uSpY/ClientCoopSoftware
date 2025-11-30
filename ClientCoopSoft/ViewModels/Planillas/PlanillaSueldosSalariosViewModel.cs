using ClientCoopSoft.DTO.Planillas;
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
            var hoy = DateTime.Now;
            Gestion = DateTime.Now.Year;
            Mes = DateTime.Now.Month;

            Anios = new ObservableCollection<int>();
            for (int y = hoy.Year - 3; y <= hoy.Year + 2; y++)
                Anios.Add(y);

            // ====== Meses disponibles ======
            Meses = new ObservableCollection<MesItemModel>
            {
                new MesItemModel { Numero = 1,  Nombre = "ENERO" },
                new MesItemModel { Numero = 2,  Nombre = "FEBRERO" },
                new MesItemModel { Numero = 3,  Nombre = "MARZO" },
                new MesItemModel { Numero = 4,  Nombre = "ABRIL" },
                new MesItemModel { Numero = 5,  Nombre = "MAYO" },
                new MesItemModel { Numero = 6,  Nombre = "JUNIO" },
                new MesItemModel { Numero = 7,  Nombre = "JULIO" },
                new MesItemModel { Numero = 8,  Nombre = "AGOSTO" },
                new MesItemModel { Numero = 9,  Nombre = "SEPTIEMBRE" },
                new MesItemModel { Numero = 10, Nombre = "OCTUBRE" },
                new MesItemModel { Numero = 11, Nombre = "NOVIEMBRE" },
                new MesItemModel { Numero = 12, Nombre = "DICIEMBRE" }
            };

            // Selecciones iniciales
            AnioSeleccionado = Gestion;
            MesSeleccionado = Meses.First(m => m.Numero == Mes);


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

        [ObservableProperty]
        private ObservableCollection<int> anios = new();

        [ObservableProperty]
        private int anioSeleccionado;

        [ObservableProperty]
        private ObservableCollection<MesItemModel> meses = new();

        [ObservableProperty]
        private MesItemModel? mesSeleccionado;

        // Cuando el usuario cambia el año del ComboBox → actualizamos Gestión
        partial void OnAnioSeleccionadoChanged(int value)
        {
            Gestion = value;
        }

        // Cuando cambia el mes del ComboBox → actualizamos Mes (1..12)
        partial void OnMesSeleccionadoChanged(MesItemModel? value)
        {
            if (value != null)
                Mes = value.Numero;
        }

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
            try
            {
                // 1) Siempre buscamos primero por Gestión + Mes seleccionados
                var resumen = await _api.BuscarPlanillaSueldosPorPeriodoAsync(Gestion, Mes);

                if (resumen == null)
                {
                    // No hay planilla para ese periodo → limpiamos todo
                    IdPlanillaActual = 0;
                    EstaCerrada = false;
                    PlanillaSueldos = new ObservableCollection<PlanillaSueldosFilaModel>();

                    MessageBox.Show(
                        $"No se encontró ninguna planilla de Sueldos y Salarios para " +
                        $"la gestión {Gestion} y mes {Mes}.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2) Actualizamos IdPlanillaActual y estado con el resultado del backend
                IdPlanillaActual = resumen.IdPlanilla;
                EstaCerrada = resumen.EstaCerrada;

                // 3) Cargamos filas de la planilla encontrada
                var lista = await _api.ObtenerDatosPlanillaSueldosAsync(IdPlanillaActual);
                PlanillaSueldos = new ObservableCollection<PlanillaSueldosFilaModel>(lista ?? new());

                // 4) (Opcional) refrescar resumen por si cambió algo en el backend
                await ActualizarResumenPlanillaAsync();

                if (PlanillaSueldos.Count == 0)
                {
                    MessageBox.Show(
                        "La planilla existe pero no tiene trabajadores asociados.\n" +
                        "Ejecute 'Crear y calcular' o revise el backend.",
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

        [RelayCommand]
        private async Task GuardarRcIvaAsync()
        {
            if (IdPlanillaActual <= 0)
            {
                MessageBox.Show(
                    "No hay una planilla seleccionada.\n\n" +
                    "Cargue primero una planilla o cree una nueva.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PlanillaSueldos == null || PlanillaSueldos.Count == 0)
            {
                MessageBox.Show(
                    "No hay filas de planilla para actualizar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int errores = 0;

                foreach (var fila in PlanillaSueldos)
                {
                    // puedes aplicar validaciones aquí (por ejemplo RC-IVA >= 0)
                    if (fila.IdTrabajadorPlanilla <= 0)
                        continue;

                    var ok = await _api.ActualizarRcIvaAsync(
                        fila.IdTrabajadorPlanilla,
                        fila.RcIva13);

                    if (!ok)
                        errores++;
                }

                // Recalcular planilla con los nuevos RC-IVA manuales
                var recalculoOk = await _api.CalcularPlanillaSueldosAsync(IdPlanillaActual);
                if (!recalculoOk)
                {
                    MessageBox.Show(
                        "Se guardó RC-IVA pero no se pudo recalcular la planilla.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    await CargarPlanillaAsync();
                }

                if (errores == 0)
                {
                    MessageBox.Show(
                        "RC-IVA guardado correctamente y planilla recalculada.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Se presentaron errores en {errores} fila(s) al guardar RC-IVA.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar RC-IVA: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
