using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Shared_Entities
{
    public class ProductStockInfoDTO
    {
        public bool IsOutOfStock { get; set; }

        public int StockCount { get; set; } 
    }
}
