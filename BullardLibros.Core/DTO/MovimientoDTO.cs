using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class MovimientoDTO
    {
        public int IdMovimiento { get; set; }
        public int IdCuentaBancaria { get; set; }
        public int IdEntidadResponsable { get; set; }
        public int IdTipoMovimiento { get; set; }
        public int? IdCategoria { get; set; }
        public int IdEstadoMovimiento { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public Decimal Monto { get; set; }
        public string NumeroDocumento { get; set; }
        public string Comentario { get; set; }
        public bool Estado { get; set; }
        public int UsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreEntidadR { get; set; }
    }
}
