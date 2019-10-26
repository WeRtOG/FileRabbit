using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FileRabbit.DAL.Entities
{
    public class Folder
    {
        [Required]
        [MaxLength(36)]
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string Path { get; set; }

        [MaxLength(36)]
        public string ParentFolderId { get; set; }

        [ForeignKey("ParentFolderId")]
        public Folder ParentFolder { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User User { get; set; }

        [Required]
        public bool IsShared { get; set; }

        public ICollection<Folder> Folders { get; set; }

        public ICollection<File> Files { get; set; }

        public Folder()
        {
            Folders = new List<Folder>();
            Files = new List<File>();
        }
    }
}
