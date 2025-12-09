using System;
using System.Collections.Generic;

namespace ComputerStore.Models
{
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Sale> Sales { get; set; }
        public decimal TotalRevenue { get; set; } 
    }
}