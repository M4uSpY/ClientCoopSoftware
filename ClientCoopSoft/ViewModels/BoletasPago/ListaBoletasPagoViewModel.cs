using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.BoletasPago
{
    public partial class ListaBoletasPagoViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public ListaBoletasPagoViewModel(ApiClient api)
        {
            _api = api;
        }
    }
}
