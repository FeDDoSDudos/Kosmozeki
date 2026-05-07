using System.Reflection;

namespace Kosmozeki.Core.Services
{
    using Models;

    public record AttackResult(bool IsHit, int HitsCount, int TotalDamage, int LocationCode, string Message);

    public class CombatEngineService
    {
        /// <summary>
        /// Рассчитывает результат стрельбы из дальнобойного оружия.
        /// </summary>
        /// <param name="diceRoll">Значение на d100 (1-100)</param>
        /// <param name="aimPenalty">Штраф за прицеливание (например, 15 для конечности, 20 для головы). Увеличивает DIF.</param>
        public AttackResult CalculateRangedAttack(RangedWeapon weapon, int bulletsFired, int diceRoll, int modifier = 0)
        {
            if (bulletsFired > weapon.CurrentAmmo || bulletsFired > weapon.ROF)
                throw new ArgumentException("Невозможно выстрелить больше патронов, чем есть в магазине или позволяет ROF.");

            // Списываем патроны
            weapon.CurrentAmmo -= bulletsFired;

            // Итоговая сложность: База - выпущенные пули + модификаторы (штраф > 0, бонус < 0)
            int currentDif = weapon.BaseDIF - bulletsFired + modifier;

            if (diceRoll < currentDif)
            {
                return new AttackResult(false, 0, 0, 0, "Промах!");
            }

            // Расчет попаданий: 1 базовое (за бросок >= DIF) + 1 за каждые полные 10 единиц разницы
            int hits = 1 + ((diceRoll - currentDif) / 10);

            // Количество попаданий не может превысить количество выпущенных пуль
            hits = Math.Min(hits, bulletsFired);

            // Локация
            int locationCode = diceRoll % 10;

            // Урон с учетом стихийного
            int totalDamage = hits * (weapon.BaseDMG + weapon.EDMG);

            return new AttackResult(
                IsHit: true,
                HitsCount: hits,
                TotalDamage: totalDamage,
                LocationCode: locationCode,
                Message: "Успех"
            );
        }

        /// <summary>
        /// Маппинг последней цифры кубика (0-9) на конкретные части тела.
        /// (Мастер может настроить эту таблицу под свою ширму).
        /// </summary>
        public BodyPartType ResolveLocationCode(int code)
        {
            return code switch
            {
                0 => BodyPartType.Head,
                1 => BodyPartType.RightArm,
                2 => BodyPartType.LeftArm,
                3 => BodyPartType.RightLeg,
                4 => BodyPartType.LeftLeg,
                _ => BodyPartType.Torso
            };
        }
    }
}