using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Asistencia
{
    public class AsistenciaListarDTO
    {
        public int IdAsistencia { get; set; }
        public string CI { get; set; } = string.Empty;
        public string ApellidosNombres { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Oficina { get; set; } = string.Empty;
        public bool EsEntrada { get; set; }
    }
}
