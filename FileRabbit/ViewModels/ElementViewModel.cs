﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FileRabbit.BLL.BusinessModels.ElementEnums;

namespace FileRabbit.PL.ViewModels
{
    public class ElementViewModel
    {
        public FileType Type { get; set; }

        public string ElemName { get; set; }

        public string LastModified { get; set; }

        public Tuple<double, Unit> Size { get; set; }

    }
}