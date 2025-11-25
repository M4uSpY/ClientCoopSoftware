using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class PlanillaAportesFilaModel
    {
        public int Id { get; set; }

        public string CarnetIdentidad { get; set; } = string.Empty;
        public string ApellidosNombres { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Ocupacion { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }

        public int DiasPagados { get; set; }

        public decimal TotalGanado { get; set; }

        public decimal Cps10 { get; set; }
        public decimal RiesgoPrima171 { get; set; }
        public decimal Provivienda2 { get; set; }
        public decimal AporteSolidario35 { get; set; }
        public decimal TotalAportes { get; set; }
    }
}
