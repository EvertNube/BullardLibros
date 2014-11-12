using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    public class PreguntaDTO
    {
        public int IdPregunta { get; set; }
        public string Descripcion { get; set; }
        public int IdSeccion { get; set; }
        public int Orden { get; set; }
        public List<RespuestasDTO> listaRespuestas { get; set; }
        public List<TipoRepuestaDTO> listaTipoRespuestas { get; set; }
    }
}
