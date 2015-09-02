using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BullardLibros.Models
{
    public class Navbar
    {
        public string menu1 { get; set; }
        public string menu2 { get; set; }
        public string menu3 { get; set; }
        public string menu4 { get; set; }
        public string menu5 { get; set; }
        public string menu6 { get; set; }
        public string menu7 { get; set; }
        public string menu8 { get; set; }

        public Navbar()
        {
            this.menu1 = "";
            this.menu2 = "";
            this.menu3 = "";
            this.menu4 = "";
            this.menu5 = "";
            this.menu6 = "";
            this.menu7 = "";
            this.menu8 = "";
        }

        public void clearAll()
        {
            this.menu1 = "";
            this.menu2 = "";
            this.menu3 = "";
            this.menu4 = "";
            this.menu5 = "";
            this.menu6 = "";
            this.menu7 = "";
            this.menu8 = "";
        }

        public void activeAll()
        {
            this.menu1 = "active";
            this.menu2 = "active";
            this.menu3 = "active";
            this.menu5 = "active";
            this.menu4 = "active";
            this.menu6 = "active";
            this.menu7 = "active";
            this.menu8 = "active";
        }
    }
}