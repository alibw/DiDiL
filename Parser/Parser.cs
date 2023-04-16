using Model;

namespace Parser;

public static class Parser
{
    private static IEnumerable<char> seperators = new[]
    {
        '\n',
        '\r',
        '\t',
        ' ',
        '.',
        '[',
        ']'
    };

    public static List<string> Split(string input)
    {
        return input.Split((char[]?)seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static List<string> SplitLine(string input, char seperator)
    {
        List<string> result = new List<string>();
        var lines = input.Split(input, seperator).ToList();
        foreach (var item in lines)
        {
            result.AddRange(Split(item));
        }

        return result;
    }

    public static Table Parse(string input)
    {
        var splitted = Split(input);
        var model = new Table();
        List<Column> columns = new List<Column>();
        var column = new Column();
        bool columnTokens = false;
        bool nameFilled = false;
        bool typeFilled = false;
        bool tableFilled = false;
        bool isForeignKey = false;
        bool isPrimaryKey = false;
        bool endOfColumns = false;
        Column? foreignKeyColumn = new Column();
        var foreignTable = new Table();

        var properties = new List<Column>();
        for (int i = 0; i < splitted.Count; i++)
        {
            if (i > 0 && splitted[i - 1] == "dbo" && !tableFilled)
            {
                model.TableName = splitted[i];
                tableFilled = true;
            }

            if ((splitted[i] == "(" || splitted[i].ToCharArray().Last() == '(') && !endOfColumns) 
            {
                columnTokens = true;
            }

            if (columnTokens)
            {
                if (splitted[i].Contains("IDENTITY"))
                {
                    column.IsIdentity = true;
                }

                if (splitted[i] == "NULL" || splitted[i] == "NULL,")
                {
                    column.Nullable = splitted[i - 1] != "NOT";
                }

                if (((splitted[i-1].ToCharArray().Last() == ',' || splitted[i-1] == "(")) && !splitted[i].Contains(')'))
                {
                    column.Name = nameFilled ? column.Name : splitted[i];
                    nameFilled = true;
                    continue;
                }

                if (nameFilled && !typeFilled)
                {
                    column.Type = splitted[i];
                    typeFilled = true;
                    continue;
                }

                if (splitted[i].ToCharArray().First() == '(' && splitted[i].ToCharArray().Last() == ')')
                {
                    column.Length = splitted[i].Replace(")","").Replace("(","");
                }
                
                if ((splitted[i].ToCharArray().Last() == ',' || splitted[i]== ")") && nameFilled && typeFilled)
                {
                    columns.Add(column);
                    nameFilled = false;
                    typeFilled = false;
                    column = new Column();
                    if (splitted[i] == ")")
                    {
                        columnTokens = false;
                        endOfColumns = true;
                    }
                }
            }

            if (splitted[i] == "FOREIGN")
            {
                isForeignKey = true;
                foreignTable = new Table();
                nameFilled = false;
            }
            
            if (splitted[i] == "PRIMARY")
            {
                isPrimaryKey = true;
            }
            if (isForeignKey)
            {
                if ((splitted[i - 1] == "dbo"))
                {
                    foreignTable.TableName = splitted[i];
                }
                
                if (splitted[i - 1] == "(" )
                {
                    if (!nameFilled)
                    {
                        foreignKeyColumn = columns.FirstOrDefault(x => x.Name == splitted[i]);
                        foreignKeyColumn!.IsForeignKey = true;
                        nameFilled = true;
                    }
                    else
                    {
                        foreignTable.Properties = new List<Column> { new Column() { IsPrimaryKey = true, Name = splitted[i] } };
                        isForeignKey = false;
                        foreignKeyColumn.ForeignKeyTable = foreignTable;
                    }
                }
            }
            if (isPrimaryKey)
            {
                if (splitted[i - 1] == "(")
                {
                    columns.FirstOrDefault(x => x.Name == splitted[i])!.IsPrimaryKey = true;
                    isPrimaryKey = false;
                }
            }
        }

        model.Properties = columns;

        return model;
    }
}