using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class EntidadResponsableR_DTO
    {
        public int IdEntidadResponsable { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public Decimal? Detraccion { get; set; }
        public Decimal Monto { get; set; }
    }
}
