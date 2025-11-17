using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.VacacionesPermisos
{
    public class SolicitudVacPermListarDTO
    {
        public int IdSolicitud { get; set; }
        public string CI { get; set; } = string.Empty;
        public string ApellidosNombres { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
