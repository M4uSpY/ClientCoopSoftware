using ClientCoopSoft.Models;
using ClientCoopSoft.Views.Trabajadores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Trabajadores
{
    public partial class ListarTrabajadoresViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<Trabajador> trabajadores = new();

        public ListarTrabajadoresViewModel(ApiClient api)
        {
            _apiClient = api;
        }

        public async Task CargarTrabajadoresAsync()
        {
            var list = await _apiClient.ObtenerTrabajadoresAsync();
            if(list != null)
            {
                Trabajadores = new ObservableCollection<Trabajador>(list);
            }
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
    }
}
