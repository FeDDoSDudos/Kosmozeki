using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Domain.Notes;

public interface INoteRepository
{
    Task<SharedNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpsertAsync(SharedNote note, CancellationToken cancellationToken = default);
}
