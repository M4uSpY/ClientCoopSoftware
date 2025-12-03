using ClientCoopSoft.DTO.Historicos;
using ClientCoopSoft.ViewModels.Reportes;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Historicos
{
    public partial class HistoricoUsuarioViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        [ObservableProperty]
        private ObservableCollection<HistoricoUsuarioListarDTO> historialu = new();

        public HistoricoUsuarioViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        public async Task CargarHistorialUsuarioAsync()
        {
            var list = await _apiClient.ObtenerHistorialUsuariosAsync();
            if (list != null)
            {
                Historialu = new ObservableCollection<HistoricoUsuarioListarDTO>(list);
            }
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            if (Historialu == null || Historialu.Count == 0)
            {
                MessageBox.Show(
                    "No hay datos para exportar.",
                    "Exportar a Excel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Guardar historial de usuarios",
                Filter = "Archivo de Excel (*.xlsx)|*.xlsx",
                FileName = $"HistorialUsuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Historial Usuarios");

                    // Encabezados
                    ws.Cell(1, 1).Value = "No";
                    ws.Cell(1, 2).Value = "IdUsuario";
                    ws.Cell(1, 3).Value = "Usuario que modificó";
                    ws.Cell(1, 4).Value = "Fecha de modificación";
                    ws.Cell(1, 5).Value = "Acción realizada";
                    ws.Cell(1, 6).Value = "Campo";
                    ws.Cell(1, 7).Value = "Valor anterior";
                    ws.Cell(1, 8).Value = "Valor actual";

                    var headerRange = ws.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Datos
                    var row = 2;
                    foreach (var item in Historialu)
                    {
                        ws.Cell(row, 1).Value = item.IdHistorico;
                        ws.Cell(row, 2).Value = item.IdUsuario;
                        ws.Cell(row, 3).Value = item.UsuarioModifico;
                        ws.Cell(row, 4).Value = item.FechaModificacion;
                        ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        ws.Cell(row, 5).Value = item.Accion;
                        ws.Cell(row, 6).Value = item.Campo;
                        ws.Cell(row, 7).Value = item.ValorAnterior ?? string.Empty;
                        ws.Cell(row, 8).Value = item.ValorActual ?? string.Empty;

                        row++;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show(
                    "El historial de usuarios se exportó correctamente.",
                    "Exportar a Excel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al exportar a Excel:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        [RelayCommand]
        private void Volver()
        {
            _dashboard.CurrentView = new ReportesViewModel(_apiClient, _dashboard);
        }
    }
}
