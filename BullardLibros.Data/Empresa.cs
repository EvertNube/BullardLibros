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
    using System.Collections.Generic;
    
    public partial class Empresa
    {
        public Empresa()
        {
            this.Area = new HashSet<Area>();
            this.Categoria = new HashSet<Categoria>();
            this.Comprobante = new HashSet<Comprobante>();
            this.CuentaBancaria = new HashSet<CuentaBancaria>();
            this.EntidadResponsable = new HashSet<EntidadResponsable>();
            this.Honorario = new HashSet<Honorario>();
            this.Responsable = new HashSet<Responsable>();
            this.Usuario = new HashSet<Usuario>();
        }
    
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public string Descripcion { get; set; }
        public decimal TipoCambio { get; set; }
        public Nullable<decimal> IGV { get; set; }
        public Nullable<int> IdPeriodo { get; set; }
        public int IdMoneda { get; set; }
    
        public virtual ICollection<Area> Area { get; set; }
        public virtual ICollection<Categoria> Categoria { get; set; }
        public virtual ICollection<Comprobante> Comprobante { get; set; }
        public virtual ICollection<CuentaBancaria> CuentaBancaria { get; set; }
        public virtual Moneda Moneda { get; set; }
        public virtual Periodo Periodo { get; set; }
        public virtual ICollection<EntidadResponsable> EntidadResponsable { get; set; }
        public virtual ICollection<Honorario> Honorario { get; set; }
        public virtual ICollection<Responsable> Responsable { get; set; }
        public virtual ICollection<Usuario> Usuario { get; set; }
    }
}
