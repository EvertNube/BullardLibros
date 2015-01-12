using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class CategoriaR_DTO
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public Decimal MontoTotal { get; set; }
        public List<CategoriaR_DTO> Hijos { get; set; }
    }
}
