using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hySite;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace hysite.Queries;

public class LastWeekViewsQuery : IRequest<int>
{

}

public class LastWeekViewsQueryHandler : IRequestHandler<LastWeekViewsQuery, int>
{   
    private readonly AppDbContext _dbContext;

    public LastWeekViewsQueryHandler(AppDbContext dbContext) => 
        this._dbContext = dbContext;

    public async Task<int> Handle(LastWeekViewsQuery request, CancellationToken cancellationToken)
    {
        var weekAgo = DateTime.Now.AddDays(-7);
        return await _dbContext.ViewStatistics
                .Where(vs => vs.Date > weekAgo)
                .SumAsync(vs => vs.Count, cancellationToken);
    }
}