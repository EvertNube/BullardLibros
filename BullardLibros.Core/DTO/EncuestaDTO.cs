using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    public class EncuestaDTO
    {
        public int IdEncuesta { get; set; }
        public string Nombre { get; set; }
        public int IdPeriodo { get; set; }
        public int IdGrupoEncuestado { get; set; }

        public List<EncuestaEvaluadorDTO> listaEncuestaEvaluador { get; set; }

    }
}
