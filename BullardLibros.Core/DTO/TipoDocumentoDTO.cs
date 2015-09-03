﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class TipoDocumentoDTO
    {
        public int IdTipoEntidad { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
    }
}