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
    
    public partial class CuentaBancaria
    {
        public CuentaBancaria()
        {
            this.Movimiento = new HashSet<Movimiento>();
        }
    
        public int IdCuentaBancaria { get; set; }
        public string NombreCuenta { get; set; }
        public System.DateTime FechaConciliacion { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal SaldoBancario { get; set; }
        public bool Estado { get; set; }
        public Nullable<int> IdMoneda { get; set; }
        public Nullable<int> IdEmpresa { get; set; }
    
        public virtual Moneda Moneda { get; set; }
        public virtual ICollection<Movimiento> Movimiento { get; set; }
        public virtual Empresa Empresa { get; set; }
    }
}
