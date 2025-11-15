using ClientCoopSoft.DTO.Horarios;

namespace ClientCoopSoft.DTO.Trabajadores
{
    public class TrabajadorEditarDTO
    {
        public int IdPersona { get; set; }
        public decimal HaberBasico { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int IdCargo { get; set; }
        public List<HorarioCrearDTO> Horarios { get; set; } = new();
    }
}
