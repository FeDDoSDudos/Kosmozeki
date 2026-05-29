using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
