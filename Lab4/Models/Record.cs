using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4.Models
{
    public class Record : ISwapRecord, IMergeRecord
    {
        public int RowIndex1 { get; set; }
        public int RowIndex2 { get; set; }
        public bool Swapped { get; set; }
        public int InsertedId { get; set; }
    }
}
