using System;
using System.IO;

namespace hySite
{
    public class IncrementViewsHandlerRequest {}

    public class IncrementViewsHandlerResponse {}

    public class IncrementViewsHandlerHandler : IHandler<IncrementViewsHandlerRequest, IncrementViewsHandlerResponse>
    {
        private readonly IViewStatisticRepository _viewStatisticRepository;
        private AppDbContext _dbContext;

        public IncrementViewsHandlerHandler(IViewStatisticRepository viewStatisticRepository, AppDbContext dbContext)
        {
            _viewStatisticRepository = viewStatisticRepository;
            _dbContext = dbContext;
        }

        public IncrementViewsHandlerResponse Handle(IncrementViewsHandlerRequest request)
        {
            var forToday = _viewStatisticRepository.GetByDate(DateTime.Today);
            if(forToday == null)
            {
                forToday = new ViewStatistic
                {
                    Date = DateTime.Today,
                    Count = 0
                };
                _viewStatisticRepository.Add(forToday);
            }

            forToday.Increment();
            _dbContext.SaveChanges();
            return new IncrementViewsHandlerResponse();
        }
    }
}
