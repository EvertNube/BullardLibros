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
                            where ((id == null ? r.IdCategoriaPadre == null : r.IdCategoriaPadre == id) && r.IdEmpresa == idEmpresa)
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
    }
}
