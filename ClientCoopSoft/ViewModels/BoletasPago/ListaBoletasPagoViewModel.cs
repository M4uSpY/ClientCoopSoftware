using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.BoletasPago
{
    public partial class ListaBoletasPagoViewModel : ObservableObject
    {
        private readonly ApiClient _api;
        private readonly int _idTrabajador;

        [ObservableProperty]
        private ObservableCollection<BoletaPagoModel> boletasPago = new();

        [ObservableProperty]
        private BoletaPagoModel? boletaSeleccionada;

        public ListaBoletasPagoViewModel(ApiClient api, int idTrabajador)
        {
            _api = api;
            _idTrabajador = idTrabajador;
        }

        public async Task CargarBoletasPagoAsync()
        {
            try
            {
                var lista = await _api.ObtenerBoletasPagoAsync(_idTrabajador)
                            ?? new List<BoletaPagoModel>();

                BoletasPago = new ObservableCollection<BoletaPagoModel>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar boletas de pago: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DescargarPdfAsync(BoletaPagoModel? boleta)
        {
            boleta ??= BoletaSeleccionada;

            if (boleta == null)
            {
                MessageBox.Show("Seleccione una boleta de la lista.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var bytes = await _api.ObtenerBoletaPdfAsync(_idTrabajador, boleta.IdPlanilla);
                if (bytes == null)
                {
                    MessageBox.Show("No se pudo obtener el PDF de la boleta.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Title = "Guardar boleta de pago",
                    Filter = "Archivo PDF|*.pdf",
                    FileName = $"Boleta_{boleta.Gestion}_{boleta.Mes:D2}_{boleta.NombreCompleto}.pdf"
                };

                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllBytes(sfd.FileName, bytes);
                    MessageBox.Show("Boleta descargada correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar la boleta: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
