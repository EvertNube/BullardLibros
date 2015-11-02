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
        public IList<CategoriaR_DTO> AvanceDePresupuesto(int? IdCuentaB, DateTime? FechaInicio, DateTime? FechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_Rep_AvanceDePresupuesto(IdCuentaB, FechaInicio, FechaFin).Select(r => new CategoriaR_DTO
                {
                    IdCategoria = r.IdCategoria,
                    Nombre = r.Nombre,
                    MontoTotal = r.MontoTotal.GetValueOrDefault(),
                    IdCategoriaPadre = r.IdCategoriaPadre
                }).ToList();
                return result;
            }
        }
    }
}
