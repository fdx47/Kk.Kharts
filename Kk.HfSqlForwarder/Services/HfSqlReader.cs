using System.Data.Odbc;
using HfSqlForwarder.Models;
using HfSqlForwarder.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HfSqlForwarder.Services;

public class HfSqlReader
{
    private readonly ILogger<HfSqlReader> _logger;
    private readonly IOptionsMonitor<ForwarderOptions> _options;

    public HfSqlReader(ILogger<HfSqlReader> logger, IOptionsMonitor<ForwarderOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<IReadOnlyList<RegaRecord>> ReadAsync(DateOnly jour, CancellationToken ct)
    {
        var opts = _options.CurrentValue.HfSql;
        var suffix = jour.ToString(opts.TableSuffixFormat);
        var physicalTable = $"{opts.TablePrefix}{suffix}";
        var tableName = physicalTable;

        var cs = $"Driver={{{opts.Driver}}};ANA={opts.AnaPath};REP={opts.RepPath};";
        using var conn = new OdbcConnection(cs);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        var sql = $"SELECT Data38, Data1, Data9, Data37, NumElt FROM {tableName}";
        using var cmd = new OdbcCommand(sql, conn)
        {
            CommandTimeout = opts.CommandTimeoutSeconds
        };

        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        var list = new List<RegaRecord>();
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            if (reader.IsDBNull(4)) continue;
            var numElt = reader.GetInt32(4);
            if (numElt != _options.CurrentValue.Filtre.NumElt) continue;

            var start = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            var end = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            var water = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
            var cycle = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

            list.Add(new RegaRecord(numElt, cycle, start, end, water, jour));
        }

        _logger.LogInformation("[HFSQL] {Count} lignes lues dans {Table} (physique: {Physical})", list.Count, tableName, physicalTable);
        return list.OrderBy(r => r.CycleNumber).ToList();
    }
}
