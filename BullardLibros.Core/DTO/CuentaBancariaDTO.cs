using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class CuentaBancariaDTO
    {
        public int IdCuentaBancaria { get; set; }
        public string NombreCuenta { get; set; }
        public DateTime FechaConciliacion { get; set; }
        public Decimal SaldoDisponible { get; set; }
        public Decimal SaldoBancario { get; set; }
        public bool Estado { get; set; }
    }
}
