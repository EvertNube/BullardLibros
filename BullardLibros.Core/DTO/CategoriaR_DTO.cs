﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BullardLibros.Core.DTO
{
    [Serializable]
    public class CategoriaR_DTO
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public Decimal MontoTotal { get; set; }
        public int? IdCategoriaPadre { get; set; }
        public int Nivel { get; set; }
        public CategoriaR_DTO Padre { get; set; }
    }
}