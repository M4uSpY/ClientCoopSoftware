using ClientCoopSoft.DTO;
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

namespace ClientCoopSoft.ViewModels
{
    public partial class EditarUsuarioViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly Usuario _usuario;

        [ObservableProperty] private string nombreUsuario;
        [ObservableProperty] private string password;
        [ObservableProperty] private ObservableCollection<Persona> personas = new();
        [ObservableProperty] private Persona? personaSeleccionada;
        [ObservableProperty] private ObservableCollection<Rol> roles = new();
        [ObservableProperty] private Rol? rolSeleccionado;



        public EditarUsuarioViewModel(Usuario usuario, ApiClient apiClient)
        {
            _usuario = usuario;
            _apiClient = apiClient;

            NombreUsuario = usuario.NombreUsuario;
            Password = usuario.Password;
            _ = CargarPersonasAsync(usuario.IdPersona);
            _ = CargarRolesAsync(usuario.Rol);
        }
        private async Task CargarPersonasAsync(int idPersona)
        {
            var lista = await _apiClient.ObtenerPersonasAsync() ?? new List<Persona>();
            Personas = new ObservableCollection<Persona>(lista);
            PersonaSeleccionada = Personas.FirstOrDefault(p => p.IdPersona == idPersona);
        }
        private async Task CargarRolesAsync(string rolActual)
        {
            var lista = await _apiClient.ObtenerRolesAsync() ?? new List<Rol>();
            Roles = new ObservableCollection<Rol>(lista);
            RolSeleccionado = Roles.FirstOrDefault(r => r.NombreRol.Equals(rolActual, StringComparison.OrdinalIgnoreCase));
        }

        [RelayCommand]
        private async Task GuardarAsync(Window window)
        {
            if (PersonaSeleccionada is null)
            {
                MessageBox.Show("Debe seleccionar una persona.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RolSeleccionado is null)
            {
                MessageBox.Show("Debe seleccionar un rol.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dto = new UsuarioEditarDTO
            {
                IdPersona = PersonaSeleccionada.IdPersona,
                NombreUsuario = NombreUsuario,
                IdRol = RolSeleccionado.IdRol,
                Password = Password
            };
            bool exito = await _apiClient.ActualizarUsuarioAsync(_usuario.IdUsuario, dto);
            if (exito)
            {
                MessageBox.Show("Usuario actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Error al actualizar el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void Cancelar(Window window)
        {
            window.DialogResult = false;
            window.Close();

        }
    }
}