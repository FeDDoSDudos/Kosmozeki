using Kosmozeki.Domain.Weapon;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using WeaponDomain = Kosmozeki.Domain.Weapon.Weapon;

namespace Kosmozeki.Infrastructure.Persistence.SQLite.Weapon;

public sealed class SqliteWeaponRepository : IWeaponRepository
{
    private readonly SqliteConnection _connection;

    public SqliteWeaponRepository(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<WeaponDomain>> GetAllAsync(CancellationToken ct = default)
    {
        var result = new List<WeaponDomain>();

        var cmd = _connection.CreateCommand();
        cmd.CommandText = """
        SELECT Id, Name, BaseDMG, EDMG, MaxAmmo, CurrentAmmo, ROF, BaseDIF
        FROM Weapons
        ORDER BY Id;
        """;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new WeaponDomain
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                BaseDMG = reader.GetInt32(2),
                EDMG = reader.GetInt32(3),
                MaxAmmo = reader.GetInt32(4),
                CurrentAmmo = reader.GetInt32(5),
                ROF = reader.GetInt32(6),
                BaseDIF = reader.GetInt32(7)
            });
        }

        return result;
    }

    public async Task<WeaponDomain> UpsertAsync(WeaponDomain weapon, CancellationToken ct = default)
    {
        if (weapon.Id == 0)
        {
            var insert = _connection.CreateCommand();
            insert.CommandText = """
            INSERT INTO Weapons (Name, BaseDMG, EDMG, MaxAmmo, CurrentAmmo, ROF, BaseDIF)
            VALUES ($name, $baseDmg, $eDmg, $maxAmmo, $currentAmmo, $rof, $baseDif);
            SELECT last_insert_rowid();
            """;

            insert.Parameters.AddWithValue("$name", weapon.Name);
            insert.Parameters.AddWithValue("$baseDmg", weapon.BaseDMG);
            insert.Parameters.AddWithValue("$eDmg", weapon.EDMG);
            insert.Parameters.AddWithValue("$maxAmmo", weapon.MaxAmmo);
            insert.Parameters.AddWithValue("$currentAmmo", weapon.CurrentAmmo);
            insert.Parameters.AddWithValue("$rof", weapon.ROF);
            insert.Parameters.AddWithValue("$baseDif", weapon.BaseDIF);

            var id = (long)(await insert.ExecuteScalarAsync(ct) ?? 0L);
            weapon.Id = (int)id;
            return weapon;
        }

        var update = _connection.CreateCommand();
        update.CommandText = """
        UPDATE Weapons
        SET Name = $name,
            BaseDMG = $baseDmg,
            EDMG = $eDmg,
            MaxAmmo = $maxAmmo,
            CurrentAmmo = $currentAmmo,
            ROF = $rof,
            BaseDIF = $baseDif
        WHERE Id = $id;
        """;

        update.Parameters.AddWithValue("$id", weapon.Id);
        update.Parameters.AddWithValue("$name", weapon.Name);
        update.Parameters.AddWithValue("$baseDmg", weapon.BaseDMG);
        update.Parameters.AddWithValue("$eDmg", weapon.EDMG);
        update.Parameters.AddWithValue("$maxAmmo", weapon.MaxAmmo);
        update.Parameters.AddWithValue("$currentAmmo", weapon.CurrentAmmo);
        update.Parameters.AddWithValue("$rof", weapon.ROF);
        update.Parameters.AddWithValue("$baseDif", weapon.BaseDIF);

        await update.ExecuteNonQueryAsync(ct);
        return weapon;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Weapons WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
