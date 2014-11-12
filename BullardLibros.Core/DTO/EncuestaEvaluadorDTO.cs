using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    public class EncuestaEvaluadorDTO
    {
        public int IdEncuestaEvaluador { get; set; }
        public int IdEncuesta { get; set; }
        public int IdEvaluador { get; set; }
        public int CodEvaluador { get; set; }
        public bool EstadoEncuesta { get; set; }
        public int IdEvaluado { get; set; }
        public List<RespuestasDTO> listaRespuestas { get; set; }
    }
}
