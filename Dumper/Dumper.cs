using System.Text;
using Model;

namespace Dumper;

public class Dumper
{
    public Table table;

    private StringCreator sc;

    public Dumper(Table _table)
    {
        table = _table;
    }

    public string Dump()
    {
        sc = new StringCreator();
        sc.Append($@"CREATE TABLE [dbo].[{table.TableName}]{Environment.NewLine}({Environment.NewLine}");
        sc.WithIndent(() => GetColumns());
        sc.Append($@"{Environment.NewLine}){Environment.NewLine}{GetPrimaryKey()}{Environment.NewLine}{GetForeignKeys()}");
        return sc.ToString();
    }

    public string GetPrimaryKey()
    {
        var primaryKey = table.Properties.FirstOrDefault(x => x.IsPrimaryKey);
        var primaryKeyLine =
            $@"ALTER TABLE [dbo].[{table.TableName}] ADD CONSTRAINT [PK_{table.TableName}] PRIMARY KEY CLUSTERED ([{primaryKey.Name}])";
        return primaryKeyLine;
    }

    public string GetColumns()
    {
        foreach (var column in table.Properties)
        {
            sc.Append(
                $@"[{column.Name}] [{column.Type}] {(string.IsNullOrEmpty(column.Length) ? "" : $"({column.Length})")} {(column.Nullable ? "NULL" : "NOT NULL")} {(column.IsIdentity ? "IDENTITY(1, 1)" : "")}{(column == table.Properties.Last() ? "" : $",{Environment.NewLine}")}");
        }

        return sc.ToString();
    }

    public string GetForeignKeys()
    {
        StringCreator sb = new StringCreator();
        foreach (var column in table.Properties.Where(x => x.IsForeignKey))
        {
            sb.Append(
                $@"ALTER TABLE [dbo].[{table.TableName}] ADD CONSTRAINT [FK_{table.TableName}_{column.ForeignKeyTable!.TableName}] FOREIGN KEY ([{column.Name}]) REFERENCES [dbo].[{column.ForeignKeyTable!.TableName}] ([{column.ForeignKeyTable.Properties.FirstOrDefault(x=>x.IsPrimaryKey)!.Name}]){Environment.NewLine}");
        }

        return sb.ToString();
    }
}