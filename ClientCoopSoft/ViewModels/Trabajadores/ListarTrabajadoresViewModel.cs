using ClientCoopSoft.Models;
using ClientCoopSoft.Views.Trabajadores;
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

namespace ClientCoopSoft.ViewModels.Trabajadores
{
    public partial class ListarTrabajadoresViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<Trabajador> trabajadores = new();

        [ObservableProperty]
        private ICollectionView trabajadoresView;

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        public ListarTrabajadoresViewModel(ApiClient api)
        {
            _apiClient = api;

            TrabajadoresView = CollectionViewSource.GetDefaultView(Trabajadores);
            if (TrabajadoresView != null)
            {
                TrabajadoresView.Filter = TrabajadoresFilter;
            }
        }

        public async Task CargarTrabajadoresAsync()
        {
            var list = await _apiClient.ObtenerTrabajadoresAsync();
            if(list != null)
            {
                Trabajadores = new ObservableCollection<Trabajador>(list);
            }
        }
        // Se ejecuta cuando cambia Trabajadores (source generator)
        partial void OnTrabajadoresChanged(ObservableCollection<Trabajador> value)
        {
            TrabajadoresView = CollectionViewSource.GetDefaultView(value);
            if (TrabajadoresView != null)
            {
                TrabajadoresView.Filter = TrabajadoresFilter;
                TrabajadoresView.Refresh();
            }
        }

        // Se ejecuta cuando cambia TextoBusqueda
        partial void OnTextoBusquedaChanged(string value)
        {
            TrabajadoresView?.Refresh();
        }

        // ====== FILTRO ======
        private bool TrabajadoresFilter(object obj)
        {
            if (obj is not Trabajador t)
                return false;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
                return true;

            var filtro = TextoBusqueda.Trim().ToLower();

            bool coincideCI = (t.CI ?? string.Empty).ToLower().Contains(filtro);
            bool coincideApellidos = (t.Apellidos ?? string.Empty).ToLower().Contains(filtro);
            bool coincideNombres = (t.Nombres ?? string.Empty).ToLower().Contains(filtro);
            bool coincideNacionalidad = (t.Nacionalidad ?? string.Empty).ToLower().Contains(filtro);
            bool coincideCargo = (t.Cargo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideOficina = (t.NombreOficina ?? string.Empty).ToLower().Contains(filtro);
            bool coincideEstado = (t.Activo ? "activo" : "inactivo").Contains(filtro);

            return coincideCI
                || coincideApellidos
                || coincideNombres
                || coincideNacionalidad
                || coincideCargo
                || coincideOficina
                || coincideEstado;
        }

        [RelayCommand]
        private async Task AgregarTrabajador(Trabajador trabajador)
        {
            var ventana = new CrearTrabajadorView
            {
                Owner = App.Current.MainWindow
            };
            var vm = new CrearTrabajadorViewModel(_apiClient);
            ventana.DataContext = vm;
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                await CargarTrabajadoresAsync();
            }
        }

        [RelayCommand]
        private async Task EditarTrabajador(Trabajador trabajador)
        {
            var ventana = new EditarTrabajadorView
            {
                Owner = App.Current.MainWindow
            };
            var vm = new EditarTrabajadorViewModel(trabajador,_apiClient);
            ventana.DataContext = vm;
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                await CargarTrabajadoresAsync();
            }
        }

        [RelayCommand]
        private async Task EliminarTrabajador(Trabajador trabajador)
        {
            if (trabajador == null) return;

            var confirm = MessageBox.Show(
                $"¿Deseas eliminar el trabajador'{trabajador.Nombres}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            // Llamada al API
            bool ok = await _apiClient.EliminarTrabajadorAsync(trabajador.IdTrabajador);
            if (ok)
            {
                // Remover localmente para actualizar UI inmediatamente
                Trabajadores.Remove(trabajador);
                MessageBox.Show("Usuario eliminado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ocurrió un error al eliminar el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    FileName = "ListaTrabajadores.pdf"
                };
                if (dlg.ShowDialog() != true) return;

                await Task.Run(() => ExportToPdf(dlg.FileName, Trabajadores));

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

        private void ExportToPdf(string path, IList<Trabajador> items)
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

                gfx.DrawString("Lista de Trabajadores", titleFont, XBrushes.Black,
                    new XRect(0, 10, pageWidth, 20), XStringFormats.TopCenter);
                y += 30;

                string header = "No | CI | Apellidos | Nombres | Nacionalidad | Género | Cargo | Oficina | Estado";
                gfx.DrawString(header, headerFont, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
            }

            DibujarEncabezado();

            int contador = 1;

            foreach (var t in items)
            {
                if (y + lineHeight > page.Height - margin)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    pageWidth = page.Width.Point;
                    DibujarEncabezado();
                }

                string genero = t.Genero ? "Masculino" : "Femenino";
                string estado = t.Activo ? "Activo" : "Inactivo";

                string line =
                    $"{contador} | {t.CI} | {t.Apellidos} | {t.Nombres} | {t.Nacionalidad} | {genero} | {t.Cargo} | {t.NombreOficina} | {estado}";

                gfx.DrawString(line, font, XBrushes.Black, new XPoint(margin, y));
                y += lineHeight;
                contador++;
            }

            using var stream = File.Create(path);
            doc.Save(stream);
        }

    }
}
