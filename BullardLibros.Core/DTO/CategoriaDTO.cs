using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardEncuestas.Core.DTO
{
    [Serializable]
    public class CategoriaDTO
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
        public int Estado { get; set; }
        public int IdCategoriaPadre { get; set; }
    }
}
