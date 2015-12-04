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
    
    public partial class Usuario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Usuario()
        {
            this.Comprobante = new HashSet<Comprobante>();
            this.Movimiento = new HashSet<Movimiento>();
        }
    
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string InicialesNombre { get; set; }
        public string Email { get; set; }
        public string Cuenta { get; set; }
        public string Pass { get; set; }
        public bool Estado { get; set; }
        public Nullable<System.DateTime> FechaRegistro { get; set; }
        public int IdRol { get; set; }
        public Nullable<int> IdCargo { get; set; }
        public int IdEmpresa { get; set; }
        public string Token { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comprobante> Comprobante { get; set; }
        public virtual Empresa Empresa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Movimiento> Movimiento { get; set; }
        public virtual Rol Rol { get; set; }
    }
}
