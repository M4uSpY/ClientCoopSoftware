using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.Faltas
{
    public partial class ListaFaltasViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public ListaFaltasViewModel(ApiClient api)
        {
            _api = api;
        }
    }
}
