using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Huellas
{
    public class HuellaRespuestaDTO
    {
        public int IdHuella { get; set; }
        public int IdPersona { get; set; }
        public int IndiceDedo { get; set; }
        public string TemplateXml { get; set; } = string.Empty;
    }
}
