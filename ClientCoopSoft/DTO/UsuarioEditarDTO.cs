using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO
{
    public class UsuarioEditarDTO
    {
        public int IdPersona { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string? PasswordNueva { get; set; }  // va vacío si no cambian la contraseña
    }
}
