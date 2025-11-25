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

namespace ClientCoopSoft.ViewModels.Planillas
{
    public partial class PlanillaAPatronalesBSocialesViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public PlanillaAPatronalesBSocialesViewModel(ApiClient api)
        {
            _api = api;
            Gestion = DateTime.Now.Year;
            Mes = DateTime.Now.Month;
            Aportes = new ObservableCollection<PlanillaAportesFilaModel>();

            Instrucciones =
                "1) Primero genere y calcule la Planilla de Sueldos y Salarios.\n" +
                "2) Tome el Id de esa planilla.\n" +
                "3) Ingréselo en 'Id Planilla Sueldos' y presione 'Cargar aportes'.";
        }

        // ====== PROPIEDADES ======
        [ObservableProperty] private int gestion;
        [ObservableProperty] private int mes;
        [ObservableProperty] private int idPlanillaSueldos;
        [ObservableProperty] private string instrucciones = string.Empty;
        [ObservableProperty] private ObservableCollection<PlanillaAportesFilaModel> aportes;

        // ====== COMANDOS ======

        [RelayCommand]
        private async Task CargarAportesAsync()
        {
            if (IdPlanillaSueldos <= 0)
            {
                MessageBox.Show("Indique un Id de planilla de sueldos válido.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lista = await _api.ObtenerDatosPlanillaAportesAsync(IdPlanillaSueldos);

                if (lista == null)
                {
                    MessageBox.Show("No se pudieron obtener los datos de aportes. " +
                                    "Verifique que la planilla de sueldos exista y esté calculada.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Aportes = new ObservableCollection<PlanillaAportesFilaModel>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la planilla de aportes: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Limpiar()
        {
            Aportes.Clear();
        }
    }
}
