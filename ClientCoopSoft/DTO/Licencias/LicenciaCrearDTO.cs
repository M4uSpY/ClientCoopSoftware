using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Licencias
{
    public class LicenciaCrearDTO
    {
        public int IdTrabajador { get; set; }
        public int IdTipoLicencia { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public string Motivo { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
