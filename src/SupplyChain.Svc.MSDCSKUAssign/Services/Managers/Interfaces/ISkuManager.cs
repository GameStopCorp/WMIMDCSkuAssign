﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces
{
    public interface ISkuManager<T>
    {
        List<T> GetSummarySkuRecords();

        List<T> GetOffsiteDailyRecords();

    }
}
