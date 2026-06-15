using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kosmozeki.Domain.Weapon;

public class Weapon
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

    public Weapon() { }

    public Weapon(string name, int baseDMG, int edmg, int maxAmmo, int rof, int baseDIF)
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