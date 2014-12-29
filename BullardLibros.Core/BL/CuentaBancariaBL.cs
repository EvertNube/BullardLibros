﻿using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class CuentaBancariaBL : Base
    {
        public List<CuentaBancariaDTO> getCuentasBancarias()
        {
            using (var context = getContext())
            {
                var result = context.CuentaBancaria.Select(x => new CuentaBancariaDTO
                {
                    IdCuentaBancaria = x.IdCuentaBancaria,
                    NombreCuenta = x.NombreCuenta,
                    FechaConciliacion = x.FechaConciliacion,
                    SaldoDisponible = x.SaldoDisponible,
                    SaldoBancario = x.SaldoBancario,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }

        public CuentaBancariaDTO getCuentaBancaria(int id)
        {
            using (var context = getContext())
            {
                var result = context.CuentaBancaria.Where(x => x.IdCuentaBancaria == id)
                    .Select(r => new CuentaBancariaDTO
                    {
                        IdCuentaBancaria = r.IdCuentaBancaria,
                        NombreCuenta = r.NombreCuenta,
                        FechaConciliacion = r.FechaConciliacion,
                        SaldoDisponible = r.SaldoDisponible,
                        SaldoBancario = r.SaldoBancario,
                        Estado = r.Estado,
                        listaMovimiento = r.Movimiento.Select(x => new MovimientoDTO {
                            IdMovimiento = x.IdMovimiento,
                            IdCuentaBancaria = x.IdCuentaBancaria,
                            IdEntidadResponsable = x.IdEntidadResponsable,
                            IdTipoMovimiento = x.IdTipoMovimiento,
                            IdCategoria = x.IdCategoria,
                            IdEstadoMovimiento = x.IdEstadoMovimiento,
                            Nombre = x.Nombre,
                            Fecha = x.Fecha,
                            Monto = x.Monto,
                            NumeroDocumento = x.NumeroDocumento,
                            Comentario = x.Comentario,
                            Estado = x.Estado,
                            UsuarioCreacion = x.UsuarioCreacion,
                            FechaCreacion = x.FechaCreacion
                        }).ToList()
                    }).SingleOrDefault();
                return result;
            }
        }
        public bool add(CuentaBancariaDTO CuentaBancaria)
        {
            using (var context = getContext())
            {
                try
                {
                    CuentaBancaria nuevo = new CuentaBancaria();
                    nuevo.NombreCuenta = CuentaBancaria.NombreCuenta;
                    nuevo.FechaConciliacion = CuentaBancaria.FechaConciliacion;
                    nuevo.SaldoDisponible = CuentaBancaria.SaldoDisponible;
                    nuevo.SaldoBancario = CuentaBancaria.SaldoBancario;
                    nuevo.Estado = true;
                    context.CuentaBancaria.Add(nuevo);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(CuentaBancariaDTO CuentaBancaria)
        {
            using (var context = getContext())
            {
                try
                {
                    //var miSaldoDisponible = context.SP_GetTotalIngresos(CuentaBancaria.IdCuentaBancaria).AsQueryable().First() as Decimal?;
                    var datoRow = context.CuentaBancaria.Where(x => x.IdCuentaBancaria == CuentaBancaria.IdCuentaBancaria).SingleOrDefault();
                    datoRow.NombreCuenta = CuentaBancaria.NombreCuenta;
                    datoRow.FechaConciliacion = CuentaBancaria.FechaConciliacion;
                    datoRow.SaldoDisponible = CuentaBancaria.SaldoDisponible;
                    datoRow.SaldoBancario = CuentaBancaria.SaldoBancario;
                    datoRow.Estado = CuentaBancaria.Estado;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        //public bool updateMontos(int id)
        //{
        //    using (var context = getContext())
        //    {
        //        try
        //        {
        //            var result1 = context.SP_GetTotalIngresos(id);
        //        }
        //        catch (Exception e)
        //        {
        //            throw e;
        //        }
        //    }
        //}
    }
}
