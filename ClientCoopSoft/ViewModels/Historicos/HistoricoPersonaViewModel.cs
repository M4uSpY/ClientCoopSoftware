using ClientCoopSoft.DTO.Historicos;
using ClientCoopSoft.Models;
using ClientCoopSoft.ViewModels.Reportes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.Historicos
{
    public partial class HistoricoPersonaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        [ObservableProperty]
        private ObservableCollection<HistoricoPersonaListarDTO> historialp = new();

        public HistoricoPersonaViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        public async Task CargarHistorialPersonaAsync()
        {
            var list = await _apiClient.ObtenerHistorialPersonasAsync();
            if (list != null)
            {
                Historialp = new ObservableCollection<HistoricoPersonaListarDTO>(list);
            }
        }
        [RelayCommand]
        private void Volver()
        {
            _dashboard.CurrentView = new ReportesViewModel(_apiClient, _dashboard);
        }
    }
}
