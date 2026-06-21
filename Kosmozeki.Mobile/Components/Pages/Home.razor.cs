using Kosmozeki.Core.Services;
using Kosmozeki.Domain.Character;
using Kosmozeki.Domain.Weapon;
using Kosmozeki.Mobile.Services;
using Microsoft.AspNetCore.Components;

namespace Kosmozeki.Mobile.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] protected CombatFacade CombatFacade { get; set; } = default!;
    [Inject] protected IInspirationState InspirationState { get; set; } = default!;

    protected Character _hero = new("Мой Космозэк");
    protected int _incomingDamage = 0;
    protected string _incomingZone = "Torso";

    protected List<Weapon> _arsenal = new();
    protected int ActiveWeaponIndex { get; set; } = 0;
    protected Weapon? ActiveWeapon => _arsenal.Count > 0 ? _arsenal[ActiveWeaponIndex] : null;

    protected string _newWepName = "";
    protected int _newWepDmg = 0, _newWepDif = 0, _newWepAmmo = 0, _newWepRof = 0;

    protected int _bulletsFired = 1;
    protected int _customModifier = 0;
    protected List<string> _combatLog = new();

    protected int CalculatedFinalDIF => ActiveWeapon is not null
        ? ActiveWeapon.BaseDIF - _bulletsFired + _customModifier
        : 0;

    protected int Inspiration
    {
        get => InspirationState.Value;
        set => InspirationState.Set(value);
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await CombatFacade.LoadAsync();
        _hero = state.Hero;
        _arsenal = state.Arsenal.ToList();
    }    

    protected void ChangeInspiration(int delta)
    {
        InspirationState.Change(delta);
    }
    protected void OnInspirationChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var value))
        {
            Inspiration = value;
        }
        else
        {
            Inspiration = 0;
        }
    }

    protected async Task TakeDamageAsync()
    {
        if (_incomingDamage <= 0) return;

        var zone = _incomingZone switch
        {
            "Head" => BodyPartType.Head,
            "LeftArm" => BodyPartType.LeftArm,
            "RightArm" => BodyPartType.RightArm,
            "LeftLeg" => BodyPartType.LeftLeg,
            "RightLeg" => BodyPartType.RightLeg,
            _ => BodyPartType.Torso
        };

        var actualDamage = await CombatFacade.ApplyDamageAsync(_hero, zone, _incomingDamage);
        var part = _hero.BodyParts[zone];

        AddLog($"<span style='color:red;'>[УРОН]</span> В {part.Name} прошло {actualDamage} ед. урона.");
        if (part.IsDestroyed)
            AddLog($"<span style='color:red; font-weight:bold;'>КРИТ! {part.Name} сломана!</span>");

        _incomingDamage = 0;
    }

    protected async Task AddNewWeaponAsync()
    {
        if (string.IsNullOrWhiteSpace(_newWepName)) return;

        var weapon = new Weapon(_newWepName, _newWepDmg, 0, _newWepAmmo, _newWepRof, _newWepDif);
        weapon = await CombatFacade.SaveWeaponAsync(weapon);

        _arsenal.Add(weapon);
        ActiveWeaponIndex = _arsenal.Count - 1;
        AddLog($"[АРСЕНАЛ] Добавлено: {_newWepName}");
    }

    protected async Task ReloadActiveWeaponAsync()
    {
        if (ActiveWeapon is null) return;

        ActiveWeapon.CurrentAmmo = ActiveWeapon.MaxAmmo;
        await CombatFacade.SaveWeaponAsync(ActiveWeapon);
        AddLog($"[ПЕРЕЗАРЯДКА] {ActiveWeapon.Name} готов.");
    }

    protected async Task RollAndShootAsync()
    {
        if (ActiveWeapon is null) return;

        if (_bulletsFired > ActiveWeapon.CurrentAmmo || _bulletsFired > ActiveWeapon.ROF)
        {
            AddLog("<span style='color:orange;'>Ошибка: Не хватает патронов или превышен ROF!</span>");
            return;
        }

        var diceRoll = Random.Shared.Next(1, 101);

        try
        {
            var result = CombatFacade.Shoot(ActiveWeapon, _bulletsFired, diceRoll, _customModifier);
            await CombatFacade.SaveWeaponAsync(ActiveWeapon);

            string logEntry = $"<span style='color:lightblue;'>[АТАКА]</span> Пуль: {_bulletsFired}. <strong>Кубик: {diceRoll}</strong> <em>(Нужно: {CalculatedFinalDIF})</em>. ";

            if (result.IsHit)
                logEntry += $"<br/>🎯 <strong>ПОПАДАНИЕ!</strong> Зашло пуль: {result.HitsCount}. Урон: <strong>{result.TotalDamage}</strong>.";
            else
                logEntry += "<br/>💨 <strong>ПРОМАХ!</strong>";

            AddLog(logEntry);
        }
        catch (Exception ex)
        {
            AddLog($"Ошибка: {ex.Message}");
        }
    }

    protected async Task DeleteActiveWeaponAsync()
    {
        if (ActiveWeapon is null || _arsenal.Count <= 1) return;

        var weaponName = ActiveWeapon.Name;
        await CombatFacade.DeleteWeaponAsync(ActiveWeapon.Id);
        _arsenal.RemoveAt(ActiveWeaponIndex);
        ActiveWeaponIndex = 0;

        AddLog($"[АРСЕНАЛ] Оружие <b>{weaponName}</b> удалено.");
    }

    protected async Task AutoSaveAsync() =>
        await CombatFacade.SaveCharacterAsync(_hero);

    protected void AddLog(string message) =>
        _combatLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
}