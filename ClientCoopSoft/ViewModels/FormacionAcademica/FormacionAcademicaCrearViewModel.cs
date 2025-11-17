using ClientCoopSoft.DTO.FormacionAcademica;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.FormacionAcademica
{
    public partial class FormacionAcademicaCrearViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly int _idTrabajador;

        public event EventHandler<bool>? CerrarVentanaSolicitado;

        [ObservableProperty] private string nivelEstudios = string.Empty;
        [ObservableProperty] private string tituloObtenido = string.Empty;
        [ObservableProperty] private string institucion = string.Empty;
        [ObservableProperty] private int anioGraduacion = DateTime.Now.Year;
        [ObservableProperty] private string? nroRegistroProfesional;

        public string TituloVentana => "Nueva formación académica";

        public FormacionAcademicaCrearViewModel(ApiClient apiClient, int idTrabajador)
        {
            _apiClient = apiClient;
            _idTrabajador = idTrabajador;
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            var dto = new FormacionAcademicaCrearDTO
            {
                IdTrabajador = _idTrabajador,
                NivelEstudios = NivelEstudios,
                TituloObtenido = TituloObtenido,
                Institucion = Institucion,
                AnioGraduacion = AnioGraduacion,
                NroRegistroProfesional = NroRegistroProfesional
            };

            var creada = await _apiClient.CrearFormacionAcademicaAsync(dto);
            if (creada != null)
            {
                CerrarVentanaSolicitado?.Invoke(this, true);
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            CerrarVentanaSolicitado?.Invoke(this, false);
        }
    }
}
