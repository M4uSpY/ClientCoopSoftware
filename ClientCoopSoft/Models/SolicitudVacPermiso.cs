using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class SolicitudVacPermiso
    {
        public int IdSolicitud { get; set; }
        public string Trabajador { get; set; } = string.Empty;
        public string TipoSolicitud { get; set; } = string.Empty;
        public string EstadoSolicitud { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

}
