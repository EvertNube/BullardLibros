//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BullardLibros.Data
{
    using System;
    
    public partial class SP_GetReporteDetalleMovimientos_Result
    {
        public int IdMovimiento { get; set; }
        public int IdCuentaBancaria { get; set; }
        public int IdEntidadResponsable { get; set; }
        public int IdTipoMovimiento { get; set; }
        public Nullable<int> IdCategoria { get; set; }
        public int IdEstadoMovimiento { get; set; }
        public string Nombre { get; set; }
        public decimal Monto { get; set; }
        public string NumeroDocumento { get; set; }
        public string Comentario { get; set; }
        public bool Estado { get; set; }
        public int UsuarioCreacion { get; set; }
        public System.DateTime Fecha { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string EntidadResNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public string UsuarioNombre { get; set; }
    }
}
