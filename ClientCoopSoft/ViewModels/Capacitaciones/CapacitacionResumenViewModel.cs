using ClientCoopSoft.DTO.Capacitaciones;
using ClientCoopSoft.DTO.FormacionAcademica;
using ClientCoopSoft.ViewModels.FormacionAcademica;
using ClientCoopSoft.Views.Capacitaciones;
using ClientCoopSoft.Views.FormacionAcademica;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Capacitaciones
{
    public partial class CapacitacionResumenViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        [ObservableProperty]
        private ObservableCollection<CapacitacionResumenDTO> capacitaciones = new();

        [ObservableProperty]
        private string tituloCabecera = "CAPACITACIONES";

        public CapacitacionResumenViewModel(int idTrabajador, ApiClient apiClient)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
            _ = CargarCapacitacionesAsync();
        }

        private async Task CargarCapacitacionesAsync()
        {
            if(_idTrabajador == 0)
            {
                TituloCabecera = "CAPACITACIONES (sin trabajador)";
                Capacitaciones.Clear();
                return;
            }
            var lista = await _apiClient.ObtenerCapacitacionesPorTrabajadorAsync(_idTrabajador);
            Capacitaciones = new ObservableCollection<CapacitacionResumenDTO>(lista ?? new());
        }

        [RelayCommand]
        private async Task NuevaCapacitacionAsync()
        {
            if (_idTrabajador == 0)
            {
                return;
            }
            var ventana = new CapacitacionCrearView
            {
                Owner = Application.Current.MainWindow
            };

            var vm = new CapacitacionCrearViewModel(_apiClient, _idTrabajador);
            ventana.DataContext = vm;

            vm.CerrarVentanaSolicitado += (s, ok) =>
            {
                ventana.DialogResult = ok;
                ventana.Close();
            };
            var result = ventana.ShowDialog();
            if (result == true)
            {
                await CargarCapacitacionesAsync();
            }
        }

        [RelayCommand]
        private async Task EditarCapacitacionAsync(CapacitacionResumenDTO? seleccionada)
        {
            if (seleccionada == null)
                return;

            var dto = await _apiClient.ObtenerCapacitacionPorIdAsync(seleccionada.IdCapacitacion);
            if (dto == null)
                return;

            var ventana = new CapacitacionEditarView
            {
                Owner = Application.Current.MainWindow
            };

            var vm = new CapacitacionEditarViewModel(_apiClient, dto);
            ventana.DataContext = vm;

            vm.CerrarVentanaSolicitado += (s, ok) =>
            {
                ventana.DialogResult = ok;
                ventana.Close();
            };

            var result = ventana.ShowDialog();
            if (result == true)
            {
                await CargarCapacitacionesAsync();
            }
        }

    }
}
