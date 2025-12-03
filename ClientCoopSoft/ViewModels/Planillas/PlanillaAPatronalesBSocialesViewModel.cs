using ClientCoopSoft.Models;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Planillas
{
    public partial class PlanillaAPatronalesBSocialesViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public PlanillaAPatronalesBSocialesViewModel(ApiClient api)
        {
            _api = api;
            Gestion = DateTime.Now.Year;
            Mes = DateTime.Now.Month;
            Aportes = new ObservableCollection<PlanillaAportesFilaModel>();

            Instrucciones =
                "1) Primero genere y calcule la Planilla de Sueldos y Salarios.\n" +
                "2) Tome el Id de esa planilla.\n" +
                "3) Ingréselo en 'Id Planilla Sueldos' y presione 'Cargar aportes'.";
        }

        // ====== PROPIEDADES ======
        [ObservableProperty] private int gestion;
        [ObservableProperty] private int mes;
        [ObservableProperty] private int idPlanillaSueldos;
        [ObservableProperty] private string instrucciones = string.Empty;
        [ObservableProperty] private ObservableCollection<PlanillaAportesFilaModel> aportes;

        // ====== COMANDOS ======

        [RelayCommand]
        private async Task CargarAportesAsync()
        {
            if (IdPlanillaSueldos <= 0)
            {
                MessageBox.Show("Indique un Id de planilla de sueldos válido.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lista = await _api.ObtenerDatosPlanillaAportesAsync(IdPlanillaSueldos);

                if (lista == null)
                {
                    MessageBox.Show("No se pudieron obtener los datos de aportes. " +
                                    "Verifique que la planilla de sueldos exista y esté calculada.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Aportes = new ObservableCollection<PlanillaAportesFilaModel>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la planilla de aportes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Limpiar()
        {
            Aportes.Clear();
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            if (Aportes == null || Aportes.Count == 0)
            {
                MessageBox.Show(
                    "No hay datos de aportes para exportar.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Exportar Planilla de Aportes Patronales y Beneficios Sociales a Excel",
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"Planilla_Aportes_{Gestion}_{Mes:00}.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Aportes");

                int fila = 1;
                int colMax = 15;

                // Nombre del mes en texto
                string nombreMes;
                try
                {
                    var dt = new DateTime(Gestion, Mes, 1);
                    nombreMes = dt.ToString("MMMM", new CultureInfo("es-BO")).ToUpperInvariant();
                }
                catch
                {
                    nombreMes = Mes.ToString("00");
                }

                // =========================
                // LOGO
                // =========================
                string logoPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "Logo_Cooperativa.png"
                );

                if (File.Exists(logoPath))
                {
                    var picture = ws.AddPicture(logoPath)
                                    .MoveTo(ws.Cell(2, 1))
                                    .Scale(0.35);
                }

                // Dejo la fila 1 libre
                fila = 2;

                // =========================
                // ENCABEZADO SUPERIOR
                // =========================

                // Nombre de la cooperativa
                ws.Cell(fila, 4).Value =
                    "COOPERATIVA DE AHORRO Y CREDITO DE VINCULO LABORAL \"LA CONFIANZA\" R.L.";
                ws.Range(fila, 4, fila, 14).Merge();
                ws.Cell(fila, 4).Style.Font.SetBold().Font.SetFontSize(14);
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                // Fila: No. EMPLEADOR y PAG. 1 DE 1
                ws.Cell(fila, 4).Value = "No. EMPLEADOR MINISTERIO DE TRABAJO:";
                ws.Range(fila, 4, fila, 10).Merge();
                ws.Cell(fila, 11).Value = "215110027-1"; // TODO: sacar de config/bd
                ws.Range(fila, 11, fila, 13).Merge();

                ws.Cell(fila, 14).Value = "PAG. 1 DE 1";
                ws.Range(fila, 14, fila, colMax).Merge();
                ws.Cell(fila, 14).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                fila++;

                // Fila: NIT
                ws.Cell(fila, 4).Value = "No. NIT:";
                ws.Range(fila, 4, fila, 10).Merge();
                ws.Cell(fila, 11).Value = "215110027"; // TODO: NIT real
                ws.Range(fila, 11, fila, 13).Merge();
                fila++;

                // Fila: Empleador Caja de Salud
                ws.Cell(fila, 4).Value = "No. de EMPLEADOR CAJA DE SALUD:";
                ws.Range(fila, 4, fila, 10).Merge();
                ws.Cell(fila, 11).Value = "710-1-1988"; // TODO
                ws.Range(fila, 11, fila, 13).Merge();
                fila++;

                // Fila: Corresponde al mes...
                ws.Cell(fila, 11).Value = $"CORRESPONDE AL MES DE {nombreMes} {Gestion}";
                ws.Range(fila, 11, fila, colMax).Merge();
                ws.Cell(fila, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                fila += 2;

                // Título central
                ws.Cell(fila, 4).Value = "PLANILLA DE APORTES PATRONALES Y BENEFICIOS SOCIALES";
                ws.Range(fila, 4, fila, 14).Merge();
                ws.Cell(fila, 4).Style.Font.SetBold().Font.SetFontSize(13);
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                ws.Cell(fila, 4).Value = "PERSONAL PERMANENTE";
                ws.Range(fila, 4, fila, 14).Merge();
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila++;

                ws.Cell(fila, 4).Value = "EN BOLIVIANOS";
                ws.Range(fila, 4, fila, 14).Merge();
                ws.Cell(fila, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                fila += 3;

                // =========================
                // CABECERAS
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
                HeaderSimple(c++, "TOTAL GANADO");

                // APORTES PATRONALES / BENEFICIOS
                int colCps = c;
                int colRiesgo = c + 1;
                int colProvi = c + 2;
                int colApSolid = c + 3;
                int colTotalAp = c + 4;

                ws.Cell(filaCabeceraGrupo, colCps).Value = "APORTES PATRONALES Y BENEFICIOS SOCIALES";
                ws.Range(filaCabeceraGrupo, colCps, filaCabeceraGrupo, colTotalAp).Merge();
                ws.Range(filaCabeceraGrupo, colCps, filaCabeceraGrupo, colTotalAp)
                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                ws.Cell(filaCabeceraDetalle, colCps).Value = "CPS 10%";
                ws.Cell(filaCabeceraDetalle, colRiesgo).Value = "RIESGO DE PRIMA 1.71%";
                ws.Cell(filaCabeceraDetalle, colProvi).Value = "PROVIVIENDA 2%";
                ws.Cell(filaCabeceraDetalle, colApSolid).Value = "APORTE SOLIDARIO 3.5%";
                ws.Cell(filaCabeceraDetalle, colTotalAp).Value = "TOTAL APORTES";

                // Estilo general cabeceras
                var headerRange = ws.Range(filaCabeceraGrupo, 1, filaCabeceraDetalle, colMax);
                headerRange.Style.Font.SetBold();
                headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9E1F2"));
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Resaltar columnas de aportes en amarillo suave
                ws.Range(filaCabeceraGrupo, colCps, filaCabeceraDetalle, colTotalAp)
                  .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));

                // =========================
                // DATOS
                // =========================
                int filaInicioDatos = filaCabeceraDetalle + 1;
                fila = filaInicioDatos;

                foreach (var item in Aportes)
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
                    ws.Cell(fila, col++).Value = item.TotalGanado;

                    ws.Cell(fila, col++).Value = item.Cps10;
                    ws.Cell(fila, col++).Value = item.RiesgoPrima171;
                    ws.Cell(fila, col++).Value = item.Provivienda2;
                    ws.Cell(fila, col++).Value = item.AporteSolidario35;
                    ws.Cell(fila, col++).Value = item.TotalAportes;

                    fila++;
                }

                int filaFinDatos = fila - 1;

                // Formato numérico
                var rangeMontos = ws.Range(filaInicioDatos, 10, filaFinDatos, colMax);
                rangeMontos.Style.NumberFormat.Format = "#,##0.00";
                rangeMontos.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                // Bordes de toda la tabla
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

                decimal Total_TotalGanado = Aportes.Sum(p => p.TotalGanado);
                decimal Total_Cps10 = Aportes.Sum(p => p.Cps10);
                decimal Total_Riesgo = Aportes.Sum(p => p.RiesgoPrima171);
                decimal Total_Provi = Aportes.Sum(p => p.Provivienda2);
                decimal Total_ApSolid = Aportes.Sum(p => p.AporteSolidario35);
                decimal Total_Aportes = Aportes.Sum(p => p.TotalAportes);

                ws.Cell(filaTotales, 10).Value = Total_TotalGanado;
                ws.Cell(filaTotales, 11).Value = Total_Cps10;
                ws.Cell(filaTotales, 12).Value = Total_Riesgo;
                ws.Cell(filaTotales, 13).Value = Total_Provi;
                ws.Cell(filaTotales, 14).Value = Total_ApSolid;
                ws.Cell(filaTotales, 15).Value = Total_Aportes;

                var totRange = ws.Range(filaTotales, 1, filaTotales, colMax);
                totRange.Style.Font.SetBold();
                totRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2CC"));
                totRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // =========================
                // PIE: REPRESENTANTE LEGAL + FECHA
                // =========================
                fila = filaTotales + 3;

                ws.Cell(fila, 6).Value = "REPRESENTANTE LEGAL";
                ws.Range(fila, 6, fila, 8).Merge();
                ws.Cell(fila, 9).Value = ""; // TODO: nombre real
                ws.Range(fila, 9, fila, 12).Merge();

                fila += 2;

                ws.Cell(fila, 9).Value = $"La Paz, {DateTime.Now:dd 'de' MMMM 'de' yyyy}";
                ws.Range(fila, 9, fila, 15).Merge();
                ws.Cell(fila, 9).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                // =========================
                // AJUSTES FINALES
                // =========================
                ws.Columns().AdjustToContents();

                wb.SaveAs(dlg.FileName);

                MessageBox.Show(
                    "Planilla de aportes exportada correctamente a Excel.",
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
    }
}
