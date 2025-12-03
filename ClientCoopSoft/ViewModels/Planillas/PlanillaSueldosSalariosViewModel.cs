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
                int colMax = 22;

                string nombreMes = MesSeleccionado?.Nombre ?? Mes.ToString("00");

                // =========================
                // LOGO
                // =========================
                string logoPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "Logo_Cooperativa.png"
                );

                if (File.Exists(logoPath))
                {
                    var picture = ws.AddPicture(logoPath)
                                    .MoveTo(ws.Cell(2, 1)) // un poco más abajo
                                    .Scale(0.35);         // ajusta si quieres más grande/chico
                }

                // Dejamos la fila 1 vacía (solo estética)
                fila = 2;

                // =========================
                // ENCABEZADO SUPERIOR
                // =========================

                // Nombre de la cooperativa (centrado, un poco más a la derecha)
                ws.Cell(fila, 4).Value =
                    "COOPERATIVA DE AHORRO Y CREDITO DE VINCULO LABORAL \"LA CONFIANZA\" R.L.";
                ws.Range(fila, 4, fila, 18).Merge();
                ws.Cell(fila, 4).Style.Font.SetBold().Font.SetFontSize(14);
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                // Fila: No. EMPLEADOR y PAG. 1 DE 1
                ws.Cell(fila, 4).Value = "No. EMPLEADOR MINISTERIO DE TRABAJO:";
                ws.Range(fila, 4, fila, 12).Merge();
                ws.Cell(fila, 13).Value = "215110027-1";   // puedes sacar de config/bd
                ws.Range(fila, 13, fila, 16).Merge();

                ws.Cell(fila, 19).Value = "PAG. 1 DE 1";
                ws.Range(fila, 19, fila, colMax).Merge();
                ws.Cell(fila, 19).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                fila++;

                // Fila: NIT
                ws.Cell(fila, 4).Value = "No. NIT:";
                ws.Range(fila, 4, fila, 12).Merge();
                ws.Cell(fila, 13).Value = "215110027";    // idem, de config
                ws.Range(fila, 13, fila, 16).Merge();
                fila++;

                // Fila: Empleador Caja de Salud
                ws.Cell(fila, 4).Value = "No. de EMPLEADOR CAJA DE SALUD:";
                ws.Range(fila, 4, fila, 12).Merge();
                ws.Cell(fila, 13).Value = "710-1-1988";
                ws.Range(fila, 13, fila, 16).Merge();
                fila++;

                // Fila: Corresponde al mes... (arriba a la derecha, como en la segunda imagen)
                ws.Cell(fila, 16).Value = $"CORRESPONDE AL MES DE {nombreMes} {Gestion}";
                ws.Range(fila, 16, fila, colMax).Merge();
                ws.Cell(fila, 16).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                fila += 2;

                // Título central
                ws.Cell(fila, 4).Value = "PLANILLA DE SUELDOS Y SALARIOS";
                ws.Range(fila, 4, fila, 18).Merge();
                ws.Cell(fila, 4).Style.Font.SetBold().Font.SetFontSize(13);
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                ws.Cell(fila, 4).Value = "PERSONAL PERMANENTE";
                ws.Range(fila, 4, fila, 18).Merge();
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                ws.Cell(fila, 4).Value = "EN BOLIVIANOS";
                ws.Range(fila, 4, fila, 18).Merge();
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila += 3; // unas filas en blanco para bajar la tabla

                // =========================
                // CABECERAS DE COLUMNAS
                // =========================
                int filaCabeceraGrupo = fila;
                int filaCabeceraDetalle = filaCabeceraGrupo + 1;

                void HeaderSimple(int col, string texto)
                {
                    ws.Cell(filaCabeceraGrupo, col).Value = texto;
                    ws.Range(filaCabeceraGrupo, col, filaCabeceraDetalle, col).Merge();
                    ws.Range(filaCabeceraGrupo, col, filaCabeceraDetalle, col)
                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                }

                int c = 1;
                HeaderSimple(c++, "No");
                HeaderSimple(c++, "CARNET DE IDENTIDAD");
                HeaderSimple(c++, "APELLIDOS Y NOMBRE");
                HeaderSimple(c++, "NACIONALIDAD");
                HeaderSimple(c++, "FECHA DE NACIMIENTO");
                HeaderSimple(c++, "SEXO");
                HeaderSimple(c++, "OCUPACIÓN QUE DESEMPEÑA");
                HeaderSimple(c++, "FECHA DE INGRESO");
                HeaderSimple(c++, "DÍAS PAGADOS");
                HeaderSimple(c++, "HABER BÁSICO");
                HeaderSimple(c++, "BONO DE ANTIGÜEDAD");

                // OTROS PAGOS
                int colBonoProduccion = c;
                int colAporteCoop = c + 1;
                ws.Cell(filaCabeceraGrupo, colBonoProduccion).Value = "OTROS PAGOS";
                ws.Range(filaCabeceraGrupo, colBonoProduccion, filaCabeceraGrupo, colAporteCoop).Merge();
                ws.Range(filaCabeceraGrupo, colBonoProduccion, filaCabeceraGrupo, colAporteCoop)
                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                ws.Cell(filaCabeceraDetalle, colBonoProduccion).Value = "BONO DE PRODUCCIÓN";
                ws.Cell(filaCabeceraDetalle, colAporteCoop).Value = "APORTE COOP 3.34%";
                c = colAporteCoop + 1;

                HeaderSimple(c++, "TOTAL GANADO");

                // DESCUENTOS
                int colGestora = c;
                int colRcIva = c + 1;
                int colApSolid = c + 2;
                int colOtros668 = c + 3;
                int colOtrosDesc = c + 4;
                int colTotalDesc = c + 5;

                ws.Cell(filaCabeceraGrupo, colGestora).Value = "DESCUENTOS";
                ws.Range(filaCabeceraGrupo, colGestora, filaCabeceraGrupo, colTotalDesc).Merge();
                ws.Range(filaCabeceraGrupo, colGestora, filaCabeceraGrupo, colTotalDesc)
                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                ws.Cell(filaCabeceraDetalle, colGestora).Value = "GESTORA 12.21%";
                ws.Cell(filaCabeceraDetalle, colRcIva).Value = "RC-IVA 13%";
                ws.Cell(filaCabeceraDetalle, colApSolid).Value = "APORTE SOLIDARIO 0.5%";
                ws.Cell(filaCabeceraDetalle, colOtros668).Value = "OTROS DESCT. 6.68%";
                ws.Cell(filaCabeceraDetalle, colOtrosDesc).Value = "OTROS DESCT.";
                ws.Cell(filaCabeceraDetalle, colTotalDesc).Value = "TOTAL DESCT.";

                c = colTotalDesc + 1;
                HeaderSimple(c++, "LÍQUIDO PAGABLE");
                HeaderSimple(c++, "FIRMA DEL EMPLEADO");

                // Estilo general cabeceras
                var headerRange = ws.Range(filaCabeceraGrupo, 1, filaCabeceraDetalle, colMax);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9E1F2"));
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // columnas amarillas
                ws.Range(filaCabeceraGrupo, colAporteCoop, filaCabeceraDetalle, colAporteCoop)
                  .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));
                ws.Range(filaCabeceraGrupo, colOtros668, filaCabeceraDetalle, colOtros668)
                  .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));
                ws.Range(filaCabeceraGrupo, colOtrosDesc, filaCabeceraDetalle, colOtrosDesc)
                  .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));

                // =========================
                // DATOS
                // =========================
                int filaInicioDatos = filaCabeceraDetalle + 1;
                fila = filaInicioDatos;

                foreach (var item in PlanillaSueldos)
                {
                    int col = 1;
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

                    ws.Cell(fila, col++).Value = string.Empty; // firma

                    fila++;
                }

                int filaFinDatos = fila - 1;

                // Formato numérico
                var rangeMontos = ws.Range(filaInicioDatos, 10, filaFinDatos, 21);
                rangeMontos.Style.NumberFormat.Format = "#,##0.00";
                rangeMontos.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                // Bordes de toda la tabla (como caja)
                var dataRange = ws.Range(filaCabeceraGrupo, 1, filaFinDatos, colMax);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // =========================
                // TOTALES
                // =========================
                int filaTotales = fila;

                ws.Cell(filaTotales, 1).Value = "TOTALES";
                ws.Range(filaTotales, 1, filaTotales, 9).Merge();
                ws.Cell(filaTotales, 1).Style.Font.SetBold();
                ws.Cell(filaTotales, 1).Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Right);

                decimal Total_HaberBasico = PlanillaSueldos.Sum(p => p.HaberBasico);
                decimal Total_BonoAnt = PlanillaSueldos.Sum(p => p.BonoAntiguedad);
                decimal Total_BonoProd = PlanillaSueldos.Sum(p => p.BonoProduccion);
                decimal Total_ApCoop = PlanillaSueldos.Sum(p => p.AporteCoop334);
                decimal Total_TGanado = PlanillaSueldos.Sum(p => p.TotalGanado);
                decimal Total_Gestora = PlanillaSueldos.Sum(p => p.Gestora1221);
                decimal Total_RcIva = PlanillaSueldos.Sum(p => p.RcIva13);
                decimal Total_ApSolid = PlanillaSueldos.Sum(p => p.AporteSolidario05);
                decimal Total_Otros668 = PlanillaSueldos.Sum(p => p.OtrosDesc668);
                decimal Total_OtrosDesc = PlanillaSueldos.Sum(p => p.OtrosDescuentos);
                decimal Total_Desc = PlanillaSueldos.Sum(p => p.TotalDescuentos);
                decimal Total_Liquido = PlanillaSueldos.Sum(p => p.LiquidoPagable);

                ws.Cell(filaTotales, 10).Value = Total_HaberBasico;
                ws.Cell(filaTotales, 11).Value = Total_BonoAnt;
                ws.Cell(filaTotales, 12).Value = Total_BonoProd;
                ws.Cell(filaTotales, 13).Value = Total_ApCoop;
                ws.Cell(filaTotales, 14).Value = Total_TGanado;
                ws.Cell(filaTotales, 15).Value = Total_Gestora;
                ws.Cell(filaTotales, 16).Value = Total_RcIva;
                ws.Cell(filaTotales, 17).Value = Total_ApSolid;
                ws.Cell(filaTotales, 18).Value = Total_Otros668;
                ws.Cell(filaTotales, 19).Value = Total_OtrosDesc;
                ws.Cell(filaTotales, 20).Value = Total_Desc;
                ws.Cell(filaTotales, 21).Value = Total_Liquido;

                var totRange = ws.Range(filaTotales, 1, filaTotales, 21);
                totRange.Style.Font.SetBold();
                totRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));
                totRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // =========================
                // PIE: REPRESENTANTE LEGAL + FECHA
                // =========================
                fila = filaTotales + 3;

                ws.Cell(fila, 8).Value = "REPRESENTANTE LEGAL";
                ws.Range(fila, 8, fila, 11).Merge();
                ws.Cell(fila, 12).Value = ""; // aquí puedes poner el nombre real
                ws.Range(fila, 12, fila, 15).Merge();

                fila += 2;

                ws.Cell(fila, 12).Value = $"La Paz, {DateTime.Now:dd 'de' MMMM 'de' yyyy}";
                ws.Range(fila, 12, fila, 20).Merge();
                ws.Cell(fila, 12).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                // =========================
                // AJUSTES FINALES
                // =========================
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

        [RelayCommand]
        private async Task GuardarOtrosDescuentosAsync()
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
                    if (fila.IdTrabajadorPlanilla <= 0)
                        continue;

                    var ok = await _api.ActualizarOtrosDescAsync(
                        fila.IdTrabajadorPlanilla,
                        fila.OtrosDescuentos);

                    if (!ok)
                        errores++;
                }

                // Recalcular planilla con los nuevos OTROS_DESC manuales
                var recalculoOk = await _api.CalcularPlanillaSueldosAsync(IdPlanillaActual);
                if (!recalculoOk)
                {
                    MessageBox.Show(
                        "Se guardaron Otros Descuentos pero no se pudo recalcular la planilla.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    await CargarPlanillaAsync();
                }

                if (errores == 0)
                {
                    MessageBox.Show(
                        "Otros Descuentos guardados correctamente y planilla recalculada.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Se presentaron errores en {errores} fila(s) al guardar Otros Descuentos.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar Otros Descuentos: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
