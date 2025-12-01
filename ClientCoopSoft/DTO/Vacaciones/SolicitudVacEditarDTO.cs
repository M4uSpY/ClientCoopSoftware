    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace ClientCoopSoft.DTO.Vacaciones
    {
        public class SolicitudVacEditarDTO
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public string? Observacion { get; set; }
        }
    }
