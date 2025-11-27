using ClientCoopSoft.DTO.VacacionesPermisos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace ClientCoopSoft.ViewModels.VacacionesPemisos
{
    public partial class ListarSolicitudesViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Action? _onVolver;

        [ObservableProperty]
        private ObservableCollection<SolicitudVacPermListarDTO> solicitudesLista = new();

        [ObservableProperty]
        private ICollectionView solicitudesView;

        // 👉 Texto del buscador
        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        public ListarSolicitudesViewModel(ApiClient apiClient, Action? onVolver = null)
        {
            _apiClient = apiClient;
            _onVolver = onVolver;

            SolicitudesView = CollectionViewSource.GetDefaultView(SolicitudesLista);
            if (SolicitudesView != null)
            {
                SolicitudesView.Filter = SolicitudesFilter;
            }
        }

        public async Task CargarSolicitudesListaAsync()
        {
            var list = await _apiClient.ObtenerListaVacacionesPermisosAsync();
            if (list != null)
            {
                SolicitudesLista = new ObservableCollection<SolicitudVacPermListarDTO>(list);
            }
        }

        partial void OnSolicitudesListaChanged(ObservableCollection<SolicitudVacPermListarDTO> value)
        {
            SolicitudesView = CollectionViewSource.GetDefaultView(value);
            if (SolicitudesView != null)
            {
                SolicitudesView.Filter = SolicitudesFilter;
                SolicitudesView.Refresh();
            }
        }

        // Cuando cambia el texto del buscador
        partial void OnTextoBusquedaChanged(string value)
        {
            SolicitudesView?.Refresh();
        }

        // ====== FILTRO ======
        private bool SolicitudesFilter(object obj)
        {
            if (obj is not SolicitudVacPermListarDTO s)
                return false;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
                return true;

            var filtro = TextoBusqueda.Trim().ToLower();

            string fechaInicio = s.FechaInicio.ToString("yyyy-MM-dd");
            string fechaFin = s.FechaFin.ToString("yyyy-MM-dd");

            bool coincideCI = (s.CI ?? string.Empty).ToLower().Contains(filtro);
            bool coincideNombre = (s.ApellidosNombres ?? string.Empty).ToLower().Contains(filtro);
            bool coincideCargo = (s.Cargo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideTipo = (s.Tipo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideMotivo = (s.Motivo ?? string.Empty).ToLower().Contains(filtro);
            bool coincideFechaInicio = fechaInicio.Contains(filtro);
            bool coincideFechaFin = fechaFin.Contains(filtro);
            bool coincideEstado = (s.Estado ?? string.Empty).ToLower().Contains(filtro);

            return coincideCI
                || coincideNombre
                || coincideCargo
                || coincideTipo
                || coincideMotivo
                || coincideFechaInicio
                || coincideFechaFin
                || coincideEstado;
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
                return;

            var (ok, error) = await _apiClient.AprobarSolicitudAsync(solicitud.IdSolicitud);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo aprobar la solicitud. Intente nuevamente."
                        : error,
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

            var (ok, error) = await _apiClient.RechazarSolicitudAsync(solicitud.IdSolicitud);

            if (!ok)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(error)
                        ? "No se pudo rechazar la solicitud. Intente nuevamente."
                        : error,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            await CargarSolicitudesListaAsync();
        }


    }
}
