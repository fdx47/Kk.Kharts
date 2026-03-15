using System.Data;
using System.Data.Odbc;

namespace HfSqlForwarder.Services;

/// <summary>
/// Singleton thread-safe pour la connexion HFSQL Historique.
/// </summary>
public sealed class HfSqlConnectionSingleton
{
    private static readonly object _lock = new();
    private static HfSqlConnectionSingleton? _instance;
    private readonly OdbcConnection _connection;

    private HfSqlConnectionSingleton()
    {
        _connection = new OdbcConnection(
            "Driver={HFSQL};" +
            @"ANA=c:\NetGlobal\NetGlobal.wdd;" +
            @"REP=c:\NetGlobal\Historique\;" +
            "Pooling=False;");
    }


    public OdbcConnection CreateFileConnection(string ficPath)
    {
        return new OdbcConnection($"Driver={{HFSQL}};FILE={ficPath};UID=;PWD=;Pooling=False;");
    }


    public static HfSqlConnectionSingleton Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    _instance ??= new HfSqlConnectionSingleton();
                }
            }
            return _instance;
        }
    }


    public OdbcConnection GetConnection()
    {
        lock (_lock)
        {
            if (_connection.State == ConnectionState.Closed ||
                _connection.State == ConnectionState.Broken)
            {
                _connection.Open();
            }
            return _connection;
        }
    }


    public void CloseConnection()
    {
        lock (_lock)
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }
    }
}
