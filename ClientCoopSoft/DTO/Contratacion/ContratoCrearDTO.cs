using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Contratacion
{
    public class ContratoCrearDTO
    {
        public int IdTrabajador { get; set; }
        public int IdTipoContrato { get; set; }
        public int IdPeriodoPago { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public byte[] ArchivoPdf { get; set; } = Array.Empty<byte>();
    }
}
