using System;
using System.Collections.Generic;

namespace hySite
{
    public interface IViewStatisticRepository
    {
        void Add(ViewStatistic stat);
        ViewStatistic GetByDate(DateTime date);
        int ViewsFrom(DateTime date);
    }
}
