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
    
    public partial class TipoDocumento
    {
        public TipoDocumento()
        {
            this.Movimiento = new HashSet<Movimiento>();
            this.EntidadResponsable = new HashSet<EntidadResponsable>();
        }
    
        public int IdTipoDocumento { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
    
        public virtual ICollection<Movimiento> Movimiento { get; set; }
        public virtual ICollection<EntidadResponsable> EntidadResponsable { get; set; }
    }
}
