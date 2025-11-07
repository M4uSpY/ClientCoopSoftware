using ClientCoopSoft.Models;
using ClientCoopSoft.Views;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace ClientCoopSoft.ViewModels
{
    public partial class UsuariosViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<Usuario> usuarios = new();

        public UsuariosViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task LoadUsuariosAsync()
        {
            var list = await _apiClient.GetUsuariosAsync();
            if(list != null)
            {
                Usuarios = new ObservableCollection<Usuario>(list);
            }
        }
        [RelayCommand]
        private async Task AgregarUsuarioAsync()
        {
            var ventana = new Views.CrearUsuarioView
            {
                Owner = Application.Current.MainWindow
            };

            var vm = new CrearUsuarioViewModel(_apiClient);
            ventana.DataContext = vm;

            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                await LoadUsuariosAsync();
            }
        }
        [RelayCommand]
        private async Task EditarUsuario(Usuario usuario)
        {
            if(usuario is null)
            {
                return;
            }
            var ventana = new EditarUsuarioWindow
            {
                Owner = Application.Current.MainWindow
            };
            var vm = new EditarUsuarioViewModel(usuario, _apiClient);
            ventana.DataContext = vm;

            bool? resultado = ventana.ShowDialog();
            if(resultado == true)
            {
                await LoadUsuariosAsync();
            }
        }
        [RelayCommand]
        private async Task EliminarUsuarioAsync(Usuario usuario)
        {
            if (usuario == null) return;

            var confirm = MessageBox.Show(
                $"¿Deseas eliminar el usuario '{usuario.NombreUsuario}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // Llamada al API
            bool ok = await _apiClient.EliminarUsuarioAsync(usuario.IdUsuario);
            if (ok)
            {
                // Remover localmente para actualizar UI inmediatamente
                Usuarios.Remove(usuario);
                MessageBox.Show("Usuario eliminado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ocurrió un error al eliminar el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------- Exportaciones (Excel y PDF) ----------------

        [RelayCommand]
        private async Task ExportarExcelAsync()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    FileName = "Usuarios.xlsx"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToXlsx(dlg.FileName, Usuarios));
                System.Windows.MessageBox.Show("Exportado a Excel correctamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exportando a Excel: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ExportarPdfAsync()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "PDF document|*.pdf",
                    FileName = "Usuarios.pdf"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToPdf(dlg.FileName, Usuarios));
                System.Windows.MessageBox.Show("Exportado a PDF correctamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exportando a PDF: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // ---------------- Helpers ----------------

        private void ExportToXlsx(string path, ObservableCollection<Usuario> items)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Usuarios");

            var headers = new[] { "No", "CI", "NombreUsuario", "Rol", "Descripción", "Apellidos y Nombres", "Estado", "Genero" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var u in items)
            {
                ws.Cell(row, 1).Value = u.IdUsuario;
                ws.Cell(row, 2).Value = u.CI;
                ws.Cell(row, 3).Value = u.NombreUsuario;
                ws.Cell(row, 4).Value = u.Rol;
                ws.Cell(row, 5).Value = u.DescripcionRol;
                ws.Cell(row, 6).Value = u.NombreCompleto;
                ws.Cell(row, 7).Value = u.Activo;
                ws.Cell(row, 8).Value = u.Genero;
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(path);
        }

        private void ExportToPdf(string path, ObservableCollection<Usuario> items)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var font = new XFont("Arial", 10, XFontStyle.Regular);

            double margin = 40;
            double y = margin;
            double lineHeight = 14;
            double pageWidth = page.Width.Point;

            // Título
            gfx.DrawString("Lista de Usuarios", new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black,
                new XRect(0, 10, pageWidth, 20), XStringFormats.TopCenter);
            y += 30;

            // Cabecera
            string header = "No | CI | NombreUsuario | Rol | Descripción | Apellidos y Nombres | Estado | Genero";
            gfx.DrawString(header, headerFont, XBrushes.Black, new XPoint(margin, y));
            y += lineHeight;

            foreach (var u in items)
            {
                string line = $"{u.IdUsuario} | {u.CI} | {u.NombreUsuario} | {u.Rol} | {u.DescripcionRol} | {u.NombreCompleto} | {u.Activo} | {u.Genero}";
                gfx.DrawString(line, font, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;

                if (y + lineHeight > page.Height - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }
            }

            // Guardar
            using var stream = File.OpenWrite(path);
            doc.Save(stream);
        }
    }
}

