//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BullardLibros.Data
{
    using System;
    
    public partial class SP_Get_MontoIncompletoEnComprobante_Result
    {
        public int IdComprobante { get; set; }
        public int IdTipoComprobante { get; set; }
        public int IdTipoDocumento { get; set; }
        public int IdEntidadResponsable { get; set; }
        public int IdMoneda { get; set; }
        public int IdEmpresa { get; set; }
        public string NroDocumento { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoSinIGV { get; set; }
        public decimal TipoCambio { get; set; }
        public Nullable<int> IdArea { get; set; }
        public Nullable<int> IdResponsable { get; set; }
        public Nullable<int> IdCategoria { get; set; }
        public Nullable<int> IdProyecto { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public Nullable<System.DateTime> FechaConclusion { get; set; }
        public string Comentario { get; set; }
        public bool Estado { get; set; }
        public bool Ejecutado { get; set; }
        public Nullable<int> IdHonorario { get; set; }
        public int UsuarioCreacion { get; set; }
        public Nullable<decimal> MontoIncompleto { get; set; }
    }
}
