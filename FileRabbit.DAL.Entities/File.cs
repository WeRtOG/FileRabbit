using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Entities
{
    public class File
    {
        [Required]
        [MaxLength(36)]
        [Key]
        public string Id { get; set; }

        [Required]
        public string Path { get; set; }

        [MaxLength(36)]
        [Required]
        public string FolderId { get; set; }

        public Folder Folder { get; set; }

        [Required]
        public bool IsShared { get; set; }
    }
}
