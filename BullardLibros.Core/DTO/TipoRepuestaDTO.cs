using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    public class TipoRepuestaDTO
    {
        public int IdTipoRespuesta { get; set; }
        public string Nombre { get; set; }
        public int IdPregunta { get; set; }
        public List<OpcionesRespuestaDTO> listaOpcionesRespuesta { get; set; }
    }
}
