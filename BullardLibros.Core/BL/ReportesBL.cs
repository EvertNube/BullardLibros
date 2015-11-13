using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class ReportesBL : Base
    {
        #region Avance de Presupuesto
        public List<CategoriaDTO> AvanceDePresupuesto(int? idEmpresa, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_Rep_AvanceDePresupuesto(idEmpresa, fechaInicio, fechaFin).Select(x => new CategoriaDTO
                {
                    IdCategoria = x.IdCategoria,
                    Nombre = x.Nombre,
                    Orden = x.Orden,
                    Estado = x.Estado,
                    IdCategoriaPadre = x.IdCategoriaPadre,
                    IdEmpresa = x.IdEmpresa,
                    Presupuesto = x.MontoFinal
                }).ToList();
                return result;
            }
        }

        public List<CategoriaDTO> getCategoriasTreeEnEmpresa(List<CategoriaDTO> lstCatsMontos, int idEmpresa, int? id = null)
        {
            
            var result = from r in lstCatsMontos
                            where ((id == null ? r.IdCategoriaPadre == null : r.IdCategoriaPadre == id) && r.Estado && r.IdEmpresa == idEmpresa)
                            select new CategoriaDTO
                            {
                                IdCategoria = r.IdCategoria,
                                Nombre = r.Nombre,
                                Orden = r.Orden,
                                Estado = r.Estado,
                                IdCategoriaPadre = r.IdCategoriaPadre,
                                IdEmpresa = r.IdEmpresa,
                                Presupuesto = lstCatsMontos.SingleOrDefault(x => x.IdCategoria == r.IdCategoria).Presupuesto
                            };
            List<CategoriaDTO> categoriasTree = result.AsEnumerable<CategoriaDTO>().OrderBy(x => x.Orden).ToList<CategoriaDTO>();

            foreach (CategoriaDTO obj in categoriasTree)
            {
                obj.Hijos = getCategoriasTreeEnEmpresa(lstCatsMontos, idEmpresa, obj.IdCategoria);
                if(obj.Hijos.Count() > 0)
                {
                    obj.Presupuesto = lstCatsMontos.Where(x => x.IdCategoriaPadre == obj.IdCategoria).Sum(x => x.Presupuesto);
                    lstCatsMontos.SingleOrDefault(x => x.IdCategoria == obj.IdCategoria).Presupuesto = obj.Presupuesto;
                }
            }

            return categoriasTree;
        }

        public List<CategoriaDTO> getCategoriasPresupuestosTreeEnEmpresa(int idEmpresa, int nivel, int? id = null)
        {
            using (var context = getContext())
            {
                var result = from r in context.Categoria
                             where ((id == null ? r.IdCategoriaPadre == null : r.IdCategoriaPadre == id) && r.Estado && r.IdEmpresa == idEmpresa)
                             select new CategoriaDTO
                             {
                                 IdCategoria = r.IdCategoria,
                                 Nombre = r.Nombre,
                                 Orden = r.Orden,
                                 Estado = r.Estado,
                                 IdCategoriaPadre = r.IdCategoriaPadre,
                                 IdEmpresa = r.IdEmpresa,
                                 Nivel = nivel,
                                 Presupuesto = r.CategoriaPorPeriodo.Where(x => x.IdPeriodo == r.Empresa.IdPeriodo).FirstOrDefault().Monto
                             };
                List<CategoriaDTO> categoriasTree = result.AsEnumerable<CategoriaDTO>().OrderBy(x => x.Orden).ToList<CategoriaDTO>();

                foreach (CategoriaDTO obj in categoriasTree)
                {
                    obj.Hijos = getCategoriasPresupuestosTreeEnEmpresa(idEmpresa, nivel+1, obj.IdCategoria);
                }
                return categoriasTree;
            }
        }
        #endregion

        #region Facturacion por areas
        public List<AreaDTO> getAreasEnEmpresa(int idEmpresa, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = getContext())
            {
                var result = context.Area.AsEnumerable().Where(x => x.IdEmpresa == idEmpresa && x.Estado).Select(x => new AreaDTO
                {
                    IdArea = x.IdArea,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Estado = x.Estado,
                    IdEmpresa = x.IdEmpresa,
                    lstClientes = context.SP_Rep_FacturacionPorAreas(x.IdArea, x.IdEmpresa, fechaInicio, fechaFin).Select(r => new EntidadResponsableR_DTO 
                    {
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        Nombre = r.Nombre,
                        Monto = r.Monto.GetValueOrDefault()
                    }).ToList()
                }).OrderBy(x => x.Nombre).ToList();

                foreach (var item in result)
                {
                    item.SumaClientes = item.lstClientes.Sum(x => x.Monto);
                }

                return result;
            }
        }
        #endregion

        #region Ingresos y Egresos por Areas
        public List<AreaDTO> getIngresosEgresosAreas(int idEmpresa, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_Rep_IngresosEgresosPorAreas(idEmpresa, fechaInicio, fechaFin).Select(x => new AreaDTO
                    {
                        IdArea = x.IdArea,
                        Nombre = x.Nombre,
                        Descripcion = x.Descripcion,
                        Estado = x.Estado,
                        IdEmpresa = x.IdEmpresa,
                        Ingresos = x.Ingreso.GetValueOrDefault(),
                        Egresos = x.Egreso.GetValueOrDefault()
                    }).OrderBy(x => x.Nombre).ToList();
                
                return result;
            }
        }
        #endregion
    }
}
