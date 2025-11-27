using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClientCoopSoft.ViewModels.BoletasPago
{
    public partial class ListaBoletasPagoViewModel : ObservableObject
    {
        private readonly ApiClient _api;
        private readonly int _idTrabajador;

        [ObservableProperty]
        private ObservableCollection<BoletaPagoModel> boletasPago = new();

        [ObservableProperty]
        private ICollectionView boletasView;

        // 👉 Años disponibles (Gestión)
        [ObservableProperty]
        private ObservableCollection<int> anios = new();

        // 👉 Meses disponibles (1–12 con nombre)
        public class MesItem
        {
            public int Numero { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        [ObservableProperty]
        private BoletaPagoModel? boletaSeleccionada;

        [ObservableProperty]
        private ObservableCollection<MesItem> meses = new();

        // 👉 Selección de filtros
        [ObservableProperty]
        private int? anioSeleccionado;

        [ObservableProperty]
        private MesItem? mesSeleccionado;

        public ListaBoletasPagoViewModel(ApiClient api, int idTrabajador)
        {
            _api = api;
            _idTrabajador = idTrabajador;

            BoletasView = CollectionViewSource.GetDefaultView(BoletasPago);
            if (BoletasView != null)
                BoletasView.Filter = BoletasFilter;

            // Meses (1-12, en español)
            Meses = new ObservableCollection<MesItem>(new[]
            {
                new MesItem { Numero = 1,  Nombre = "Enero" },
                new MesItem { Numero = 2,  Nombre = "Febrero" },
                new MesItem { Numero = 3,  Nombre = "Marzo" },
                new MesItem { Numero = 4,  Nombre = "Abril" },
                new MesItem { Numero = 5,  Nombre = "Mayo" },
                new MesItem { Numero = 6,  Nombre = "Junio" },
                new MesItem { Numero = 7,  Nombre = "Julio" },
                new MesItem { Numero = 8,  Nombre = "Agosto" },
                new MesItem { Numero = 9,  Nombre = "Septiembre" },
                new MesItem { Numero = 10, Nombre = "Octubre" },
                new MesItem { Numero = 11, Nombre = "Noviembre" },
                new MesItem { Numero = 12, Nombre = "Diciembre" },
            });
        }

        public async Task CargarBoletasPagoAsync()
        {
            try
            {
                var lista = await _api.ObtenerBoletasPagoAsync(_idTrabajador)
                            ?? new List<BoletaPagoModel>();

                BoletasPago = new ObservableCollection<BoletaPagoModel>(lista);

                var aniosDistintos = lista
                    .Select(b => b.Gestion)
                    .Distinct()
                    .OrderByDescending(a => a)
                    .ToList();

                Anios = new ObservableCollection<int>(aniosDistintos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar boletas de pago: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        partial void OnBoletasPagoChanged(ObservableCollection<BoletaPagoModel> value)
        {
            BoletasView = CollectionViewSource.GetDefaultView(value);
            if (BoletasView != null)
            {
                BoletasView.Filter = BoletasFilter;
                BoletasView.Refresh();
            }
        }

        // Cuando cambia año o mes seleccionado → refrescar filtro
        partial void OnAnioSeleccionadoChanged(int? value)
        {
            BoletasView?.Refresh();
        }

        partial void OnMesSeleccionadoChanged(MesItem? value)
        {
            BoletasView?.Refresh();
        }

        // ====== FILTRO POR AÑO / MES ======
        private bool BoletasFilter(object obj)
        {
            if (obj is not BoletaPagoModel b)
                return false;

            // Si no hay filtros, mostrar todo
            if (AnioSeleccionado is null && MesSeleccionado is null)
                return true;

            bool coincideAnio = !AnioSeleccionado.HasValue || b.Gestion == AnioSeleccionado.Value;
            bool coincideMes = MesSeleccionado == null || b.Mes == MesSeleccionado.Numero;

            return coincideAnio && coincideMes;
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
