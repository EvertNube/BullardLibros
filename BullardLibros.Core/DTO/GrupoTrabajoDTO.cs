using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    public class GrupoTrabajoDTO
    {
        public int IdGrupoTrabajo { get; set; }
        public string Nombre { get; set; }
        public List<EncuestaDTO> listaEncuesta { get; set; }
        public List<PersonaDTO> listaPersona { get; set; }
    }
}
