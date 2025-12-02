using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Huellas
{
    public class HuellaDTO
    {
        public int IdPersona { get; set; }
        public int IndiceDedo { get; set; }    // 1, 2, etc.
        public string TemplateXml { get; set; } = string.Empty;
    }
}
