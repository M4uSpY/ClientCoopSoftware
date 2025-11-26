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
    public partial class HistoricoUsuarioViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        [ObservableProperty]
        private ObservableCollection<HistoricoUsuarioListarDTO> historialu = new();

        public HistoricoUsuarioViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        public async Task CargarHistorialUsuarioAsync()
        {
            var list = await _apiClient.ObtenerHistorialUsuariosAsync();
            if (list != null)
            {
                Historialu = new ObservableCollection<HistoricoUsuarioListarDTO>(list);
            }
        }
        [RelayCommand]
        private void Volver()
        {
            _dashboard.CurrentView = new ReportesViewModel(_apiClient, _dashboard);
        }
    }
}
