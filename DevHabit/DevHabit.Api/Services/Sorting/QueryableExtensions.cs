using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Services.Sorting;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query,
                                             string? sort,
                                             SortMapping[] mappings,
                                             string defaultOrderBy = "Id")
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(defaultOrderBy);
        }

        string[] sortFields = sort.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        var orderByParts = new List<string>();
        foreach (string field in sortFields)
        {
            (string sortField, bool isDecending) = ParseSortField(field);

            SortMapping mapping = mappings.First(m =>
                m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));

            string direction = (isDecending, mapping.Reverse) switch
            {
                (false, false) => "ASC",
                (false, true) => "DESC",
                (true, false) => "DESC",
                (true, true) => "ASC"
            };

            orderByParts.Add($"{mapping.PropertyName} {direction}");
        }

        string orderBy = string.Join(", ", orderByParts);

        return query.OrderBy(orderBy);
    }

    private static (string SortField, bool IsDecending) ParseSortField(string field)
    {
        string[] parts = field.Split(' ');
        string sortField = parts[0];
        bool isDecending = parts.Length > 1 &&
                           parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortField, isDecending);
    }
}
