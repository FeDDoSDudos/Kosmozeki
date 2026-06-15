using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Domain.Weapon;

public interface IWeaponRepository
{
    Task<List<Weapon>> GetAllAsync(CancellationToken ct = default);
    Task<Weapon> UpsertAsync(Weapon weapon, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
