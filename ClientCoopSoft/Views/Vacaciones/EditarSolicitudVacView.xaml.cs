using ClientCoopSoft.ViewModels.Vacaciones;
using System.Windows;

namespace ClientCoopSoft.Views.Vacaciones
{
    /// <summary>
    /// Lógica de interacción para EditarSolicitudVacView.xaml
    /// </summary>
    public partial class EditarSolicitudVacView : Window
    {
        public EditarSolicitudVacView(EditarSolicitudVacacionViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            vm.SolicitudEditada += () =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}
