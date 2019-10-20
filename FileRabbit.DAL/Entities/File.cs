using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Entites
{
    public class File
    {
        [Required]
        [Key]
        public string Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public string FolderId { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [Required]
        public bool IsShared { get; set; }
    }
}
