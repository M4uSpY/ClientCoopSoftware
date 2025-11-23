using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.Models
{
    public class PlanillaResumenModel
    {
        public int IdPlanilla { get; set; }
        public int IdTipoPlanilla { get; set; }
        public int Gestion { get; set; }
        public int Mes { get; set; }
        public DateTime PeriodoDesde { get; set; }
        public DateTime PeriodoHasta { get; set; }
        public bool EstaCerrada { get; set; }
    }
}
