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
    
    public partial class EntidadResponsable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntidadResponsable()
        {
            this.Comprobante = new HashSet<Comprobante>();
            this.Movimiento = new HashSet<Movimiento>();
            this.Proyecto = new HashSet<Proyecto>();
        }
    
        public int IdEntidadResponsable { get; set; }
        public Nullable<int> IdTipoIdentificacion { get; set; }
        public Nullable<int> IdTipoEntidad { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public Nullable<decimal> Detraccion { get; set; }
        public string Tipo { get; set; }
        public int IdEmpresa { get; set; }
        public string NroIdentificacion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comprobante> Comprobante { get; set; }
        public virtual Empresa Empresa { get; set; }
        public virtual TipoEntidad TipoEntidad { get; set; }
        public virtual TipoIdentificacion TipoIdentificacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Movimiento> Movimiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Proyecto> Proyecto { get; set; }
    }
}
