using ClientCoopSoft.DTO.Horarios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Trabajadores
{
    public class TrabajadorCrearDTO
    {
        public int IdPersona { get; set; }
        public decimal HaberBasico { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int IdCargo { get; set; }
        public List<HorarioCrearDTO> Horarios { get; set; } = new List<HorarioCrearDTO>();
    }
}
