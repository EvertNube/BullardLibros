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
    public class MovimientoBL : Base
    {
        public List<MovimientoDTO> getMovimientos()
        {
            using (var context = getContext())
            {
                var result = context.Movimiento.Select(x => new MovimientoDTO
                {
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
                }).ToList();
                return result;
            }
        }
        public MovimientoDTO getMovimiento(int id)
        {
            using (var context = getContext())
            {
                var result = context.Movimiento.Where(x => x.IdMovimiento == id)
                    .Select(r => new MovimientoDTO
                    {
                        IdMovimiento = r.IdMovimiento,
                        IdCuentaBancaria = r.IdCuentaBancaria,
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        IdTipoMovimiento = r.IdTipoMovimiento,
                        IdCategoria = r.IdCategoria,
                        IdEstadoMovimiento = r.IdEstadoMovimiento,
                        Nombre = r.Nombre,
                        Fecha = r.Fecha,
                        Monto = r.Monto,
                        NumeroDocumento = r.NumeroDocumento,
                        Comentario = r.Comentario,
                        Estado = r.Estado,
                        UsuarioCreacion = r.UsuarioCreacion,
                        FechaCreacion = r.FechaCreacion
                    }).SingleOrDefault();
                return result;
            }
        }
        public bool add(MovimientoDTO Movimiento)
        {
            using (var context = getContext())
            {
                try
                {
                    Movimiento nuevo = new Movimiento();
                    nuevo.IdCuentaBancaria = Movimiento.IdCuentaBancaria;
                    nuevo.IdEntidadResponsable = Movimiento.IdEntidadResponsable;
                    nuevo.IdTipoMovimiento = Movimiento.IdTipoMovimiento;
                    nuevo.IdCategoria = Movimiento.IdCategoria;
                    nuevo.IdEstadoMovimiento = Movimiento.IdEstadoMovimiento;
                    nuevo.Nombre = Movimiento.Nombre;
                    nuevo.Fecha = Movimiento.Fecha;
                    nuevo.Monto = Movimiento.Monto;
                    nuevo.NumeroDocumento = Movimiento.NumeroDocumento;
                    nuevo.Comentario = Movimiento.Comentario;
                    nuevo.Estado = true;
                    nuevo.UsuarioCreacion = Movimiento.UsuarioCreacion;
                    nuevo.FechaCreacion = Movimiento.FechaCreacion;
                    context.Movimiento.Add(nuevo);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(MovimientoDTO Movimiento)
        {
            using (var context = getContext())
            {
                try
                {
                    var datoRow = context.Movimiento.Where(x => x.IdMovimiento == Movimiento.IdMovimiento).SingleOrDefault();
                    datoRow.IdCuentaBancaria = Movimiento.IdCuentaBancaria;
                    datoRow.IdEntidadResponsable = Movimiento.IdEntidadResponsable;
                    datoRow.IdTipoMovimiento = Movimiento.IdTipoMovimiento;
                    datoRow.IdCategoria = Movimiento.IdCategoria;
                    datoRow.IdEstadoMovimiento = Movimiento.IdEstadoMovimiento;
                    datoRow.Nombre = Movimiento.Nombre;
                    datoRow.Fecha = Movimiento.Fecha;
                    datoRow.Monto = Movimiento.Monto;
                    datoRow.NumeroDocumento = Movimiento.NumeroDocumento;
                    datoRow.Comentario = Movimiento.Comentario;
                    datoRow.Estado = Movimiento.Estado;
                    datoRow.UsuarioCreacion = Movimiento.UsuarioCreacion;
                    datoRow.FechaCreacion = Movimiento.FechaCreacion;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /*public IList<TipoMovimientoDTO> getTiposMovimiento(bool AsSelectList = false)
        {
            
        }*/
    }
}
