﻿using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class EmpresaBL : Base
    {
        public List<EmpresaDTO> getEmpresas()
        {
            using (var context = getContext())
            {
                var result = context.Empresa.Select(x => new EmpresaDTO
                {
                    IdEmpresa = x.IdEmpresa,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Descripcion = x.Descripcion
                }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }

        public List<EmpresaDTO> getEmpresasActivas()
        {
            using(var context = getContext())
            {
                var result = context.Empresa.Where(x => x.Estado).Select(x => new EmpresaDTO
                    {
                        IdEmpresa = x.IdEmpresa,
                        Nombre = x.Nombre,
                        Estado = x.Estado,
                        Descripcion = x.Descripcion
                    }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }

        public EmpresaDTO getEmpresa(int id)
        {
            using(var context = getContext())
            {
                var result = context.Empresa.Where(x => x.IdEmpresa == id)
                    .Select(r => new EmpresaDTO
                    {
                        IdEmpresa = r.IdEmpresa,
                        Nombre = r.Nombre,
                        Estado = r.Estado,
                        Descripcion = r.Descripcion
                    }).SingleOrDefault();
                return result;
            }
        }
        public bool add(EmpresaDTO Empresa)
        {
            using(var context = getContext())
            {
                try
                {
                    Empresa nuevo = new Empresa();
                    nuevo.Nombre = Empresa.Nombre;
                    nuevo.Estado = Empresa.Estado;
                    nuevo.Descripcion = Empresa.Descripcion;
                    context.Empresa.Add(nuevo);
                    context.SaveChanges();
                    return true;
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(EmpresaDTO Empresa)
        {
            using(var context = getContext())
            {
                try
                {
                    var row = context.Empresa.Where(x => x.IdEmpresa == Empresa.IdEmpresa).SingleOrDefault();
                    row.Nombre = Empresa.Nombre;
                    row.Estado = Empresa.Estado;
                    row.Descripcion = Empresa.Descripcion;
                    context.SaveChanges();
                    return true;
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
