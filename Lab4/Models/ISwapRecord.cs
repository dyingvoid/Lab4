﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Models
{
    internal interface ISwapRecord : IRecord
    {
        public bool Swapped { get; set; }
    }
}
