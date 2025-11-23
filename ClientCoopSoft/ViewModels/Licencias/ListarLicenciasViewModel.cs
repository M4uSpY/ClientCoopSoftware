using ClientCoopSoft.DTO.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientCoopSoft.ViewModels.Licencias
{
    public partial class ListarLicenciasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Action? _onVolver;

        [ObservableProperty]
        private ObservableCollection<LicenciaListarDTO> licenciasLista = new();

        public ListarLicenciasViewModel(ApiClient apiClient, Action? onVolver = null)
        {
            _apiClient = apiClient;
            _onVolver = onVolver;
        }

        public async Task CargarLicenciasListaAsync()
        {
            var list = await _apiClient.ObtenerLicenciasAsync();
            if (list != null)
            {
                LicenciasLista = new ObservableCollection<LicenciaListarDTO>(list);
            }
        }

        [RelayCommand]
        private void Volver()
        {
            _onVolver?.Invoke();
        }

        [RelayCommand]
        private async Task AprobarLicencia(LicenciaListarDTO? licencia)
        {
            if (licencia is null)
                return;

            var mensaje = $"¿Está seguro que desea APROBAR la licencia N° {licencia.IdLicencia} " +
                          $"de {licencia.ApellidosNombres} del {licencia.FechaInicio:dd/MM/yyyy} " +
                          $"al {licencia.FechaFin:dd/MM/yyyy}?";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar aprobación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            var (ok, error) = await _apiClient.AprobarLicenciaAsync(licencia.IdLicencia);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo aprobar la licencia. Intente nuevamente."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await CargarLicenciasListaAsync();
        }

        [RelayCommand]
        private async Task RechazarLicencia(LicenciaListarDTO? licencia)
        {
            if (licencia is null)
                return;

            var mensaje = $"¿Está seguro que desea RECHAZAR la licencia N° {licencia.IdLicencia} " +
                          $"de {licencia.ApellidosNombres} del {licencia.FechaInicio:dd/MM/yyyy} " +
                          $"al {licencia.FechaFin:dd/MM/yyyy}?";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar rechazo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            var (ok, error) = await _apiClient.RechazarLicenciaAsync(licencia.IdLicencia);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo rechazar la licencia. Intente nuevamente."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await CargarLicenciasListaAsync();
        }
    }
}
