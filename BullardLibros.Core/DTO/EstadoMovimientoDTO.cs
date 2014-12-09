﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardEncuestas.Core.DTO
{
    [Serializable]
    public class EstadoMovimientoDTO
    {
        public int IdEstadoMovimiento { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
    }
}
