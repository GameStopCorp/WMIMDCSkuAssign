using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyChain.Svc.MSDCSKUAssign.Controllers
{
   public interface IMsdcController
    {
        ResultProcessMultiDCSkus ProcessMultiDCSkus(bool isReporting);
    }
}
