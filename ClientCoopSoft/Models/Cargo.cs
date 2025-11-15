using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class Cargo
    {
        public int IdCargo { get; set; }
        public int IdOficina { get; set; }
        public string NombreCargo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
