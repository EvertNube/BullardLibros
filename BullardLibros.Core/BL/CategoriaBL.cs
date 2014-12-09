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
                    nuevo.Orden = Categoria.Orden;
                    nuevo.Estado = Categoria.Estado;
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
                    datoRow.Orden = Categoria.Orden;
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
    }
}
