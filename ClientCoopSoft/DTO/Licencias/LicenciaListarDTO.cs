using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Licencias
{
    public class LicenciaListarDTO
    {
        public int IdLicencia { get; set; }
        public int IdTrabajador { get; set; }

        public string CI { get; set; } = string.Empty;
        public string ApellidosNombres { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public string TipoLicencia { get; set; } = string.Empty;
        public string EstadoLicencia { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public decimal CantidadJornadas { get; set; }

        public string Motivo { get; set; } = string.Empty;
        public string? Observacion { get; set; }

        public bool TieneArchivoJustificativo { get; set; }
    }
}
