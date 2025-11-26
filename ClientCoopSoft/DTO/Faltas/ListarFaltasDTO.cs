using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCoopSoft.DTO.Faltas
{
    public class ListarFaltasDTO
    {
        public int IdFalta { get; set; }
        public string CI { get; set; } = string.Empty;
        public string ApellidosNombres { get; set; } = string.Empty;

        // Tabla clasificador
        public string Tipo { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public bool TieneArchivoJustificativo { get; set; }
        public string EstadoArchivoJustificativo { get; set; } = string.Empty;
    }
}
