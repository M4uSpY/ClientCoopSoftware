using ClientCoopSoft.DTO.Extras;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.LogsAcceso
{
    public partial class LogsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<LogsAccesoDTO> listaLogs = new();

        public LogsViewModel(ApiClient api)
        {
            _apiClient = api;
        }

        public async Task CargarLogsAccesoAsync()
        {
            var list = await _apiClient.ObtenerListaLogsAcceso();
            if(list != null)
            {
                ListaLogs = new ObservableCollection<LogsAccesoDTO>(list);
            }
        }
    }
}
