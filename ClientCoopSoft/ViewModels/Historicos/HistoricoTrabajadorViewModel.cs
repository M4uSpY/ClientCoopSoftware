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
    public partial class HistoricoTrabajadorViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        [ObservableProperty]
        private ObservableCollection<HistoricoTrabajadorListarDTO> historialt = new();

        public HistoricoTrabajadorViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        public async Task CargarHistorialTrabajadorAsync()
        {
            var list = await _apiClient.ObtenerHistorialTrabajadoresAsync();
            if (list != null)
            {
                Historialt = new ObservableCollection<HistoricoTrabajadorListarDTO>(list);
            }
        }
        [RelayCommand]
        private void Volver()
        {
            _dashboard.CurrentView = new ReportesViewModel(_apiClient, _dashboard);
        }
    }
}
