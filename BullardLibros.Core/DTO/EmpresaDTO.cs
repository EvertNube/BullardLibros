﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]  
    public class EmpresaDTO
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public bool Estado { get; set; }
        public string Descripcion { get; set; }
        public Decimal TipoCambio { get; set; }
        public Decimal? IGV { get; set; }
        public int? IdPeriodo { get; set; }
        public int IdMoneda { get; set; }

        public string SimboloMoneda { get; set; }
    }
}
