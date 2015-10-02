using BullardLibros.Core.DTO;
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
                    FechaEmision = x.FechaEmision,
                    FechaConclusion = x.FechaConclusion,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    IdHonorario = x.IdHonorario,
                    NombreEntidad = x.EntidadResponsable.Nombre,
                    NombreMoneda = x.Moneda.Nombre,
                    NombreTipoComprobante = x.TipoComprobante.Nombre,
                    NombreTipoDocumento = x.TipoDocumento.Nombre,
                    SimboloMoneda = x.Moneda.Simbolo,
                    MontoSinIGV = x.MontoSinIGV
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
                    FechaEmision = x.FechaEmision,
                    FechaConclusion = x.FechaConclusion,
                    Comentario = x.Comentario,
                    Estado = x.Estado,
                    IdHonorario = x.IdHonorario,
                    NombreEntidad = x.EntidadResponsable.Nombre,
                    NombreMoneda = x.Moneda.Nombre,
                    NombreTipoComprobante = x.TipoComprobante.Nombre,
                    NombreTipoDocumento = x.TipoDocumento.Nombre,
                    SimboloMoneda = x.Moneda.Simbolo,
                    MontoSinIGV = x.MontoSinIGV
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
                        FechaEmision = r.FechaEmision,
                        FechaConclusion = r.FechaConclusion,
                        Comentario = r.Comentario,
                        Estado = r.Estado,
                        IdHonorario = r.IdHonorario,
                        NombreEntidad = r.EntidadResponsable.Nombre,
                        NombreMoneda = r.Moneda.Nombre,
                        NombreTipoComprobante = r.TipoComprobante.Nombre,
                        NombreTipoDocumento = r.TipoDocumento.Nombre,
                        SimboloMoneda = r.Moneda.Simbolo,
                        MontoSinIGV = r.MontoSinIGV,
                        lstMontos = r.AreaPorComprobante.Select(x => new AreaPorComprobanteDTO 
                        {
                            IdArea = x.IdArea,
                            IdComprobante = x.IdComprobante,
                            Monto = x.Monto
                        }).ToList()
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
                    nuevo.FechaEmision = Comprobante.FechaEmision;
                    nuevo.FechaConclusion = Comprobante.FechaConclusion;
                    nuevo.Comentario = Comprobante.Comentario;
                    nuevo.Estado = true;
                    nuevo.Ejecutado = false;
                    nuevo.IdHonorario = Comprobante.IdHonorario;
                    nuevo.MontoSinIGV = Comprobante.MontoSinIGV ?? 0;
                    context.Comprobante.Add(nuevo);
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
                    row.FechaEmision = Comprobante.FechaEmision;
                    row.FechaConclusion = Comprobante.FechaConclusion;
                    row.Comentario = Comprobante.Comentario;
                    row.Estado = Comprobante.Estado;
                    row.IdHonorario = Comprobante.IdHonorario;
                    row.MontoSinIGV = Comprobante.MontoSinIGV ?? 0;
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

        public List<AreaDTO> getListaAreasEnEmpresa(int idEmpresa)
        {
            //SOLO ACTIVOS
            using (var context = getContext())
            {
                var result = context.Area.Where(x => x.IdEmpresa == idEmpresa && x.Estado).Select(x => new AreaDTO
                {
                    IdArea = x.IdArea,
                    Nombre = x.Nombre,
                    Estado = x.Estado
                }).ToList();
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
                var result = context.Comprobante.Where(x => x.IdEmpresa == idEmpresa && x.IdEntidadResponsable == idEntidad && x.IdTipoDocumento == idTipoDoc && !x.Ejecutado && x.Estado).Select(x => new Select2DTO_B
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
