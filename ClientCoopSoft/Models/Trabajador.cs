using ClientCoopSoft.DTO.Horarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class Trabajador
    {
        public int IdTrabajador { get; set; }
        public string CI { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public int IdNacionalidad { get; set; }
        public string Nacionalidad { get; set; } = string.Empty;
        public bool Genero { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public string NombreOficina { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int IdPersona { get; set; }
        public decimal HaberBasico { get; set; }
        public DateTime FechaIngreso { get; set; }

        public List<HorarioDTO> Horarios { get; set; } = new List<HorarioDTO>();
    }
}
