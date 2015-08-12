using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BullardLibros.Models
{
    public class Navbar
    {
        public string primaryActive { get; set; }
        public string successActive { get; set; }
        public string infoActive { get; set; }
        public string warningActive { get; set; }
        public string dangerActive { get; set; }

        public Navbar()
        {
            this.primaryActive = "";
            this.successActive = "";
            this.infoActive = "";
            this.warningActive = "";
            this.dangerActive = "";
        }

        public void clearAll()
        {
            this.primaryActive = "";
            this.successActive = "";
            this.infoActive = "";
            this.warningActive = "";
            this.dangerActive = "";
        }

        public void activeAll()
        {
            this.primaryActive = "active";
            this.successActive = "active";
            this.infoActive = "active";
            this.warningActive = "active";
            this.dangerActive = "active";
        }
    }
}