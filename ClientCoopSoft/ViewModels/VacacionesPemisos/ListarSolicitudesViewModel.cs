using ClientCoopSoft.DTO.VacacionesPermisos;
using ClientCoopSoft.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.VacacionesPemisos
{
    public partial class ListarSolicitudesViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Action? _onVolver;

        [ObservableProperty]
        private ObservableCollection<SolicitudVacPermListarDTO> solicitudesLista = new();

        public ListarSolicitudesViewModel(ApiClient apiClient, Action? onVolver = null)
        {
            _apiClient = apiClient;
            _onVolver = onVolver;
        }

        public async Task CargarSolicitudesListaAsync()
        {
            var list = await _apiClient.ObtenerListaVacacionesPermisosAsync();
            if (list != null)
            {
                SolicitudesLista = new ObservableCollection<SolicitudVacPermListarDTO>(list);
            }
        }

        [RelayCommand]
        private void Volver()
        {
            _onVolver?.Invoke();  
        }
        [RelayCommand]
        private async Task AprobarSolicitud(SolicitudVacPermListarDTO? solicitud)
        {
            if (solicitud is null)
                return;

            var mensaje = $"¿Está seguro que desea APROBAR la solicitud N° {solicitud.IdSolicitud} " +
                          $"de {solicitud.ApellidosNombres} del {solicitud.FechaInicio:dd/MM/yyyy} " +
                          $"al {solicitud.FechaFin:dd/MM/yyyy}?";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar aprobación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return; // el usuario canceló

            var ok = await _apiClient.AprobarSolicitudAsync(solicitud.IdSolicitud);

            if (!ok)
            {
                MessageBox.Show(
                    "No se pudo aprobar la solicitud. Intente nuevamente.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await CargarSolicitudesListaAsync();
        }


        [RelayCommand]
        private async Task RechazarSolicitud(SolicitudVacPermListarDTO? solicitud)
        {
            if (solicitud is null)
                return;

            var mensaje = $"¿Está seguro que desea RECHAZAR la solicitud N° {solicitud.IdSolicitud} " +
                          $"de {solicitud.ApellidosNombres} del {solicitud.FechaInicio:dd/MM/yyyy} " +
                          $"al {solicitud.FechaFin:dd/MM/yyyy}?";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar rechazo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            var ok = await _apiClient.RechazarSolicitudAsync(solicitud.IdSolicitud);

            if (!ok)
            {
                MessageBox.Show(
                    "No se pudo rechazar la solicitud. Intente nuevamente.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await CargarSolicitudesListaAsync();
        }

    }
}
