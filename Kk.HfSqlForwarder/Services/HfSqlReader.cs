using HfSqlForwarder.Models;
using System.Data.Odbc;

namespace HfSqlForwarder.Services;

/// <summary>
/// Lecteur HFSQL synchrone basé sur le pattern KKMustacheAlarmesPush.
/// Utilise une connexion singleton thread-safe.
/// </summary>
public class HfSqlReader
{
    private readonly ILogger<HfSqlReader> _logger;
    private readonly RuntimeSettingsService _runtime;

    private const string TablePrefix = "AMOY";
    private const string TableSuffixFormat = "yyyyMMdd";
    private const string RepPath = @"C:\NetGlobal\Historique\";

    public HfSqlReader(ILogger<HfSqlReader> logger, RuntimeSettingsService runtime)
    {
        _logger = logger;
        _runtime = runtime;
    }


    public Task<IReadOnlyList<RegaRecord>> ReadAsync(DateOnly jour, CancellationToken ct)
    {
        var result = ReadSync(jour);
        return Task.FromResult(result);
    }


    public IReadOnlyList<RegaRecord> ReadSync(DateOnly jour)
    {
        var suffix = jour.ToString(TableSuffixFormat);
        var tableName = $"{TablePrefix}{suffix}";

        var fic = Path.Combine(RepPath, $"{tableName}.FIC");
        var ndxLower = Path.Combine(RepPath, $"{tableName}.ndx");
        var ndxUpper = Path.Combine(RepPath, $"{tableName}.NDX");

        var ficExists = File.Exists(fic);
        var ndxExists = File.Exists(ndxLower) || File.Exists(ndxUpper);

        _logger.LogInformation("[HFSQL] Table={Table} FIC={FicExists} NDX={NdxExists}", tableName, ficExists, ndxExists);

        if (!ficExists || !ndxExists)
        {
            _logger.LogWarning("Table HFSQL incomplète: {Table}", tableName);
            return Array.Empty<RegaRecord>();
        }

        var list = new List<RegaRecord>();
        var ficPath = Path.Combine(RepPath, $"{tableName}.FIC");

        // Connexion directe HFSQL en mode free table (MODE=FREE)
        var cs = $"Driver={{HFSQL}};MODE=FREE;REP=c:\\NetGlobal\\Historique;Pooling=False;";
        _logger.LogInformation("[HFSQL] Connexion (MODE=FREE) : {Cs}", cs);

        using var conn = new OdbcConnection(cs);
        conn.Open();
        _logger.LogInformation("[HFSQL] Connexion OK");

        try
        {
            TryReadWithConnection(conn, tableName, jour, list, skipOpenFile: true);
        }
        catch (OdbcException ex)
        {
            _logger.LogError(ex, "[HFSQL] Erreur lecture table {Table}", tableName);
            throw;
        }

        return list.OrderBy(r => r.CycleNumber).ToList();
    }


    private void TryReadWithConnection(OdbcConnection conn, string tableName, DateOnly jour, List<RegaRecord> list, bool skipOpenFile = false)
    {
        _logger.LogInformation("\nConnection string: {Conn}\n", conn.ConnectionString);

        if (!skipOpenFile)
        {
            // Ouvrir explicitement le fichier (fichier non déclaré dans l'analyse)
            var openSql = $"OPEN FILE {tableName} PATH \"{RepPath}\"";
            try
            {
                _logger.LogInformation("[HFSQL] Exécution: {Sql}", openSql);
                using var openCmd = new OdbcCommand(openSql, conn) { CommandTimeout = 30 };
                openCmd.ExecuteNonQuery();
            }
            catch (OdbcException ex)
            {
                _logger.LogWarning(ex, "[HFSQL] OPEN FILE échec, tentative quand même du SELECT");
            }
        }

        var candidates = new[] { tableName, $"[{tableName}]", $"\"{tableName}\"" };
        var filtre = _runtime.Get().Filtre;
        var filters = (filtre.NumEltList is { Count: > 0 }) ? filtre.NumEltList : new List<int> { filtre.NumElt };
        OdbcException? last = null;

        foreach (var candidate in candidates)
        {
            var sql = $"SELECT * FROM {candidate}";
            _logger.LogInformation("[HFSQL] Exécution: {Sql}", sql);

            try
            {
                using var cmd = new OdbcCommand(sql, conn) { CommandTimeout = 30 };
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        continue;

                    var numElt = reader.GetInt32(0);
                    if (filters.Count > 0 && !filters.Contains(numElt))
                        continue;

                    var start = GetIntSafe(reader, 43);   // Data38
                    var end = GetIntSafe(reader, 6);      // Data1
                    var water = GetDecimalSafe(reader, 14); // Data9
                    var cycle = GetIntSafe(reader, 42);   // Data37

                    list.Add(new RegaRecord(numElt, cycle, start, end, water, jour));
                }

                last = null;
                break;
            }
            catch (OdbcException ex) when (ex.Message.Contains("inconnu", StringComparison.OrdinalIgnoreCase))
            {
                last = ex;
                _logger.LogWarning("[HFSQL] Table introuvable avec alias {Candidate}", candidate);
            }
        }

        if (last is not null)
        {
            throw last;
        }

        _logger.LogInformation("[HFSQL] {Count} lignes lues (table: {Table})", list.Count, tableName);
    }

    private static int GetIntSafe(OdbcDataReader reader, int index)
    {
        if (index >= reader.FieldCount) return 0;
        if (reader.IsDBNull(index)) return 0;
        return Convert.ToInt32(reader.GetValue(index));
    }

    private static decimal GetDecimalSafe(OdbcDataReader reader, int index)
    {
        if (index >= reader.FieldCount) return 0;
        if (reader.IsDBNull(index)) return 0;
        return Convert.ToDecimal(reader.GetValue(index));
    }
}
