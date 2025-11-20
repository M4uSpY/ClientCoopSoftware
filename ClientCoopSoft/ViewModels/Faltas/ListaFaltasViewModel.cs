using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.DTO.Faltas;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
