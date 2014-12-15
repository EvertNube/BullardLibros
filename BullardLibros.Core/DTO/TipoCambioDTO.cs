using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class TipoCambioDTO
    {
        public int IdTipoCambio { get; set; }
        public DateTime Fecha { get; set; }
        public Decimal Valor { get; set; }
        public int IdMoneda { get; set; }
    }
}
