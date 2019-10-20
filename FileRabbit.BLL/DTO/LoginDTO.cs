using System;
using System.Collections.Generic;
using System.Text;

namespace FileRabbit.BLL.DTO
{
    public class LoginDTO
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool Remember { get; set; }
    }
}
