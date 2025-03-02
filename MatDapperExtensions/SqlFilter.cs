using ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatDapperExtensions;

public static class SqlFilter
{
    public static (bool, string) Generate(this List<Filter> filters)
    {
        if (filters is null)
            return (true, string.Empty);

        var types = _filterConditions.Keys.ToHashSet();

        if (filters.Any(f => !types.Contains(f.Type)))
            return (false, "Invalid operator");

        var conditions = new HashSet<string>();

        foreach (var filter in filters)
            conditions.Add(_filterConditions[filter.Type](filter));

        return (true, string.Join(" AND ", conditions));
    }

    private static readonly Dictionary<string, Func<Filter, string>> _filterConditions = new()
    {
        { nameof(FilterType.Equals), f => $"{f.Column} = '{f.Value}'" },
        { nameof(FilterType.Contains), f => $"{f.Column} LIKE '%{f.Value}%'" },
        { nameof(FilterType.GreaterThan), f => $"{f.Column} > '{f.Value}'" },
        { nameof(FilterType.LessThan), f => $"{f.Column} < '{f.Value}'" }
    };
}

public sealed class QueryOptions
{
    public List<Filter> Filters { get; set; }
    public List<string> Columns { get; set; }
    public Pager Pager { get; set; }
}
public enum FilterType
{
    Equals,
    Contains,
    GreaterThan,
    LessThan
}

public record Filter(string Column, string Value, string Type);
