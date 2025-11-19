using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.ViewModels.Planillas
{
    public partial class PlanillaSueldosSalariosViewModel : ObservableObject
    {
        private readonly ApiClient _api;

        public PlanillaSueldosSalariosViewModel(ApiClient api)
        {
            _api = api; 
        }
    }
}
