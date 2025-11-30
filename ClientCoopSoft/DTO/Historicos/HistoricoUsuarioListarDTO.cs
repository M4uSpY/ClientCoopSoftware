using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Historicos
{
    public class HistoricoUsuarioListarDTO
    {
        public int IdHistorico { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioModifico { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }
        public string Accion { get; set; } = string.Empty;

        public string Campo { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorActual { get; set; }
    }
}
