using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Capacitaciones
{
    public class CapacitacionResumenDTO
    {
        public int IdCapacitacion { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public byte[]? ArchivoCertificado { get; set; }
    }
}
