using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClientCoopSoft.DTO
{
    public class HuellaResultado
    {
        public byte[] TemplateBytes { get; set; } = Array.Empty<byte>();
        public BitmapImage? ImagenHuella { get; set; } // Cambiado de BitmapImage
    }
}
