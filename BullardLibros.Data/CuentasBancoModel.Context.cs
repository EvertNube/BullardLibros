﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CuentasBancoEntities : DbContext
    {
        public CuentasBancoEntities()
            : base("name=CuentasBancoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<EstadoMovimiento> EstadoMovimiento { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<TipoMovimiento> TipoMovimiento { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<sysdiagrams> sysdiagrams { get; set; }
        public DbSet<Movimiento> Movimiento { get; set; }
        public DbSet<CuentaBancaria> CuentaBancaria { get; set; }
        public DbSet<Moneda> Moneda { get; set; }
        public DbSet<TipoCambio> TipoCambio { get; set; }
        public DbSet<EntidadResponsable> EntidadResponsable { get; set; }
    }
}
