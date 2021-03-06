﻿using BullardLibros.Core.DTO;
using BullardLibros.Core.BL;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class ComprobanteBL : Base
    {
        public List<ComprobanteDTO> getComprobantesEnEmpresa(int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.Comprobante.Where(x => x.IdEmpresa == idEmpresa).Select(x => new ComprobanteDTO
                {
                    IdComprobante = x.IdComprobante,
                    IdTipoComprobante = x.IdTipoComprobante,
                    IdTipoDocumento = x.IdTipoDocumento,
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    IdMoneda = x.IdMoneda,
                    IdEmpresa = x.IdEmpresa,
                    NroDocumento = x.NroDocumento,
                    Monto = x.Monto,
                    IdArea = x.IdArea,
                    IdResponsable = x.IdResponsable,
                    IdCategoria = x.IdCategoria,
                    IdProyecto = x.IdProyecto,
                    FechaEmision = x.FechaEmision,
                    FechaConclusion = x.FechaConclusion,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    Ejecutado = x.Ejecutado,
                    IdHonorario = x.IdHonorario,
                    NombreEntidad = x.EntidadResponsable.Nombre,
                    NombreMoneda = x.Moneda.Nombre,
                    NombreTipoComprobante = x.TipoComprobante.Nombre,
                    NombreTipoDocumento = x.TipoDocumento.Nombre,
                    SimboloMoneda = x.Moneda.Simbolo,
                    MontoSinIGV = x.MontoSinIGV,
                    TipoCambio = x.TipoCambio,
                    UsuarioCreacion = x.UsuarioCreacion,
                    NombreUsuario = x.Usuario.Cuenta,
                    NombreCategoria = x.Categoria.Nombre ?? "",
                    NombreProyecto = x.Proyecto.Nombre ?? ""
                }).OrderBy(x => x.NroDocumento).ToList();
                return result;
            }
        }
        public List<ComprobanteDTO> getComprobantesEnEmpresaPorTipo(int idEmpresa, int tipo)
        {
            using (var context = getContext())
            {
                var result = context.Comprobante.Where(x => x.IdEmpresa == idEmpresa && x.IdTipoComprobante == tipo).Select(x => new ComprobanteDTO
                {
                    IdComprobante = x.IdComprobante,
                    IdTipoComprobante = x.IdTipoComprobante,
                    IdTipoDocumento = x.IdTipoDocumento,
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    IdMoneda = x.IdMoneda,
                    IdEmpresa = x.IdEmpresa,
                    NroDocumento = x.NroDocumento,
                    Monto = x.Monto,
                    IdArea = x.IdArea,
                    IdResponsable = x.IdResponsable,
                    IdCategoria = x.IdCategoria,
                    IdProyecto = x.IdProyecto,
                    FechaEmision = x.FechaEmision,
                    FechaConclusion = x.FechaConclusion,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    Ejecutado = x.Ejecutado,
                    IdHonorario = x.IdHonorario,
                    NombreEntidad = x.EntidadResponsable.Nombre,
                    NombreMoneda = x.Moneda.Nombre,
                    NombreTipoComprobante = x.TipoComprobante.Nombre,
                    NombreTipoDocumento = x.TipoDocumento.Nombre,
                    SimboloMoneda = x.Moneda.Simbolo,
                    MontoSinIGV = x.MontoSinIGV,
                    TipoCambio = x.TipoCambio,
                    UsuarioCreacion = x.UsuarioCreacion,
                    NombreUsuario = x.Usuario.Cuenta,
                    NombreCategoria = x.Categoria.Nombre ?? "",
                    NombreProyecto = x.Proyecto.Nombre ?? ""
                }).OrderBy(x => x.NroDocumento).ToList();
                return result;
            }
        }
        public List<ComprobanteDTO> getComprobantesEnEmpresaViewBag(int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.Comprobante.Where(x => x.Estado && x.IdEmpresa == idEmpresa).Select(x => new ComprobanteDTO
                {
                    IdComprobante = x.IdComprobante,
                    IdTipoComprobante = x.IdTipoComprobante,
                    IdTipoDocumento = x.IdTipoDocumento,
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    IdMoneda = x.IdMoneda,
                    IdEmpresa = x.IdEmpresa,
                    NroDocumento = x.NroDocumento,
                    Monto = x.Monto,
                    IdArea = x.IdArea,
                    IdResponsable = x.IdResponsable,
                    IdCategoria = x.IdCategoria,
                    IdProyecto = x.IdProyecto,
                    FechaEmision = x.FechaEmision,
                    FechaConclusion = x.FechaConclusion,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    Ejecutado = x.Ejecutado,
                    IdHonorario = x.IdHonorario,
                    NombreEntidad = x.EntidadResponsable.Nombre,
                    NombreMoneda = x.Moneda.Nombre,
                    NombreTipoComprobante = x.TipoComprobante.Nombre,
                    NombreTipoDocumento = x.TipoDocumento.Nombre,
                    SimboloMoneda = x.Moneda.Simbolo,
                    MontoSinIGV = x.MontoSinIGV,
                    TipoCambio = x.TipoCambio,
                    UsuarioCreacion = x.UsuarioCreacion,
                    NombreUsuario = x.Usuario.Cuenta,
                    NombreCategoria = x.Categoria.Nombre ?? "",
                    NombreProyecto = x.Proyecto.Nombre
                }).OrderBy(x => x.NroDocumento).ToList();
                return result;
            }
        }
        public ComprobanteDTO getComprobanteEnEmpresa(int idEmpresa, int id)
        {
            using (var context = getContext())
            {
                var result = context.Comprobante.Where(x => x.IdComprobante == id && x.IdEmpresa == idEmpresa)
                    .Select(r => new ComprobanteDTO
                    {
                        IdComprobante = r.IdComprobante,
                        IdTipoComprobante = r.IdTipoComprobante,
                        IdTipoDocumento = r.IdTipoDocumento,
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        IdMoneda = r.IdMoneda,
                        IdEmpresa = r.IdEmpresa,
                        NroDocumento = r.NroDocumento,
                        Monto = r.Monto,
                        IdArea = r.IdArea,
                        IdResponsable = r.IdResponsable,
                        IdCategoria = r.IdCategoria,
                        IdProyecto = r.IdProyecto,
                        FechaEmision = r.FechaEmision,
                        FechaConclusion = r.FechaConclusion,
                        Comentario = r.Comentario,
                        Estado = r.Estado,
                        Ejecutado = r.Ejecutado,
                        IdHonorario = r.IdHonorario,
                        NombreEntidad = r.EntidadResponsable.Nombre,
                        NombreMoneda = r.Moneda.Nombre,
                        NombreTipoComprobante = r.TipoComprobante.Nombre,
                        NombreTipoDocumento = r.TipoDocumento.Nombre,
                        SimboloMoneda = r.Moneda.Simbolo,
                        MontoSinIGV = r.MontoSinIGV,
                        TipoCambio = r.TipoCambio,
                        UsuarioCreacion = r.UsuarioCreacion,
                        NombreUsuario = r.Usuario.Cuenta,
                        NombreCategoria = r.Categoria.Nombre ?? "",
                        NombreProyecto = r.Proyecto.Nombre,
                        lstMontos = r.AreaPorComprobante.Select(x => new AreaPorComprobanteDTO 
                        {
                            IdArea = x.IdArea,
                            IdComprobante = x.IdComprobante,
                            Monto = x.Monto,
                            NombreArea = x.Area.Nombre
                        }).ToList()
                    }).SingleOrDefault();
                return result;
            }
        }

        public ComprobanteDTO getComprobanteEjecutadoEnEmpresa(int id, int idCuentaBancaria, int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.SP_Get_MontoIncompletoEnComprobante(id, idCuentaBancaria, idEmpresa)
                    .Select(r => new ComprobanteDTO
                    {
                        IdComprobante = r.IdComprobante,
                        IdTipoComprobante = r.IdTipoComprobante,
                        IdTipoDocumento = r.IdTipoDocumento,
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        IdMoneda = r.IdMoneda,
                        IdEmpresa = r.IdEmpresa,
                        NroDocumento = r.NroDocumento,
                        Monto = r.Monto,
                        MontoSinIGV = r.MontoSinIGV,
                        IdArea = r.IdArea,
                        IdResponsable = r.IdResponsable,
                        IdCategoria = r.IdCategoria,
                        IdProyecto = r.IdProyecto,
                        FechaEmision = r.FechaEmision,
                        FechaConclusion = r.FechaConclusion,
                        Comentario = r.Comentario,
                        Estado = r.Estado,
                        Ejecutado = r.Ejecutado,
                        IdHonorario = r.IdHonorario,
                        TipoCambio = r.TipoCambio,
                        UsuarioCreacion = r.UsuarioCreacion,
                        MontoIncompleto = r.MontoIncompleto.GetValueOrDefault()
                    }).SingleOrDefault();
                return result;
            }
        }

        public bool add(ComprobanteDTO Comprobante)
        {
            using (var context = getContext())
            {
                try
                {
                    Comprobante nuevo = new Comprobante();
                    nuevo.IdTipoComprobante = Comprobante.IdTipoComprobante;
                    nuevo.IdTipoDocumento = Comprobante.IdTipoDocumento;
                    nuevo.IdEntidadResponsable = Comprobante.IdEntidadResponsable;
                    nuevo.IdMoneda = Comprobante.IdMoneda;
                    nuevo.IdEmpresa = Comprobante.IdEmpresa;
                    nuevo.NroDocumento = Comprobante.NroDocumento;
                    nuevo.Monto = Comprobante.Monto;
                    nuevo.IdArea = Comprobante.IdArea;
                    nuevo.IdResponsable = Comprobante.IdResponsable;
                    nuevo.IdCategoria = Comprobante.IdCategoria;
                    nuevo.IdProyecto = Comprobante.IdProyecto;
                    nuevo.FechaEmision = Comprobante.FechaEmision;
                    nuevo.FechaConclusion = Comprobante.FechaConclusion;
                    nuevo.Comentario = Comprobante.Comentario;
                    nuevo.Estado = true;
                    nuevo.Ejecutado = false;
                    nuevo.IdHonorario = Comprobante.IdHonorario;
                    nuevo.MontoSinIGV = Comprobante.MontoSinIGV;
                    nuevo.TipoCambio = Comprobante.TipoCambio;
                    nuevo.UsuarioCreacion = Comprobante.UsuarioCreacion;
                    context.Comprobante.Add(nuevo);

                    foreach (var item in Comprobante.lstMontos)
                    {
                        AreaPorComprobante novo = new AreaPorComprobante();
                        novo.IdArea = item.IdArea;
                        novo.IdComprobante = nuevo.IdComprobante;
                        novo.Monto = item.Monto;
                        nuevo.AreaPorComprobante.Add(novo);
                    }

                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(ComprobanteDTO Comprobante)
        {
            using (var context = getContext())
            {
                try
                {
                    var row = context.Comprobante.Where(x => x.IdComprobante == Comprobante.IdComprobante).SingleOrDefault();
                    row.IdTipoComprobante = Comprobante.IdTipoComprobante;
                    row.IdTipoDocumento = Comprobante.IdTipoDocumento;
                    row.IdEntidadResponsable = Comprobante.IdEntidadResponsable;
                    row.IdMoneda = Comprobante.IdMoneda;
                    row.IdEmpresa = Comprobante.IdEmpresa;
                    row.NroDocumento = Comprobante.NroDocumento;
                    row.Monto = Comprobante.Monto;
                    row.IdArea = Comprobante.IdArea;
                    row.IdResponsable = Comprobante.IdResponsable;
                    row.IdCategoria = Comprobante.IdCategoria;
                    row.IdProyecto = Comprobante.IdProyecto;
                    row.FechaEmision = Comprobante.FechaEmision;
                    row.FechaConclusion = Comprobante.FechaConclusion;
                    row.Comentario = Comprobante.Comentario;
                    row.Estado = Comprobante.Estado;
                    row.IdHonorario = Comprobante.IdHonorario;
                    row.MontoSinIGV = Comprobante.MontoSinIGV;
                    row.TipoCambio = Comprobante.TipoCambio;
                    row.UsuarioCreacion = Comprobante.UsuarioCreacion;

                    var allmontos = from m in context.AreaPorComprobante
                                    where m.IdComprobante == row.IdComprobante
                                    select m;

                    foreach (var item in allmontos)
	                {
                        row.AreaPorComprobante.Remove(item);
	                }

                    foreach (var item in Comprobante.lstMontos)
                    {
                        AreaPorComprobante novo = new AreaPorComprobante();
                        novo.IdArea = item.IdArea;
                        novo.IdComprobante = row.IdComprobante;
                        novo.Monto = item.Monto;
                        row.AreaPorComprobante.Add(novo);
                    }

                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public bool delete(int id)
        {
            using (var context = getContext())
            {
                try
                {
                    var row = context.Comprobante.Where(x => x.IdComprobante == id).SingleOrDefault();
                    //Si el comprobante esta ligado a Movimientos primero se eliminan todos los Movimientos
                    var allmovimientos = context.Movimiento.Where(x => x.IdComprobante == row.IdComprobante).ToList();
                    MovimientoBL movBL = new MovimientoBL();
                    foreach(var item in allmovimientos)
                    {
                        movBL.delete(item.IdMovimiento);
                    }
                    //Si el comprobante esta ligado a pagos por areas primero se eliminan todos sus montos AreasPorComprobantes
                    var allmontos = context.AreaPorComprobante.Where(x => x.IdComprobante == row.IdComprobante).ToList();
                    foreach(var item in allmontos)
                    {
                        row.AreaPorComprobante.Remove(item);
                    }
                    context.Comprobante.Remove(row);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public bool actualizarEjecutado(int idComprobante, bool ejecutado, int idEmpresa)
        {
            using (var context = getContext())
            {
                try
                {
                    var row = context.Comprobante.Where(x => x.IdComprobante == idComprobante && x.IdEmpresa == idEmpresa).SingleOrDefault();
                    row.Ejecutado = ejecutado;
                    if (ejecutado) row.FechaConclusion = DateTime.Now;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }   
            }
        }

        public List<TipoComprobanteDTO> getTipoDeComprobantes()
        {
            using (var context = getContext())
            {
                var result = context.TipoComprobante.Select(x => new TipoComprobanteDTO
                {
                    IdTipoComprobante = x.IdTipoComprobante,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }
        public List<TipoDocumentoDTO> getTipoDeDocumentos()
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
        public List<TipoEntidadDTO> getTipoDeEntidades()
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
        public List<EntidadResponsableDTO> getListaEntidadesEnEmpresa(int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdEmpresa == idEmpresa).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }

        public List<EntidadResponsableDTO> getListaClientesEnEmpresa(int idEmpresa)
        {
            //SOLO ACTIVOS
            //Clientes Entidad Tipo 1
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdTipoEntidad == 1 && x.IdEmpresa == idEmpresa && x.Estado).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }
        public List<EntidadResponsableDTO> getListaProveedoresEnEmpresa(int idEmpresa)
        {
            //SOLO ACTIVOS
            //Proveedores Entidad Tipo 2
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdTipoEntidad == 2 && x.IdEmpresa == idEmpresa && x.Estado).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
                return result;
            }
        }

        public List<MonedaDTO> getListaMonedas()
        {
            using (var context = getContext())
            {
                var result = context.Moneda.Select(x => new MonedaDTO
                {
                    IdMoneda = x.IdMoneda,
                    Nombre = x.Nombre,
                    Simbolo = x.Simbolo
                }).ToList();
                return result;
            }
        }

        public List<AreaNDTO> getListaAreasEnEmpresa(int idEmpresa, bool? esNull = false)
        {
            //SOLO ACTIVOS
            using (var context = getContext())
            {
                var result = context.Area.Where(x => x.IdEmpresa == idEmpresa && x.Estado).Select(x => new AreaNDTO
                {
                    IdArea = x.IdArea,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).OrderBy(x => x.Nombre).ToList();

                if(esNull != null)
                {
                    result.Insert(0, new AreaNDTO() { IdArea = null, Nombre = "Seleccione un área" });
                }
                return result;
            }
        }

        public List<ResponsableDTO> getListaResponsablesEnEmpresa(int idEmpresa)
        {
            //SOLO ACTIVOS
            using (var context = getContext())
            {
                var result = context.Responsable.Where(x => x.IdEmpresa == idEmpresa && x.Estado).Select(x => new ResponsableDTO
                    {
                        IdResponsable = x.IdResponsable,
                        Nombre = x.Nombre,
                        Estado = x.Estado
                    }).ToList();
                return result;
            }
        }

        public List<Select2DTO_B> getComprobantesPorEntXTDoc(int idEmpresa, int idEntidad, int idTipoDoc)
        {
            using (var context = getContext())
            {
                var result = context.Comprobante.Where(x => x.IdEmpresa == idEmpresa && x.IdEntidadResponsable == idEntidad && x.IdTipoDocumento == idTipoDoc && x.Estado).Select(x => new Select2DTO_B
                    {
                        id = x.IdComprobante,
                        text = x.NroDocumento
                    }).ToList();
                return result;
            }
        }

        public List<HonorarioDTO> getListaHonorariosEnEmpresa(int idEmpresa)
        {
            HonorarioBL objBL = new HonorarioBL();
            return objBL.getHonorariosEnEmpresaViewBag(idEmpresa);
        }
    }
}
