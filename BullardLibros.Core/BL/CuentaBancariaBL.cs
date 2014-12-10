using BullardLibros.Core.DTO;
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
                        Estado = r.Estado
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
    }
}
