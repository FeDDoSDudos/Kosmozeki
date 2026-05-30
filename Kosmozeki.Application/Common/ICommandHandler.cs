using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
