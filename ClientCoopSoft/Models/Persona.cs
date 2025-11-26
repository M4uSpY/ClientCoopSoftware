using ClientCoopSoft.DTO.Extras;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.Models
{
    public class Persona
    {
        public int IdPersona { get; set; }
        public string CarnetIdentidad { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public bool Genero { get; set; }
        public int IdNacionalidad { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[]? Foto { get; set; }
        public string? Huella { get; set; }
        public TrabajadorPersonaDTO? Trabajador { get; set; }



        public string NombreCompleto =>
        string.Join(" ",
            new[] { PrimerNombre, SegundoNombre}
            .Where(s => !string.IsNullOrWhiteSpace(s))
        ).Trim();

        public BitmapImage? PrevisualizarImagen
        {
            get
            {
                if (Foto == null || Foto.Length == 0) return null;
                using (var ms = new MemoryStream(Foto))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
        }
    }


}
