using Npgsql;
using System.Data;

namespace BlastTicket.Infra.Data;

// インターフェース定義
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionFactory(string connectionString)
    {
        // アプリケーション寿命全体で1つだけインスタンスを作るのがベストプラクティスみたい。
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = builder.Build();
    }

    public IDbConnection CreateConnection()
    {
        // ここでプールからコネクションを取り出します
        return _dataSource.OpenConnection();
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }
}