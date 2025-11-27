using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Asistencia
{
    public partial class ListarAsistenciasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<AsistenciaListarDTO> asistencias = new();

        public ListarAsistenciasViewModel(ApiClient apiCient)
        {
            _apiClient = apiCient;
        }

        public async Task CargarAsistenciasAsync()
        {
            var list = await _apiClient.ObtenerListaAsistencias();
            if(list != null)
            {
                Asistencias = new ObservableCollection<AsistenciaListarDTO>(list);
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
                    FileName = "RegistroAsistencias.pdf"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToPdf(dlg.FileName, Asistencias));
                MessageBox.Show("Exportado a PDF correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando a PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportToPdf(string path, IList<AsistenciaListarDTO> items)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Si tu versión de PdfSharpCore usa XFontStyleEx, cámbialo aquí.
            var titleFont = new XFont("Arial", 14.0, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12.0, XFontStyle.Bold);
            var font = new XFont("Arial", 10.0, XFontStyle.Regular);

            double margin = 40;
            double y = margin;
            double lineHeight = 14;
            double pageWidth = page.Width.Point;

            // Función local para dibujar título + cabecera (la usamos en la primera página y en cada salto de página)
            void DibujarEncabezado()
            {
                y = margin;

                // Título
                gfx.DrawString("Registro de Asistencias", titleFont, XBrushes.Black,
                    new XRect(0, 10, pageWidth, 20), XStringFormats.TopCenter);
                y += 30;

                // Cabecera
                string header = "No | CI | Apellidos y Nombres | Cargo | Fecha | Hora | Oficina | Tipo";
                gfx.DrawString(header, headerFont, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
            }

            DibujarEncabezado();

            int contador = 1;

            foreach (var a in items)
            {
                // Salto de página si no hay espacio
                if (y + lineHeight > page.Height - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    pageWidth = page.Width.Point;

                    DibujarEncabezado();
                }

                string fecha = a.Fecha.ToString("yyyy-MM-dd");
                string hora = a.Hora.ToString(@"hh\:mm");
                string tipo = a.EsEntrada ? "Entrada" : "Salida";

                string line = $"{contador} | {a.CI} | {a.ApellidosNombres} | {a.Cargo} | {fecha} | {hora} | {a.Oficina} | {tipo}";

                gfx.DrawString(line, font, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
                contador++;
            }

            using var stream = File.Create(path);
            doc.Save(stream);
        }

    }
}
