namespace LifeCrm.Core.Interfaces;

public record CsvParseError(int RowNumber, string Reason, string RawRow);

public interface ICsvService
{
    Task<byte[]> ExportAsync<T>(IEnumerable<T> rows);
    Task<(List<T> Rows, List<CsvParseError> Errors)> ImportAsync<T>(byte[] csvBytes);
}
