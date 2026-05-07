using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Kosmozeki.Core.Models;
using Kosmozeki.Core.Services;
using Kosmozeki.Core.Data;

namespace Kosmozeki.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] protected CombatEngineService CombatService { get; set; } = default!;
        [Inject] protected AppDbContext DbContext { get; set; } = default!;

        protected Character _hero = new Character("Мой Космозэк");
        protected int _incomingDamage = 0;
        protected string _incomingZone = "Torso";

        protected List<RangedWeapon> _arsenal = new();
        protected int ActiveWeaponIndex { get; set; } = 0;
        protected RangedWeapon? ActiveWeapon => _arsenal.Count > 0 ? _arsenal[ActiveWeaponIndex] : null;

        protected string _newWepName = "";
        protected int _newWepDmg = 0, _newWepDif = 0, _newWepAmmo = 0, _newWepRof = 0;

        protected int _bulletsFired = 1;
        protected int _customModifier = 0; // Заменили штраф на универсальный модификатор
        protected List<string> _combatLog = new();

        // Свойство для подсчета итогового DIF в реальном времени для интерфейса
        protected int CalculatedFinalDIF => ActiveWeapon != null
            ? ActiveWeapon.BaseDIF - _bulletsFired + _customModifier
            : 0;

        protected override void OnInitialized()
        {
            var savedHero = DbContext.Characters.FirstOrDefault();
            if (savedHero != null) _hero = savedHero;
            else DbContext.Characters.Add(_hero);

            var savedWeapons = DbContext.Arsenal.ToList();
            if (savedWeapons.Any()) _arsenal = savedWeapons;
            else
            {
                var pistol = new RangedWeapon("Стандартный пистолет", 20, 0, 7, 2, 30);
                var rifle = new RangedWeapon("Штурмовая винтовка", 15, 0, 30, 30, 50);
                DbContext.Arsenal.AddRange(pistol, rifle);
                _arsenal.Add(pistol); _arsenal.Add(rifle);
            }
            DbContext.SaveChanges();
        }

        protected void TakeDamage()
        {
            if (_incomingDamage <= 0) return;
            BodyPartType zone = _incomingZone switch
            {
                "Head" => BodyPartType.Head,
                "LeftArm" => BodyPartType.LeftArm,
                "RightArm" => BodyPartType.RightArm,
                "LeftLeg" => BodyPartType.LeftLeg,
                "RightLeg" => BodyPartType.RightLeg,
                _ => BodyPartType.Torso
            };

            var part = _hero.BodyParts[zone];
            int actualDamage = part.TakeDamage(_incomingDamage, applyArmor: true);

            AddLog($"<span style='color:red;'>[УРОН]</span> В {part.Name} прошло {actualDamage} ед. урона.");
            if (part.IsDestroyed) AddLog($"<span style='color:red; font-weight:bold;'>КРИТ! {part.Name} сломана!</span>");
            _incomingDamage = 0;
            DbContext.SaveChanges();
        }

        protected void AddNewWeapon()
        {
            if (string.IsNullOrWhiteSpace(_newWepName)) return;
            var weapon = new RangedWeapon(_newWepName, _newWepDmg, 0, _newWepAmmo, _newWepRof, _newWepDif);
            _arsenal.Add(weapon);
            DbContext.Arsenal.Add(weapon);
            DbContext.SaveChanges();
            ActiveWeaponIndex = _arsenal.Count - 1;
            AddLog($"[АРСЕНАЛ] Добавлено: {_newWepName}");
        }

        protected void ReloadActiveWeapon()
        {
            if (ActiveWeapon == null) return;
            ActiveWeapon.CurrentAmmo = ActiveWeapon.MaxAmmo;
            DbContext.SaveChanges();
            AddLog($"[ПЕРЕЗАРЯДКА] {ActiveWeapon.Name} готов.");
        }

        protected void RollAndShoot()
        {
            if (ActiveWeapon == null) return;
            if (_bulletsFired > ActiveWeapon.CurrentAmmo || _bulletsFired > ActiveWeapon.ROF)
            {
                AddLog("<span style='color:orange;'>Ошибка: Не хватает патронов или превышен ROF!</span>");
                return;
            }

            int diceRoll = Random.Shared.Next(1, 101);

            try
            {
                var result = CombatService.CalculateRangedAttack(ActiveWeapon, _bulletsFired, diceRoll, _customModifier);

                // Выводим в лог требуемый DIF, чтобы было понятно, почему попал или промазал
                string logEntry = $"<span style='color:lightblue;'>[АТАКА]</span> Пуль: {_bulletsFired}. <strong>Кубик: {diceRoll}</strong> <em>(Нужно: {CalculatedFinalDIF})</em>. ";

                if (result.IsHit)
                    logEntry += $"<br/>🎯 <strong>ПОПАДАНИЕ!</strong> Зашло пуль: {result.HitsCount}. Урон: <strong>{result.TotalDamage}</strong>.";
                else
                    logEntry += "<br/>💨 <strong>ПРОМАХ!</strong>";

                AddLog(logEntry);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка: {ex.Message}");
            }
        }

        protected async Task DeleteActiveWeaponAsync()
        {
            if (ActiveWeapon == null || _arsenal.Count <= 1) return;
            string weaponName = ActiveWeapon.Name;
            DbContext.Arsenal.Remove(ActiveWeapon);
            await DbContext.SaveChangesAsync();
            _arsenal.RemoveAt(ActiveWeaponIndex);
            ActiveWeaponIndex = 0;
            AddLog($"[АРСЕНАЛ] Оружие <b>{weaponName}</b> удалено.");
        }

        protected async Task AutoSaveAsync()
        {
            DbContext.Entry(_hero).Property(c => c.BodyParts).IsModified = true;
            await DbContext.SaveChangesAsync();
        }

        protected void AddLog(string message) => _combatLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}