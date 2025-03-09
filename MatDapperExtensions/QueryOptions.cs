using MatSqlFilter;
using ResultPattern;
using System.Collections.Generic;

namespace MatDapperExtensions;

public sealed class QueryOptions
{
    public Filter Filter { get; set; }
    public List<string> Columns { get; set; }
    public Pager Pager { get; set; }
}



