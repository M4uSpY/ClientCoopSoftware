using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class BoletaPagoModel
    {
        public int IdPlanilla { get; set; }
        public int Gestion { get; set; }
        public int Mes { get; set; }
        public string MesNombre { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public int DiasTrabajados { get; set; }

        public decimal TotalGanado { get; set; }
        public decimal LiquidoPagable { get; set; }
    }
}
