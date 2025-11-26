using ClientCoopSoft.DTO.Historicos;
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
    public partial class HistoricoFaltaViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        [ObservableProperty]
        private ObservableCollection<HistoricoFaltaListarDTO> historialf = new();

        public HistoricoFaltaViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        public async Task CargarHistorialFaltaAsync()
        {
            var list = await _apiClient.ObtenerHistorialFaltasAsync();
            if (list != null)
            {
                Historialf = new ObservableCollection<HistoricoFaltaListarDTO>(list);
            }
        }

        [RelayCommand]
        private void Volver()
        {
            _dashboard.CurrentView = new ReportesViewModel(_apiClient, _dashboard);
        }
    }
}
