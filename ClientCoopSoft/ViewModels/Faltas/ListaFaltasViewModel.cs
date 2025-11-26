using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.DTO.Faltas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Faltas
{
    public partial class ListaFaltasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<ListarFaltasDTO> faltas = new();

        public ListaFaltasViewModel(ApiClient apiCient)
        {
            _apiClient = apiCient;
        }

        public async Task CargarFaltasAsync()
        {
            var list = await _apiClient.ObtenerListaFaltas();
            if (list != null)
            {
                Faltas = new ObservableCollection<ListarFaltasDTO>(list);
            }
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
    }
}
