using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Licencias
{
    public class TipoLicencia
    {
        public int IdClasificador { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string ValorCategoria { get; set; } = string.Empty;
    }
}
