using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
