using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Historicos
{
    public class HistoricoFaltaListarDTO
    {
        public int IdHistorico { get; set; }
        public int IdFalta { get; set; }
        public string UsuarioModifico { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }
        public string Accion { get; set; } = string.Empty;

        // Nuevo modelo normalizado
        public string Campo { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorActual { get; set; }
    }

}
