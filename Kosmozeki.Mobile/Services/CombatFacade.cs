using Kosmozeki.Core.Services;
using Kosmozeki.Domain.Character;
using Kosmozeki.Domain.Weapon;

namespace Kosmozeki.Mobile.Services;

public sealed class CombatFacade
{
    private readonly ICharacterRepository _characters;
    private readonly IWeaponRepository _weapons;
    private readonly CombatEngineService _combat;

    public CombatFacade(
        ICharacterRepository characters,
        IWeaponRepository weapons,
        CombatEngineService combat)
    {
        _characters = characters;
        _weapons = weapons;
        _combat = combat;
    }

    public async Task<(Character Hero, IReadOnlyList<Weapon> Arsenal)> LoadAsync(CancellationToken ct = default)
    {
        var hero = await _characters.GetFirstAsync(ct);
        if (hero is null)
        {
            hero = new Character("Мой Космозэк");
            hero = await _characters.UpsertAsync(hero, ct);
        }

        var weapons = await _weapons.GetAllAsync(ct);
        if (weapons.Count == 0)
        {
            var pistol = new Weapon("Стандартный пистолет", 20, 0, 7, 2, 30);
            var rifle = new Weapon("Штурмовая винтовка", 15, 0, 30, 30, 50);

            await _weapons.UpsertAsync(pistol, ct);
            await _weapons.UpsertAsync(rifle, ct);

            weapons = await _weapons.GetAllAsync(ct);
        }

        return (hero, weapons);
    }

    public async Task<int> ApplyDamageAsync(Character hero, BodyPartType zone, int damage, CancellationToken ct = default)
    {
        var part = hero.BodyParts[zone];
        var actualDamage = part.TakeDamage(damage, applyArmor: true);
        await _characters.UpsertAsync(hero, ct);
        return actualDamage;
    }

    public Task<Weapon> SaveWeaponAsync(Weapon weapon, CancellationToken ct = default) =>
        _weapons.UpsertAsync(weapon, ct);

    public Task DeleteWeaponAsync(int id, CancellationToken ct = default) =>
        _weapons.DeleteAsync(id, ct);

    public async Task SaveCharacterAsync(Character hero, CancellationToken ct = default) =>
        await _characters.UpsertAsync(hero, ct);

    public AttackResult Shoot(Weapon weapon, int bulletsFired, int diceRoll, int modifier = 0) =>
        _combat.CalculateRangedAttack(weapon, bulletsFired, diceRoll, modifier);
}
