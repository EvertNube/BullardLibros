using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//required for sql function access
using System.Data.Entity.Core.Objects.DataClasses;

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
                }).OrderBy(x => x.Fecha).ToList();
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
                    //El IdCategoria sera 1 porque la Categoria con Id = 1 es No tiene categoria
                    nuevo.IdCategoria = (Movimiento.IdCategoria != 0 && Movimiento.IdCategoria != null) ? Movimiento.IdCategoria : 1;
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
                    //Actualizar saldos del Libro
                    ActualizarSaldos(Movimiento.IdCuentaBancaria);
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
                    //El IdCategoria sera 1 porque la Categoria con Id = 1 es No tiene categoria
                    datoRow.IdCategoria = (Movimiento.IdCategoria != 0 && Movimiento.IdCategoria != null) ? Movimiento.IdCategoria : 1;
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
                    //Actualizar saldos del Libro
                    ActualizarSaldos(Movimiento.IdCuentaBancaria);
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public IList<TipoMovimientoDTO> getTiposMovimientos(bool AsSelectList = false)
        {
            TipoMovimientoBL oBL = new TipoMovimientoBL();
            if (!AsSelectList)
                return oBL.getTiposMovimientos();
            else
            {
                var lista = oBL.getTiposMovimientos();
                lista.Insert(0, new TipoMovimientoDTO() { IdTipoMovimiento = 0, Nombre = "Seleccione el Tipo de Mov." });
                return lista;
            }
        }

        public IList<EstadoMovimientoDTO> getEstadosMovimientos(bool AsSelectList = false)
        {
            EstadoMovimientoBL oBL = new EstadoMovimientoBL();
            if (!AsSelectList)
                return oBL.getEstadosMovimientos();
            else
            {
                var lista = oBL.getEstadosMovimientos();
                lista.Insert(0, new EstadoMovimientoDTO() { IdEstadoMovimiento = 0, Nombre = "Seleccione el Estado del Mov." });
                return lista;
            }
        }

        public IList<EntidadResponsableDTO> getEntidadesResponsables(bool AsSelectList = false)
        {
            EntidadResponsableBL oBL = new EntidadResponsableBL();
            if (!AsSelectList)
                return oBL.getEntidadResponsables();
            else
            {
                var lista = oBL.getEntidadResponsables();
                lista.Insert(0, new EntidadResponsableDTO() { IdEntidadResponsable = 0, Nombre = "Seleccione la Entidad Responsable" });
                return lista;
            }
        }

        public string getNombreCategoria(int id)
        {
            if(id != 0)
            { 
            CategoriaBL oBL = new CategoriaBL();
            return oBL.getCategoria(id).Nombre;
            }
            return "Sin Categoría";
        }

        public void ActualizarSaldos(int idCuentaB)
        {
            if (idCuentaB != 0)
            {
                CuentaBancariaBL oBL = new CuentaBancariaBL();
                oBL.updateSaldoDisponible(idCuentaB);
            }
        }
    }
}
