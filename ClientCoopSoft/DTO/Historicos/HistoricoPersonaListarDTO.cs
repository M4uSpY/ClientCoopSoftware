using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Historicos
{
    public class HistoricoPersonaListarDTO
    {
        public int IdHistorico { get; set; }
        public int IdPersona { get; set; }
        public string UsuarioModifico { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string ApartadosModificados { get; set; } = string.Empty;
    }
}
