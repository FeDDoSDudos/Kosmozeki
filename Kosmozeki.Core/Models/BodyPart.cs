using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace Kosmozeki.Core.Models
{
    public enum BodyPartType { Head = 0, Torso = 1, LeftArm = 2, RightArm = 3, LeftLeg = 4, RightLeg = 5 }

    public class BodyPart
    {
        public BodyPartType Type { get; init; }
        public string Name { get; init; }
        public int MaxHp { get; init; }

        // Сделали setter публичным для ручного редактирования
        public int CurrentHp { get; set; }
        public int Armor { get; set; }

        public bool IsDestroyed => CurrentHp <= 0;

        public BodyPart() { } // Пустой конструктор для сериализатора

        public BodyPart(BodyPartType type, string name, int maxHp, int armor = 0)
        {
            Type = type;
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Armor = armor;
        }

        public int TakeDamage(int damage, bool applyArmor = true)
        {
            int effectiveDamage = applyArmor ? Math.Max(0, damage - Armor) : damage;
            if (Type == BodyPartType.Head) effectiveDamage *= 2;
            CurrentHp = Math.Max(0, CurrentHp - effectiveDamage);
            return effectiveDamage;
        }
    }

    public class Character
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public Dictionary<BodyPartType, BodyPart> BodyParts { get; set; }

        public Character() { } // Для EF Core

        public Character(string name)
        {
            Name = name;
            BodyParts = new Dictionary<BodyPartType, BodyPart>
            {
                { BodyPartType.Head, new BodyPart(BodyPartType.Head, "Голова", 100) },
                { BodyPartType.Torso, new BodyPart(BodyPartType.Torso, "Тело", 150) },
                { BodyPartType.LeftArm, new BodyPart(BodyPartType.LeftArm, "Левая рука", 100) },
                { BodyPartType.RightArm, new BodyPart(BodyPartType.RightArm, "Правая рука", 100) },
                { BodyPartType.LeftLeg, new BodyPart(BodyPartType.LeftLeg, "Левая нога", 100) },
                { BodyPartType.RightLeg, new BodyPart(BodyPartType.RightLeg, "Правая нога", 100) }
            };
        }
    }

    public class RangedWeapon
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int BaseDMG { get; set; }
        public int EDMG { get; set; }
        public int MaxAmmo { get; set; }
        public int CurrentAmmo { get; set; }
        public int ROF { get; set; }
        public int BaseDIF { get; set; }

        public RangedWeapon() { } // Для EF Core

        public RangedWeapon(string name, int baseDMG, int edmg, int maxAmmo, int rof, int baseDIF)
        {
            Name = name;
            BaseDMG = baseDMG;
            EDMG = edmg;
            MaxAmmo = maxAmmo;
            CurrentAmmo = maxAmmo;
            ROF = rof;
            BaseDIF = baseDIF;
        }
    }
}