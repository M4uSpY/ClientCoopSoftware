using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Contratacion
{
    public class ContratoActualizarDTO
    {
        public int IdContrato { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public int IdTipoContrato { get; set; } // Clasificador
        public int IdPeriodoPago { get; set; } // Clasificador
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public byte[] ArchivoPdf { get; set; } = Array.Empty<byte>(); // para DESCARGAR CONTRATO
    }
}
