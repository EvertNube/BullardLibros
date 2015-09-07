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
                    NroOperacion = x.NroOperacion,
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
                        NroOperacion = r.NroOperacion,
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
                    nuevo.NroOperacion = Movimiento.NroOperacion;
                    //nuevo.Fecha = Movimiento.Fecha;
                    nuevo.Fecha = Convert.ToDateTime(Movimiento.Fecha.ToString("yyyy-MM-dd hh:mm:ss tt"));
                    nuevo.Monto = Movimiento.Monto;
                    nuevo.NumeroDocumento = Movimiento.NumeroDocumento;
                    nuevo.Comentario = Movimiento.Comentario;
                    nuevo.Estado = true;
                    nuevo.UsuarioCreacion = Movimiento.UsuarioCreacion;
                    //nuevo.FechaCreacion = Movimiento.FechaCreacion;
                    nuevo.FechaCreacion = Convert.ToDateTime(Movimiento.FechaCreacion.ToString("yyyy-MM-dd hh:mm:ss tt"));
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
                    datoRow.NroOperacion = Movimiento.NroOperacion;
                    //datoRow.Fecha = Movimiento.Fecha;
                    datoRow.Fecha = Convert.ToDateTime(Movimiento.Fecha.ToString("yyyy-MM-dd hh:mm:ss tt"));
                    datoRow.Monto = Movimiento.Monto;
                    datoRow.NumeroDocumento = Movimiento.NumeroDocumento;
                    datoRow.Comentario = Movimiento.Comentario;
                    datoRow.Estado = Movimiento.Estado;
                    datoRow.UsuarioCreacion = Movimiento.UsuarioCreacion;
                    //datoRow.FechaCreacion = Movimiento.FechaCreacion;
                    datoRow.FechaCreacion = Convert.ToDateTime(Movimiento.FechaCreacion.ToString("yyyy-MM-dd hh:mm:ss tt"));
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
            {
                //return oBL.getTiposMovimientos();
                var lista = oBL.getTiposMovimientos().OrderByDescending(x => x.IdTipoMovimiento).ToList();
                return lista;
            }
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

        public IList<EntidadResponsableDTO> getEntidadesResponsablesEnEmpresa(int idEmpresa, bool AsSelectList = false)
        {
            EntidadResponsableBL oBL = new EntidadResponsableBL();
            if (!AsSelectList)
                return oBL.getEntidadResponsablesEnEmpresaViewBag(idEmpresa);
            else
            {
                var lista = oBL.getEntidadResponsablesEnEmpresaViewBag(idEmpresa);
                lista.Insert(0, new EntidadResponsableDTO() { IdEntidadResponsable = 0, Nombre = "Seleccione la Entidad Responsable" });
                return lista;
            }
        }

        public IList<EntidadResponsableDTO> getEntidadesResponsables(bool AsSelectList = false)
        {
            EntidadResponsableBL oBL = new EntidadResponsableBL();
            if (!AsSelectList)
                return oBL.getEntidadResponsablesViewBag();
            else
            {
                var lista = oBL.getEntidadResponsablesViewBag();
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
                oBL.updateSaldos(idCuentaB);
            }
        }

        public IList<MovimientoDTO> getReporteDetalleLibro(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetReporteDetalleMovimientos(IdCuentaB, FechaInicio, FechaFin).Select(x => new MovimientoDTO
                {
                    IdMovimiento = x.IdMovimiento,
                    IdCuentaBancaria = x.IdCuentaBancaria,
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    IdTipoMovimiento = x.IdTipoMovimiento,
                    IdCategoria = x.IdCategoria,
                    IdEstadoMovimiento = x.IdEstadoMovimiento,
                    NroOperacion = x.NroOperacion,
                    Fecha = x.Fecha,
                    Monto = x.Monto,
                    NumeroDocumento = x.NumeroDocumento,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    UsuarioCreacion = x.UsuarioCreacion,
                    FechaCreacion = x.FechaCreacion,
                    NombreEntidadR = x.EntidadResNombre,
                    NombreCategoria = x.CategoriaNombre,
                    NombreUsuario = x.UsuarioNombre
                }).ToList();
                return result;
            }
        }
        public List<TipoEntidadDTO> getListaTipoEntidades()
        {
            using (var context = getContext())
            {
                var result = context.TipoEntidad.Select(x => new TipoEntidadDTO
                {
                    IdTipoEntidad = x.IdTipoEntidad,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }
        public List<Select2DTO> getListaFormaMovimientosIngresos()
        {
            using (var context = getContext())
            {
                var result = context.FormaMovimiento.Where(x => x.IdTipoMovimiento == 1).Select(x => new Select2DTO 
                { 
                    id = x.IdFormaMovimiento,
                    text = x.Nombre
                    //IdFormaMovimiento = x.IdFormaMovimiento,
                    //IdTipoMovimiento = x.IdTipoMovimiento,
                    //Nombre = x.Nombre,
                    //Estado = x.Estado,
                    //NombreTipo = x.IdTipoMovimiento == 1 ? "Ingreso" : "Egreso"
                }).ToList();
                return result;
            }
        }
        public List<Select2DTO> getListaFormaMovimientosEgresos()
        {
            using (var context = getContext())
            {
                var result = context.FormaMovimiento.Where(x => x.IdTipoMovimiento == 2).Select(x => new Select2DTO
                {
                    id = x.IdFormaMovimiento,
                    text = x.Nombre
                    //IdFormaMovimiento = x.IdFormaMovimiento,
                    //IdTipoMovimiento = x.IdTipoMovimiento,
                    //Nombre = x.Nombre,
                    //Estado = x.Estado,
                    //NombreTipo = x.IdTipoMovimiento == 1 ? "Ingreso" : "Egreso"
                }).ToList();
                return result;
            }
        }

        public List<TipoDocumentoDTO> getListaTiposDeDocumento()
        {
            using (var context = getContext())
            {
                var result = context.TipoDocumento.Select(x => new TipoDocumentoDTO 
                {
                    IdTipoDocumento = x.IdTipoDocumento,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }

        public IList<TipoDocumentoDTO> getListaTiposDeDocumentoVB(bool AsSelectList = false)
        {
            if (!AsSelectList)
                return getListaTiposDeDocumento();
            else
            {
                var lista = getListaTiposDeDocumento();
                lista.Insert(0, new TipoDocumentoDTO() { IdTipoDocumento = 0, Nombre = "Seleccione un tipo" });
                return lista;
            }
        }
    }
}
