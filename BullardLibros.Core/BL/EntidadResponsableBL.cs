using BullardLibros.Core.DTO;
using BullardLibros.Data;
using BullardLibros.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.BL
{
    public class EntidadResponsableBL : Base
    {
        public List<EntidadResponsableDTO> getEntidadResponsables()
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Detraccion = x.Detraccion
                }).ToList();
                return result;
            }
        }

        public EntidadResponsableDTO getEntidadResponsable(int id)
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdEntidadResponsable == id)
                    .Select(r => new EntidadResponsableDTO
                    {
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        Nombre = r.Nombre,
                        Estado = r.Estado,
                        Detraccion = r.Detraccion
                    }).SingleOrDefault();
                return result;
            }
        }
        public bool add(EntidadResponsableDTO EntidadResponsable)
        {
            using (var context = getContext())
            {
                try
                {
                    EntidadResponsable nuevo = new EntidadResponsable();
                    nuevo.Nombre = EntidadResponsable.Nombre;
                    nuevo.Estado = true;
                    nuevo.Detraccion = EntidadResponsable.Detraccion;
                    context.EntidadResponsable.Add(nuevo);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool update(EntidadResponsableDTO EntidadResponsable)
        {
            using (var context = getContext())
            {
                try
                {
                    var datoRow = context.EntidadResponsable.Where(x => x.IdEntidadResponsable == EntidadResponsable.IdEntidadResponsable).SingleOrDefault();
                    datoRow.Nombre = EntidadResponsable.Nombre;
                    datoRow.Estado = EntidadResponsable.Estado;
                    datoRow.Detraccion = EntidadResponsable.Detraccion;
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