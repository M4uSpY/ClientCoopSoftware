using ClientCoopSoft.DTO.FormacionAcademica;
using ClientCoopSoft.Models;
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

namespace ClientCoopSoft.ViewModels.FormacionAcademica
{
    public partial class FormacionAcademicaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient; // ya lo tienes definido en tu proyecto
        private readonly int _idTrabajador;

        [ObservableProperty]
        private ObservableCollection<FormacionAcademicaResumenDTO> formaciones = new();

        [ObservableProperty]
        private string tituloCabecera = "FORMACIÓN ACADÉMICA";

        public FormacionAcademicaViewModel(int idTrabajador, ApiClient apiClient)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;

            _ = CargarFormacionesAsync();
        }

        private async Task CargarFormacionesAsync()
        {
            if (_idTrabajador == 0)
            {
                TituloCabecera = "FORMACIÓN ACADÉMICA (sin trabajador)";
                Formaciones.Clear();
                return;
            }

            var lista = await _apiClient.ObtenerFormacionesPorTrabajadorAsync(_idTrabajador);
            Formaciones = new ObservableCollection<FormacionAcademicaResumenDTO>(lista ?? new());
        }

        [RelayCommand]
        private async Task NuevaFormacionAsync()
        {
            if (_idTrabajador == 0)
                return;

            var ventana = new FormacionAcademicaCrearView
            {
                Owner = Application.Current.MainWindow
            };

            var vm = new FormacionAcademicaCrearViewModel(_apiClient, _idTrabajador);
            ventana.DataContext = vm;

            vm.CerrarVentanaSolicitado += (s, ok) =>
            {
                ventana.DialogResult = ok;
                ventana.Close();
            };

            var result = ventana.ShowDialog();
            if (result == true)
            {
                await CargarFormacionesAsync();
            }
        }

        [RelayCommand]
        private async Task EditarFormacionAsync(FormacionAcademicaResumenDTO? seleccionada)
        {
            if (seleccionada == null)
                return;

            var dto = await _apiClient.ObtenerFormacionPorIdAsync(seleccionada.IdFormacion);
            if (dto == null)
                return;

            var ventana = new FormacionAcademicaEditarView
            {
                Owner = Application.Current.MainWindow
            };

            var vm = new FormacionAcademicaEditarViewModel(_apiClient, dto);
            ventana.DataContext = vm;

            vm.CerrarVentanaSolicitado += (s, ok) =>
            {
                ventana.DialogResult = ok;
                ventana.Close();
            };

            var result = ventana.ShowDialog();
            if (result == true)
            {
                await CargarFormacionesAsync();
            }
        }
    }
}
