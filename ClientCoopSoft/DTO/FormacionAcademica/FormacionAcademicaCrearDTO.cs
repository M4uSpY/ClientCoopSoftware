using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.FormacionAcademica
{
    public class FormacionAcademicaCrearDTO
    {
        public int IdTrabajador { get; set; }
        public string NivelEstudios { get; set; } = string.Empty;
        public string TituloObtenido { get; set; } = string.Empty;
        public string Institucion { get; set; } = string.Empty;
        public int AnioGraduacion { get; set; }
        public string? NroRegistroProfesional { get; set; }
    }

}
