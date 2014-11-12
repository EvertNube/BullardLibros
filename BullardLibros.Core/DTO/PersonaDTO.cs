using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class PersonaDTO
    {
        public int IdPersona { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int IdEmpresa { get; set; }
        public int IdGrupoTrabajo { get; set; }
        public List<EncuestaEvaluadorDTO> listaEncuestaEvaluador { get; set; }
        public List<EncuestaEvaluadorDTO> listaEncuestaEvaluado { get; set; }
        
    }
}
