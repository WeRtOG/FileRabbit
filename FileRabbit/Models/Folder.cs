using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.Models
{
    public class Folder
    {
        [Required]
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string Path { get; set; }

        public string ParentFolderId { get; set; }
        
        [Required]
        public string OwnerId { get; set; }

        [Required]
        public bool IsShared { get; set; }
    }
}
