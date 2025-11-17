using ClientCoopSoft.DTO.FormacionAcademica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Extras
{
    public class TrabajadorPersonaDTO
    {
        public int IdTrabajador { get; set; }
        public string CodigoTrabajador { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public bool EsActivo { get; set; }

        // Título principal (más reciente)
        public string? TituloObtenido { get; set; }

        // Lista de formaciones para las cards
        public List<FormacionAcademicaResumenDTO> Formaciones { get; set; } = new();
    }
}
