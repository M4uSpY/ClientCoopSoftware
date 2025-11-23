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

        [RelayCommand]
        private async Task VerJustificativo(LicenciaListarDTO? licencia)
        {
            if (licencia is null)
                return;

            if (!licencia.TieneArchivoJustificativo)
            {
                MessageBox.Show("Esta licencia no tiene archivo justificativo adjunto.",
                    "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var (ok, archivo, error) =
                await _apiClient.DescargarJustificativoLicenciaAsync(licencia.IdLicencia);

            if (!ok || archivo == null || archivo.Length == 0)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo descargar el archivo justificativo."
                        : error,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"Justificativo_Licencia_{licencia.IdLicencia}.pdf"
                );

                System.IO.File.WriteAllBytes(tempPath, archivo);

                // Abrir el archivo PDF con visor predeterminado
                var psi = new System.Diagnostics.ProcessStartInfo(tempPath)
                {
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"El archivo se descargó, pero no se pudo abrir automáticamente.\nError: {ex.Message}",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

    }
}
