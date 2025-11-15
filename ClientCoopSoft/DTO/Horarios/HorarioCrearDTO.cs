using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Horarios
{
    public class HorarioCrearDTO
    {
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraEntrada { get; set; } = string.Empty;
        public string HoraSalida { get; set; } = string.Empty;
    }
}
