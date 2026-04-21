using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Services;

public class CsvService : ICsvService
{
    public async Task<byte[]> ExportAsync<T>(IEnumerable<T> rows)
    {
        using var ms  = new MemoryStream();
        using var sw  = new StreamWriter(ms);
        using var csv = new CsvWriter(sw, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.WriteRecords(rows);
        await sw.FlushAsync();
        return ms.ToArray();
    }

    public async Task<(List<T> Rows, List<CsvParseError> Errors)> ImportAsync<T>(byte[] csvBytes)
    {
        using var ms  = new MemoryStream(csvBytes);
        using var sr  = new StreamReader(ms);
        using var csv = new CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var rows   = new List<T>();
        var errors = new List<CsvParseError>();
        var row    = 0;

        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            row++;
            try { rows.Add(record); }
            catch (Exception ex) { errors.Add(new CsvParseError(row, ex.Message, csv.Context.Parser.RawRecord)); }
        }
        return (rows, errors);
    }
}
