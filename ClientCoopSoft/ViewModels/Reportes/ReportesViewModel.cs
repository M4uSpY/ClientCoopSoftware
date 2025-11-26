using ClientCoopSoft.DTO.Historicos;
using ClientCoopSoft.ViewModels.Historicos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.Reportes
{
    public partial class ReportesViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly DashboardViewModel _dashboard;

        public ReportesViewModel(ApiClient apiClient, DashboardViewModel dashboard)
        {
            _apiClient = apiClient;
            _dashboard = dashboard;
        }

        [RelayCommand]
        private async Task AbrirHistoricoFaltas()
        {
            var listaHFVM = new HistoricoFaltaViewModel(_apiClient, _dashboard);
            await listaHFVM.CargarHistorialFaltaAsync();
            _dashboard.CurrentView = listaHFVM;
        }
        [RelayCommand]
        private async Task AbrirHistoricoPersonas()
        {
            var listaHPVM = new HistoricoPersonaViewModel(_apiClient, _dashboard);
            await listaHPVM.CargarHistorialPersonaAsync();
            _dashboard.CurrentView = listaHPVM;
        }
        [RelayCommand]
        private async Task AbrirHistoricoTrabajadores()
        {
            var listaHTVM = new HistoricoTrabajadorViewModel(_apiClient, _dashboard);
            await listaHTVM.CargarHistorialTrabajadorAsync();
            _dashboard.CurrentView = listaHTVM;
        }
        [RelayCommand]
        private async Task AbrirHistoricoUsuarios()
        {
            var listaHUVM = new HistoricoUsuarioViewModel(_apiClient, _dashboard);
            await listaHUVM.CargarHistorialUsuarioAsync();
            _dashboard.CurrentView = listaHUVM;
        }
    }
}
