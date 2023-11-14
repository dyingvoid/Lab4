using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Models
{
    internal interface IMergeRecord : IRecord
    {
        public int InsertedId { get; set; }
    }
}
