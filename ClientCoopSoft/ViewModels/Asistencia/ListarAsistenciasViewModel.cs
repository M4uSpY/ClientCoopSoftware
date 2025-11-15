using ClientCoopSoft.DTO.Asistencia;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ClientCoopSoft.ViewModels.Asistencia
{
    public partial class ListarAsistenciasViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<AsistenciaListarDTO> asistencias = new();

        public ListarAsistenciasViewModel(ApiClient apiCient)
        {
            _apiClient = apiCient;
        }

        public async Task CargarAsistenciasAsync()
        {
            var list = await _apiClient.ObtenerListaAsistencias();
            if(list != null)
            {
                Asistencias = new ObservableCollection<AsistenciaListarDTO>(list);
            }
        }

    }
}
