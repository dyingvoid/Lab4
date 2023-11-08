using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab4.ViewModels;

namespace Lab4.Models
{
    internal interface ISort
    {
        public void Sort<T>(Batch<T> batch);
    }
}
