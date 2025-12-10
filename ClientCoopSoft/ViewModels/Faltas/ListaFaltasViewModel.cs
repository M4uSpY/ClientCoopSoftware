using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.DTO.Faltas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClientCoopSoft.ViewModels.Faltas
{
    public partial class ListaFaltasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<ListarFaltasDTO> faltas = new();

        [ObservableProperty]
        private ICollectionView faltasView;

        // 👉 Texto de búsqueda
        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        public ListaFaltasViewModel(ApiClient apiCient)
        {
            _apiClient = apiCient;

            FaltasView = CollectionViewSource.GetDefaultView(Faltas);
            if (FaltasView != null)
            {
                FaltasView.Filter = FaltasFilter;
            }
        }

        public async Task CargarFaltasAsync()
        {
            var list = await _apiClient.ObtenerListaFaltas();
            if (list != null)
            {
                Faltas = new ObservableCollection<ListarFaltasDTO>(list);
            }
        }

        partial void OnFaltasChanged(ObservableCollection<ListarFaltasDTO> value)
        {
            FaltasView = CollectionViewSource.GetDefaultView(value);
            if (FaltasView != null)
            {
                FaltasView.Filter = FaltasFilter;
                FaltasView.Refresh();
            }
        }

        // Cuando cambia el texto del buscador
        partial void OnTextoBusquedaChanged(string value)
        {
            FaltasView?.Refresh();
        }

        // ====== FILTRO ======
        private bool FaltasFilter(object obj)
        {
            if (obj is not ListarFaltasDTO f)
                return false;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
                return true;

            var filtro = TextoBusqueda.Trim().ToLower();

            string fecha = f.Fecha.ToString("yyyy-MM-dd");
            string estadoArchivo = (f.EstadoArchivoJustificativo ?? string.Empty).ToLower();

            bool coincideCI = (f.CI ?? string.Empty).ToLower().Contains(filtro);
            bool coincideNombre = (f.ApellidosNombres ?? string.Empty).ToLower().Contains(filtro);
            bool coincideTipo = (f.Tipo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideFecha = fecha.Contains(filtro);
            bool coincideDescripcion = (f.Descripcion ?? string.Empty).ToLower().Contains(filtro);
            bool coincideEstadoArchivo = estadoArchivo.Contains(filtro);

            return coincideCI
                || coincideNombre
                || coincideTipo
                || coincideFecha
                || coincideDescripcion
                || coincideEstadoArchivo;
        }

        [RelayCommand]
        private async Task SubirArchivoFaltaAsync(ListarFaltasDTO? falta)
        {
            if (falta == null)
                return;

            var ofd = new OpenFileDialog
            {
                Title = "Seleccionar archivo justificativo",
                Filter = "PDF|*.pdf|Imágenes|*.png;*.jpg;*.jpeg|Todos los archivos|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                var ok = await _apiClient.SubirArchivoJustificativoFaltaAsync(falta.IdFalta, ofd.FileName);
                if (ok)
                {
                    MessageBox.Show("Archivo justificativo guardado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarFaltasAsync(); // recargar para actualizar el estado
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el archivo justificativo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // VER ARCHIVO
        [RelayCommand]
        private async Task VerArchivoFaltaAsync(ListarFaltasDTO? falta)
        {
            if (falta == null)
                return;

            var bytes = await _apiClient.DescargarArchivoJustificativoFaltaAsync(falta.IdFalta);
            if (bytes == null || bytes.Length == 0)
            {
                MessageBox.Show("Esta falta no tiene archivo justificativo.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var tempFile = Path.Combine(Path.GetTempPath(), $"Justificativo_Falta_{falta.IdFalta}.pdf");
                await File.WriteAllBytesAsync(tempFile, bytes);

                var psi = new ProcessStartInfo(tempFile)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo justificativo.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ELIMINAR FALTA
        [RelayCommand]
        private async Task EliminarFaltaAsync(ListarFaltasDTO? falta)
        {
            if (falta == null)
                return;

            var confirmar = MessageBox.Show(
                "¿Está seguro que desea eliminar esta falta?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes)
                return;

            var ok = await _apiClient.EliminarFaltaAsync(falta.IdFalta);
            if (ok)
            {
                Faltas.Remove(falta);
            }
            else
            {
                MessageBox.Show("No se pudo eliminar la falta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =================== EXPORTAR A PDF ===================
        [RelayCommand]
        private async Task ExportarPdfAsync()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "PDF document|*.pdf",
                    FileName = "ControlFaltas.pdf"
                };
                if (dlg.ShowDialog() != true) return;

                // Puedes usar Faltas (toda la lista) o FaltasView filtrada.
                await Task.Run(() => ExportToPdf(dlg.FileName, Faltas));

                MessageBox.Show("Exportado a PDF correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exportando a PDF: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToPdf(string path, IList<ListarFaltasDTO> items)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 14.0, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12.0, XFontStyle.Bold);
            var font = new XFont("Arial", 10.0, XFontStyle.Regular);

            double margin = 40;
            double y = margin;
            double lineHeight = 14;
            double pageWidth = page.Width.Point;

            void DibujarEncabezado()
            {
                y = margin;

                // Título
                gfx.DrawString("Control de Faltas", titleFont, XBrushes.Black,
                    new XRect(0, 10, pageWidth, 20), XStringFormats.TopCenter);
                y += 30;

                // Cabecera
                string header = "No | CI | Apellidos y Nombres | Tipo | Fecha | Descripción | Estado archivo";
                gfx.DrawString(header, headerFont, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
            }

            DibujarEncabezado();

            int contador = 1;

            foreach (var f in items)
            {
                if (y + lineHeight > page.Height - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    pageWidth = page.Width.Point;
                    DibujarEncabezado();
                }

                string fecha = f.Fecha.ToString("yyyy-MM-dd");
                string estadoArchivo = f.EstadoArchivoJustificativo ?? "";
                string line =
                    $"{contador} | {f.CI} | {f.ApellidosNombres} | {f.Tipo} | {fecha} | {f.Descripcion} | {estadoArchivo}";

                gfx.DrawString(line, font, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
                contador++;
            }

            using var stream = File.Create(path);
            doc.Save(stream);
        }

    }
}
