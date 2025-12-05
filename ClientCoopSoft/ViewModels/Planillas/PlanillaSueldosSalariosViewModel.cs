using ClientCoopSoft.DTO.Planillas;
using ClientCoopSoft.Models;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using TextAlignment = iText.Layout.Properties.TextAlignment;

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
            if (PlanillaSueldos == null || PlanillaSueldos.Count == 0)
            {
                MessageBox.Show(
                    "No hay datos de planilla para exportar.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Exportar Planilla de Sueldos y Salarios a Excel",
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"Planilla_Sueldos_{Gestion}_{Mes:00}.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Planilla");

                int fila = 1;

                // Título
                ws.Cell(fila, 1).Value = "PLANILLA DE SUELDOS Y SALARIOS";
                ws.Range(fila, 1, fila, 19).Merge();
                ws.Cell(fila, 1).Style
                    .Font.SetBold()
                    .Font.SetFontSize(14);
                ws.Cell(fila, 1).Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                // Subtítulo: Gestión / Mes
                string nombreMes = MesSeleccionado?.Nombre ?? Mes.ToString("00");
                ws.Cell(fila, 1).Value = $"Gestión: {Gestion}   Mes: {nombreMes}";
                ws.Range(fila, 1, fila, 19).Merge();
                ws.Cell(fila, 1).Style
                    .Font.SetFontSize(11);
                ws.Cell(fila, 1).Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila += 2;

                // Encabezados (mismos que el DataGrid)
                int col = 1;
                ws.Cell(fila, col++).Value = "No";
                ws.Cell(fila, col++).Value = "CI";
                ws.Cell(fila, col++).Value = "Apellidos y Nombres";
                ws.Cell(fila, col++).Value = "Nac.";
                ws.Cell(fila, col++).Value = "F. Nac.";
                ws.Cell(fila, col++).Value = "Sexo";
                ws.Cell(fila, col++).Value = "Ocupación";
                ws.Cell(fila, col++).Value = "F. Ingreso";
                ws.Cell(fila, col++).Value = "Días pagados";

                ws.Cell(fila, col++).Value = "Haber Básico";
                ws.Cell(fila, col++).Value = "Bono Antig.";
                ws.Cell(fila, col++).Value = "Bono Prod.";
                ws.Cell(fila, col++).Value = "Aporte Coop 3.34%";
                ws.Cell(fila, col++).Value = "Total Ganado";

                ws.Cell(fila, col++).Value = "Gestora 12.21%";
                ws.Cell(fila, col++).Value = "RC-IVA 13%";
                ws.Cell(fila, col++).Value = "Ap. Solid. 0.5%";
                ws.Cell(fila, col++).Value = "Otros 6.68%";
                ws.Cell(fila, col++).Value = "Otros Desc.";
                ws.Cell(fila, col++).Value = "Total Desc.";
                ws.Cell(fila, col++).Value = "Líquido Pagable";

                // Estilo encabezados
                var headerRange = ws.Range(fila, 1, fila, col - 1);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9E1F2"));
                // Alineación
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                fila++;

                // Filas de datos
                foreach (var item in PlanillaSueldos)
                {
                    col = 1;

                    ws.Cell(fila, col++).Value = item.Id;
                    ws.Cell(fila, col++).Value = item.CarnetIdentidad;
                    ws.Cell(fila, col++).Value = item.ApellidosNombres;
                    ws.Cell(fila, col++).Value = item.Nacionalidad;
                    ws.Cell(fila, col++).Value = item.FechaNacimiento.ToShortDateString();
                    ws.Cell(fila, col++).Value = item.Sexo;
                    ws.Cell(fila, col++).Value = item.Ocupacion;
                    ws.Cell(fila, col++).Value = item.FechaIngreso.ToShortDateString();
                    ws.Cell(fila, col++).Value = item.DiasPagados;

                    ws.Cell(fila, col++).Value = item.HaberBasico;
                    ws.Cell(fila, col++).Value = item.BonoAntiguedad;
                    ws.Cell(fila, col++).Value = item.BonoProduccion;
                    ws.Cell(fila, col++).Value = item.AporteCoop334;
                    ws.Cell(fila, col++).Value = item.TotalGanado;

                    ws.Cell(fila, col++).Value = item.Gestora1221;
                    ws.Cell(fila, col++).Value = item.RcIva13;
                    ws.Cell(fila, col++).Value = item.AporteSolidario05;
                    ws.Cell(fila, col++).Value = item.OtrosDesc668;
                    ws.Cell(fila, col++).Value = item.OtrosDescuentos;
                    ws.Cell(fila, col++).Value = item.TotalDescuentos;
                    ws.Cell(fila, col++).Value = item.LiquidoPagable;

                    fila++;
                }

                // Formato numérico a columnas de montos
                // (ajusta índices si cambias columnas)
                int filaInicioDatos = 5; // donde empiezan los datos (calculado arriba)
                int filaFinDatos = fila - 1;

                // Haber Básico..Líquido Pagable → columnas 10 a 21
                var rangeMontos = ws.Range(filaInicioDatos, 10, filaFinDatos, 21);
                rangeMontos.Style.NumberFormat.Format = "#,##0.00";
                rangeMontos.Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Right);

                // Ajuste de anchos de columna
                ws.Columns().AdjustToContents();

                wb.SaveAs(dlg.FileName);

                MessageBox.Show(
                    "Planilla exportada correctamente a Excel.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar a Excel: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
