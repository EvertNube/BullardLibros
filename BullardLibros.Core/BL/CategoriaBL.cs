﻿using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class CategoriaBL : Base
    {
        public List<CategoriaDTO> getCategorias()
        {
            using (var context = getContext())
            {
                var result = context.Categoria.Select(x => new CategoriaDTO
                    {
                        IdCategoria = x.IdCategoria,
                        Nombre = x.Nombre,
                        Orden = x.Orden,
                        Estado = x.Estado,
                        IdCategoriaPadre = x.IdCategoriaPadre
                    }).ToList();
                return result;
            }
        }

        public IList<CategoriaDTO> getCategoriasTree(int? id = null)
        {
            using (var context = getContext())
            {
                var result = from r in context.Categoria
                             where (id == null ? r.IdCategoriaPadre == null : r.IdCategoriaPadre == id)
                             select new CategoriaDTO
                             {
                                 IdCategoria = r.IdCategoria,
                                 Nombre = r.Nombre,
                                 Orden = r.Orden,
                                 Estado = r.Estado,
                                 IdCategoriaPadre = r.IdCategoriaPadre
                             };
                IList<CategoriaDTO> categoriasTree = result.AsEnumerable<CategoriaDTO>().OrderBy(x => x.Orden).ToList<CategoriaDTO>();

                foreach (CategoriaDTO obj in categoriasTree)
                {
                    obj.Hijos = getCategoriasTree(obj.IdCategoria);
                }
                return categoriasTree;
            }
        }

        public IList<CategoriaDTO> getCategoriasPadre(bool AsSelectList = false)
        {
            if (!AsSelectList)
                return getCategoriasTree();
            else
            {
                var lista = getCategoriasTree();
                lista.Insert(0, new CategoriaDTO() { IdCategoria = 0, Nombre = "Seleccione la Categoría Padre" });
                return lista;
            }
        }

        public CategoriaDTO getCategoria(int id)
        {
            using (var context = getContext())
            {
                var result = context.Categoria.Where(x => x.IdCategoria == id)
                    .Select(r => new CategoriaDTO
                    {
                        IdCategoria = r.IdCategoria,
                        Nombre = r.Nombre,
                        Orden = r.Orden,
                        Estado = r.Estado,
                        IdCategoriaPadre = r.IdCategoriaPadre
                    }).SingleOrDefault();
                return result;
            }
        }
        public bool add(CategoriaDTO Categoria)
        {
            using (var context = getContext())
            {
                try
                {
                    Categoria nuevo = new Categoria();
                    nuevo.Nombre = Categoria.Nombre;
                    if (Categoria.IdCategoriaPadre != 0 && Categoria.IdCategoriaPadre != null)
                        nuevo.Orden = getUltimoHijo(Categoria.IdCategoriaPadre.GetValueOrDefault());
                    else
                        nuevo.Orden = 1;
                    //nuevo.Orden = Categoria.Orden;
                    nuevo.Estado = true;
                    nuevo.IdCategoriaPadre = Categoria.IdCategoriaPadre;
                    context.Categoria.Add(nuevo);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(CategoriaDTO Categoria)
        {
            using (var context = getContext())
            {
                try
                {
                    var datoRow = context.Categoria.Where(x => x.IdCategoria == Categoria.IdCategoria).SingleOrDefault();
                    datoRow.Nombre = Categoria.Nombre;
                    //datoRow.Orden = Categoria.Orden;
                    if (Categoria.IdCategoriaPadre != 0 && Categoria.IdCategoriaPadre != null)
                        datoRow.Orden = getUltimoHijo(Categoria.IdCategoriaPadre.GetValueOrDefault());
                    else
                        datoRow.Orden = 1;
                    datoRow.Estado = Categoria.Estado;
                    datoRow.IdCategoriaPadre = Categoria.IdCategoriaPadre;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public int getUltimoHijo(int idPadre)
        {
            using (var context = getContext())
            {
                var result = from r in context.Categoria.Where(x => x.IdCategoriaPadre == idPadre)
                             select new CategoriaDTO
                             {
                                 IdCategoria = r.IdCategoria,
                                 Nombre = r.Nombre,
                                 Orden = r.Orden,
                                 Estado = r.Estado,
                                 IdCategoriaPadre = r.IdCategoriaPadre
                             };
                IList<CategoriaDTO> lstCategorias = result.AsEnumerable<CategoriaDTO>().OrderBy(x => x.Orden).ToList<CategoriaDTO>();
                if (lstCategorias.Count == 0)
                    return 1;
                return lstCategorias.Last().Orden + 1;
            }
        }

        public string getNombreCategoria(int id)
        {
            if (id != 0 && id != null)
            {
                CategoriaBL oBL = new CategoriaBL();
                return oBL.getCategoria(id).Nombre;
            }
            return "Sin Categoría";
        }

        public IList<CategoriaR_DTO> getReporteCategorias(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetReporteResumenCategorias(IdCuentaB, FechaInicio, FechaFin).Select(r => new CategoriaR_DTO
                {
                    IdCategoria = r.IdCategoria,
                    Nombre = r.Nombre,
                    MontoTotal = r.MontoTotal.GetValueOrDefault(),
                    IdCategoriaPadre = r.IdCategoriaPadre
                }).ToList();
                return result;
            }
        }

        public CategoriaR_DTO obtenerPadreEntidadReporte(CategoriaR_DTO obj, List<CategoriaDTO> lstCategorias, int Nivel)
        {
            if (obj.IdCategoriaPadre != null)
            {
                CategoriaR_DTO nuevo = new CategoriaR_DTO();
                CategoriaDTO aux = lstCategorias.Find(x => x.IdCategoria == obj.IdCategoriaPadre);
                nuevo.IdCategoria = aux.IdCategoria;
                nuevo.Nombre = aux.Nombre;
                nuevo.IdCategoriaPadre = aux.IdCategoriaPadre;

                if (nuevo.IdCategoriaPadre != null)
                {
                    nuevo = obtenerPadreEntidadReporte(nuevo, lstCategorias, Nivel);
                    nuevo.Nivel = nuevo.Padre.Nivel + 1;
                    if (CONSTANTES.NivelCat < Nivel)
                        CONSTANTES.NivelCat = Nivel;
                }
                else
                { nuevo.Nivel = Nivel; }
                obj.Padre = nuevo;
                obj.Nivel = nuevo.Nivel + 1;
            }
            return obj;
        }
    }
}
