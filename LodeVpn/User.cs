using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeVpn
{
    public class User
    {
        public string Name { get; set; }
        public string Gmail { get; set; }
        public string Password { get; set; }
        public bool IsPremium { get; set; }
        public int DayBuySubcribe{ get; set; }
        public DateTime DaysForFreePlan { get; set; }
        public DateTime DaysBuySubcribe { get; set; }
        public DateTime Created { get; set; }
    }
}
