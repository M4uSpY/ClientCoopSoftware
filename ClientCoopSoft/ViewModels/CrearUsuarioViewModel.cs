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
    public partial class CrearUsuarioViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty] private string nombreUsuario = string.Empty;
        [ObservableProperty] private ObservableCollection<Persona> personas = new();
        [ObservableProperty] private Persona? personaSeleccionada;
        [ObservableProperty] private ObservableCollection<Rol> roles = new();
        [ObservableProperty] private Rol? rolSeleccionado;

        public CrearUsuarioViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            _ = CargarPersonasAsync();
            _ = CargarRolesAsync();
        }

        private async Task CargarPersonasAsync()
        {
            var lista = await _apiClient.ObtenerPersonasAsync() ?? new List<Persona>();
            Personas = new ObservableCollection<Persona>(lista);
        }

        private async Task CargarRolesAsync()
        {
            var lista = await _apiClient.ObtenerRolesAsync() ?? new List<Rol>();
            Roles = new ObservableCollection<Rol>(lista);
        }

        [RelayCommand]
        private async Task GuardarAsync(Window window)
        {
            var pwdBox = window.FindName("PwdBox") as System.Windows.Controls.PasswordBox;
            var password = pwdBox?.Password ?? string.Empty;

            if (PersonaSeleccionada == null)
            {
                MessageBox.Show("Selecciona una persona.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (RolSeleccionado == null)
            {
                MessageBox.Show("Selecciona un rol.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MessageBox.Show("Ingresa un nombre de usuario.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingresa una contraseña.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new UsuarioCrearDTO
            {
                IdPersona = PersonaSeleccionada.IdPersona,
                NombreUsuario = NombreUsuario,
                Password = password,
                IdRol = RolSeleccionado.IdRol,
                EstadoUsuario = true
            };

            bool exito = await _apiClient.CrearUsuarioAsync(dto);
            if (exito)
            {
                MessageBox.Show("Usuario creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Error al crear el usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
