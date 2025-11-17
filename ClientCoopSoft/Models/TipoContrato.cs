using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class TipoContrato
    {
        public int IdClasificador { get; set; }
        public string ValorCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
