using ClientCoopSoft.DTO.Extras;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.LogsAcceso
{
    public partial class LogsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<LogsAccesoDTO> listaLogs = new();

        public LogsViewModel(ApiClient api)
        {
            _apiClient = api;
        }

        public async Task CargarLogsAccesoAsync()
        {
            var list = await _apiClient.ObtenerListaLogsAcceso();
            if(list != null)
            {
                ListaLogs = new ObservableCollection<LogsAccesoDTO>(list);
            }
        }

        [RelayCommand]
        private async Task ExportarExcelAsync()
        {
            try
            {
                if (ListaLogs == null || ListaLogs.Count == 0)
                {
                    MessageBox.Show("No existen registros de logs para exportar.", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    FileName = "LogsAcceso.xlsx"
                };

                if (dlg.ShowDialog() != true)
                    return;

                await Task.Run(() => ExportLogsToXlsx(dlg.FileName, ListaLogs));

                MessageBox.Show("Logs exportados a Excel correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando logs a Excel: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportLogsToXlsx(string path, ObservableCollection<LogsAccesoDTO> items)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("LogsAcceso");

            var headers = new[]
            {
                "No",
                "Nombre de Usuario",
                "Apellidos y Nombres",
                "Fecha Login",
                "Hora Login",
                "Fecha Logout",
                "Hora Logout"
            };

            // Encabezados
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var log in items)
            {
                ws.Cell(row, 1).Value = log.IdLog;
                ws.Cell(row, 2).Value = log.NombreUsuario;
                ws.Cell(row, 3).Value = log.ApellidosNombres;
                ws.Cell(row, 4).Value = log.FechaLogin.ToString() ?? string.Empty;
                ws.Cell(row, 5).Value = log.HoraLogin.ToString() ?? string.Empty;
                ws.Cell(row, 6).Value = log.FechaLogout?.ToString() ?? string.Empty;
                ws.Cell(row, 7).Value = log.HoraLogout?.ToString() ?? string.Empty;
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(path);
        }
    }
}
