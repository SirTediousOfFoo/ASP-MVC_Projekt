using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Vjezba.Model
{
    public class Comment
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Content { get; set; }

        public string PosterName { get; set; }
        public string UserID { get; set; }
        public int ComicID { get; set; }
        public DateTime TimePosted { get; set; }
    }
}