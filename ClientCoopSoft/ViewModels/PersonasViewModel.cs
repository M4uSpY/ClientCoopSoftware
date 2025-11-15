using ClientCoopSoft.DTO.Personas;
using ClientCoopSoft.Models;
using ClientCoopSoft.ViewModels.Personas;
using ClientCoopSoft.Views;
using ClientCoopSoft.Views.Personas;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;



namespace ClientCoopSoft.ViewModels
{
    public partial class PersonasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<Persona> personas = new();

        public PersonasViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task CargarPersonasAsync()
        {
            var list = await _apiClient.ObtenerPersonasAsync();
            if (list != null)
            {
                Personas = new ObservableCollection<Persona>(list);
            }
        }

        [RelayCommand]
        private async Task AgregarPersona(Persona persona)
        {
            var ventana = new CrearPersonaView
            {
                Owner = Application.Current.MainWindow
            };
            var vm = new CrearPersonaViewModel(_apiClient);
            ventana.DataContext = vm;
            bool? resultado = ventana.ShowDialog();
            if(resultado == true)
            {
                await CargarPersonasAsync();
            }
        }
        [RelayCommand]
        private async Task EditarPersona(Persona persona)
        {
            if (persona is null)
            {
                return;
            }
            var ventana = new EditarPersonaView
            {
                Owner = Application.Current.MainWindow
            };
            var vm = new EditarPersonaViewModel(persona, _apiClient);
            ventana.DataContext = vm;

            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                await CargarPersonasAsync();
            }
        }
        [RelayCommand]
        private async Task EliminarPersonaAsync(Persona persona)
        {
            if (persona == null) return;

            var confirm = MessageBox.Show(
                $"¿Deseas eliminar la persona '{persona.PrimerNombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // Llamada al API
            bool ok = await _apiClient.EliminarPersonaAsync(persona.IdPersona);
            if (ok)
            {
                // Remover localmente para actualizar UI inmediatamente
                Personas.Remove(persona);
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
                    FileName = "Personas.xlsx"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToXlsx(dlg.FileName, Personas));
                MessageBox.Show("Exportado a Excel correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando a Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    FileName = "Personas.pdf"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToPdf(dlg.FileName, Personas));
                MessageBox.Show("Exportado a PDF correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando a PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------- Helpers ----------------

        private void ExportToXlsx(string path, ObservableCollection<Persona> items)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Personas");

            var headers = new[] {
                "No", "CI", "Ap.Paterno", "Ap.Materno", "Nombres",
                "F.Nacimiento", "Genero", "Direccion", "Telefono", "Email"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var p in items)
            {
                ws.Cell(row, 1).Value = p.IdPersona;
                ws.Cell(row, 2).Value = p.CarnetIdentidad;
                ws.Cell(row, 3).Value = p.ApellidoPaterno;
                ws.Cell(row, 4).Value = p.ApellidoMaterno;
                ws.Cell(row, 5).Value = p.NombreCompleto;
                ws.Cell(row, 6).Value = p.FechaNacimiento.ToString("yyyy-MM-dd");
                ws.Cell(row, 7).Value = (p.Genero) ? (p.Genero ? "Masculino" : "Femenino") : "";
                ws.Cell(row, 8).Value = p.Direccion;
                ws.Cell(row, 9).Value = p.Telefono;
                ws.Cell(row, 10).Value = p.Email;
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(path);
        }

        private void ExportToPdf(string path, ObservableCollection<Persona> items)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // usa XFontStyleEx para PdfSharpCore
            var titleFont = new XFont("Arial", 14.0, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12.0, XFontStyle.Bold);
            var font = new XFont("Arial", 10.0, XFontStyle.Regular);

            double margin = 40;
            double y = margin;
            double lineHeight = 14;
            double pageWidth = page.Width.Point;

            // Título
            gfx.DrawString("Lista de Personas", titleFont, XBrushes.Black,
                new XRect(0, 10, pageWidth, 20), XStringFormats.TopCenter);
            y += 30;

            // Cabecera
            string header = "No | CI | Ap.Paterno | Ap.Materno | Nombres | F.Nac. | Genero | Direccion | Telefono | Email";
            gfx.DrawString(header, headerFont, XBrushes.Black, new XPoint(margin, y));
            y += lineHeight;

            foreach (var p in items)
            {
                string fecha = p.FechaNacimiento != DateTime.MinValue ? p.FechaNacimiento.ToString("yyyy-MM-dd") : "";
                string genero = (p.Genero) ? (p.Genero? "Masculino" : "Femenino") : "";
                string line = $"{p.IdPersona} | {p.CarnetIdentidad} | {p.ApellidoPaterno} | {p.ApellidoMaterno} | {p.NombreCompleto} | {fecha} | {genero} | {p.Direccion} | {p.Telefono} | {p.Email}";
                gfx.DrawString(line, font, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;

                if (y + lineHeight > page.Height - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }
            }

            using var stream = System.IO.File.OpenWrite(path);
            doc.Save(stream);
        }
    }
}
