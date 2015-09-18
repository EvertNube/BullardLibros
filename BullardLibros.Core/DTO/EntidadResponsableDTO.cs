using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class EntidadResponsableDTO
    {
        public int IdEntidadResponsable { get; set; }
        public int? IdTipoIdentificacion { get; set; }
        public int? IdTipoEntidad { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public Decimal? Detraccion { get; set; }
        public string Tipo { get; set; }
        public int IdEmpresa { get; set; }
        public string NroIdentificacion { get; set; }

        public string NombreTipoEntidad { get; set; }
    }
}
