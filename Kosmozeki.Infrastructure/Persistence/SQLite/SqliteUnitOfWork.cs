using Kosmozeki.Domain.Shared;
using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.Sqlite;

public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly SqliteConnection _connection;
    private SqliteTransaction? _transaction;

    public SqliteUnitOfWork(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        if (_transaction is not null)
            return;

        _transaction = await _connection.BeginTransactionAsync(ct);
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public SqliteTransaction? CurrentTransaction => _transaction;
}