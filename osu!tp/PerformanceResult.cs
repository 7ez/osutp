using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osutp
{
    public class TpPerformanceResult
    {
        public double Total;
        public double Aim;
        public double Speed;
        public double Acc;

        internal TpPerformanceResult()
        {
            Total = 0.0;
            Aim = 0.0;
            Speed = 0.0;
            Acc = 0.0;
        }
    }
}
