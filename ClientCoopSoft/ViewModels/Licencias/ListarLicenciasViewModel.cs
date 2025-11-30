using ClientCoopSoft.DTO.Licencias;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClientCoopSoft.ViewModels.Licencias
{
    public partial class ListarLicenciasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Action? _onVolver;

        [ObservableProperty]
        private ObservableCollection<LicenciaListarDTO> licenciasLista = new();

        [ObservableProperty]
        private ICollectionView licenciasView;

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        public ListarLicenciasViewModel(ApiClient apiClient, Action? onVolver = null)
        {
            _apiClient = apiClient;
            _onVolver = onVolver;

            LicenciasView = CollectionViewSource.GetDefaultView(LicenciasLista);
            if (LicenciasView != null)
            {
                LicenciasView.Filter = LicenciasFilter;
            }
        }

        public async Task CargarLicenciasListaAsync()
        {
            var list = await _apiClient.ObtenerLicenciasAsync();
            if (list != null)
            {
                LicenciasLista = new ObservableCollection<LicenciaListarDTO>(list);
            }
        }

        partial void OnLicenciasListaChanged(ObservableCollection<LicenciaListarDTO> value)
        {
            LicenciasView = CollectionViewSource.GetDefaultView(value);
            if (LicenciasView != null)
            {
                LicenciasView.Filter = LicenciasFilter;
                LicenciasView.Refresh();
            }
        }

        // Cuando cambia el texto del buscador
        partial void OnTextoBusquedaChanged(string value)
        {
            LicenciasView?.Refresh();
        }

        // ====== FILTRO ======
        private bool LicenciasFilter(object obj)
        {
            if (obj is not LicenciaListarDTO l)
                return false;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
                return true;

            var filtro = TextoBusqueda.Trim().ToLower();

            string fechaInicio = l.FechaInicio.ToString("yyyy-MM-dd");
            string fechaFin = l.FechaFin.ToString("yyyy-MM-dd");
            string horaInicio = l.HoraInicio.ToString();   // seguro para cualquier tipo
            string horaFin = l.HoraFin.ToString();
            string jornadas = l.CantidadJornadas.ToString();

            bool coincideCI = (l.CI ?? string.Empty).ToLower().Contains(filtro);
            bool coincideNombre = (l.ApellidosNombres ?? string.Empty).ToLower().Contains(filtro);
            bool coincideCargo = (l.Cargo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideTipo = (l.TipoLicencia ?? string.Empty).ToLower().Contains(filtro);
            bool coincideEstado = (l.Estado ?? string.Empty).ToLower().Contains(filtro);
            bool coincideFechaInicio = fechaInicio.Contains(filtro);
            bool coincideFechaFin = fechaFin.Contains(filtro);
            bool coincideHoraInicio = horaInicio.ToLower().Contains(filtro);
            bool coincideHoraFin = horaFin.ToLower().Contains(filtro);
            bool coincideJornadas = jornadas.Contains(filtro);
            bool coincideMotivo = (l.Motivo ?? string.Empty).ToLower().Contains(filtro);

            return coincideCI
                || coincideNombre
                || coincideCargo
                || coincideTipo
                || coincideEstado
                || coincideFechaInicio
                || coincideFechaFin
                || coincideHoraInicio
                || coincideHoraFin
                || coincideJornadas
                || coincideMotivo;
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

        [RelayCommand]
        private async Task EliminarLicencia(LicenciaListarDTO? licencia)
        {
            if (licencia is null)
                return;

            var mensaje = $"¿Está seguro que desea ELIMINAR la licencia N° {licencia.IdLicencia} " +
                          $"de {licencia.ApellidosNombres} del {licencia.FechaInicio:dd/MM/yyyy} " +
                          $"al {licencia.FechaFin:dd/MM/yyyy}?\n\n" +
                          "Solo se pueden eliminar licencias en estado 'Pendiente'.";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            var (ok, error) = await _apiClient.EliminarLicenciaAsync(licencia.IdLicencia);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo eliminar la licencia. Intente nuevamente."
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
