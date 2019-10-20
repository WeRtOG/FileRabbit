using System;
using System.Collections.Generic;
using System.Text;
using static FileRabbit.BLL.BusinessModels.ElementEnums;

namespace FileRabbit.BLL.DTO
{
    public class ElementDTO
    {
        public FileType Type { get; set; }

        public string ElemName { get; set; }

        public string LastModified { get; set; }

        public Tuple<double, Unit> Size { get; set; }
    }
}
