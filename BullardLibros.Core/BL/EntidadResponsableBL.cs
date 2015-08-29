using BullardLibros.Core.DTO;
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
    public class EntidadResponsableBL : Base
    {
        public List<EntidadResponsableDTO> getEntidadResponsablesEnEmpresa(int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdEmpresa == idEmpresa).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Detraccion = x.Detraccion,
                    Tipo = x.Tipo,
                    IdEmpresa = x.IdEmpresa
                }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }
        public List<EntidadResponsableDTO> getEntidadResponsables()
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Detraccion = x.Detraccion,
                    Tipo = x.Tipo
                }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }

        public List<EntidadResponsableDTO> getEntidadResponsablesEnEmpresaViewBag(int idEmpresa)
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.Estado && x.IdEmpresa == idEmpresa).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Detraccion = x.Detraccion,
                    Tipo = x.Tipo
                }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }

        public List<EntidadResponsableDTO> getEntidadResponsablesViewBag()
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.Estado).Select(x => new EntidadResponsableDTO
                {
                    IdEntidadResponsable = x.IdEntidadResponsable,
                    Nombre = x.Nombre,
                    Estado = x.Estado,
                    Detraccion = x.Detraccion,
                    Tipo = x.Tipo
                }).OrderBy(x => x.Nombre).ToList();
                return result;
            }
        }

        public EntidadResponsableDTO getEntidadResponsableEnEmpresa(int idEmpresa, int id)
        {
            using (var context = getContext())
            {
                var result = context.EntidadResponsable.Where(x => x.IdEntidadResponsable == id && x.IdEmpresa == idEmpresa)
                    .Select(r => new EntidadResponsableDTO
                    {
                        IdEntidadResponsable = r.IdEntidadResponsable,
                        Nombre = r.Nombre,
                        Estado = r.Estado,
                        Detraccion = r.Detraccion,
                        Tipo = r.Tipo,
                        IdEmpresa = r.IdEmpresa
                    }).SingleOrDefault();
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
                        Detraccion = r.Detraccion,
                        Tipo = r.Tipo
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
                    nuevo.Tipo = EntidadResponsable.Tipo;
                    nuevo.IdEmpresa = EntidadResponsable.IdEmpresa;
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
                    datoRow.Tipo = EntidadResponsable.Tipo;
                    datoRow.IdEmpresa = EntidadResponsable.IdEmpresa;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public IList<EntidadResponsableR_DTO> getReporteResumenEntidadesR(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetReporteResumenEntidadesRes(IdCuentaB, FechaInicio, FechaFin).Select(x => new EntidadResponsableR_DTO
                    {
                        IdEntidadResponsable = x.IdEntidadResponsable,
                        Nombre = x.Nombre,
                        Detraccion = x.Detraccion,
                        Monto = x.MontoTotal.GetValueOrDefault(),
                        Tipo = x.Tipo
                    }).ToList();
                return result;
            }
        }
    }
}