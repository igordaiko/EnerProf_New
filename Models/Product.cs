using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnerProf.Models
{
    public class Product
    {
        public int Id { get; set; }
        public Company Company { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        [Column(TypeName = "ntext")]
        public string Description { get; set; }
        public string Images { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Model> Models { get; set; }
    }

    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Product Product { get; set; }
        public virtual ICollection<AttributesValues> Properties { get; set; }
    }
}
