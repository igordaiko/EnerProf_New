using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnerProf.Models
{
    public class Industries
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        [Column(TypeName = "ntext")]
        public string Description { get; set; }
    }
}