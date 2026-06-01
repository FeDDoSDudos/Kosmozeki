using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Domain.Character;

public interface ICharacterRepository
{
    Task<Character?> GetFirstAsync(CancellationToken ct = default);
    Task<Character> UpsertAsync(Character character, CancellationToken ct = default);
}
