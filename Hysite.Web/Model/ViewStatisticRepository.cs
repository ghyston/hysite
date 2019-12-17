using System;
using System.Linq;

namespace hySite
{
    public class ViewStatisticRepository : IViewStatisticRepository
    {
        private AppDbContext _dbContext;

        public ViewStatisticRepository(AppDbContext db)
        {
            _dbContext = db;
        }

        public void Add(ViewStatistic stat)
        {
            _dbContext.Add(stat);
        }
        
        public ViewStatistic GetByDate(DateTime date)
        {
            //@todo: DateTime.Compare how works?
            return _dbContext.ViewStatistics
                .FirstOrDefault(vs => 
                vs.Date.Day == date.Day && 
                vs.Date.Month == date.Month &&
                vs.Date.Year == date.Year);
        }

        public int ViewsFrom(DateTime date)
        {
            return _dbContext.ViewStatistics
                .Where(vs => vs.Date > date)
                .Sum(vs => vs.Count);
        }

    }
}