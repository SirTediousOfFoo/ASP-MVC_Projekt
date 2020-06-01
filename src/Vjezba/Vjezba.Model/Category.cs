using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Text;

namespace Vjezba.Model
{
    public class Category
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Tag { get; set; }

        public virtual ICollection<Comic> Comics { get; set; }
    }
}
