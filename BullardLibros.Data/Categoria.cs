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
    using System.Collections.Generic;
    
    public partial class Categoria
    {
        public Categoria()
        {
            this.Movimiento = new HashSet<Movimiento>();
        }
    
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
        public bool Estado { get; set; }
        public Nullable<int> IdCategoriaPadre { get; set; }
    
        public virtual ICollection<Movimiento> Movimiento { get; set; }
    }
}
