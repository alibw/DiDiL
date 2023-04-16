using System.Diagnostics;
using Model;
using NUnit.Framework;

namespace Test;

public class Test
{
    
public string AdvancedScript = @"
CREATE TABLE [dbo].[Session]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[MasterId] [bigint] NOT NULL,
[WeekDay] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Time] [time] NOT NULL,
[ClassNum] [nvarchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[LessonId] [bigint] NOT NULL
)
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED ([Id])
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [FK_Session_Lesson] FOREIGN KEY ([LessonId]) REFERENCES [dbo].[Lesson] ([Id])
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [FK_Session_Master] FOREIGN KEY ([MasterId]) REFERENCES [dbo].[Master] ([Id])
";

public string SimpleScript = @"CREATE TABLE [dbo].[Lesson]
(
    [Id] [bigint] NOT NULL IDENTITY(1,1),
    [Title] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
";

[Test]
    public void SplitTest()
    {
        List<string> tokens = new()
        {
            "CREATE", "TABLE", "dbo", "Lesson", "(", "Id", "bigint",
            "NOT", "NULL", "IDENTITY(1,1),", "Title", "nvarchar", "(max)", "COLLATE", "SQL_Latin1_General_CP1_CI_AS",
            "NOT", "NULL", ")"
        };
        var splitted = Parser.Parser.Split(SimpleScript);
        Assert.AreEqual(tokens, splitted);
    }
    
    [Test]
    public void GetPropertiesNamesTest()
    {
        List<Column> columns = new()
        {
            new()
            {
                Name = "Id",
                Type = "bigint",
                Nullable = false,
                IsIdentity = true
            },
            new()
            {
                Name = "Title",
                Type = "nvarchar",
                Nullable = false
            }
        };
        var parsed = Parser.Parser.Parse(SimpleScript);
        Assert.AreEqual(columns, parsed.Properties);
    }
    
    public void BCompare(string expected,string actual)
    {
        var projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        string expectedPath = $@"{projectPath}\Test\Expected\Expected.txt";
        string actualPath = $@"{projectPath}\Test\Actual\Actual.txt";
        File.WriteAllText(expectedPath,expected);
        File.WriteAllText(actualPath,actual);
        Process.Start("cmd.exe", $"/C start bcompare.exe {expectedPath} {actualPath}");
    }

    [Test]
    public void CycleTest()
    {
        var parsedTable = Parser.Parser.Parse(AdvancedScript);
        var columns = new List<Column>();
        foreach (var item in parsedTable.Properties)
        {
            var column = new Column()
            {
                Name = item.Name,
                Type = item.Type,
                Length = item.Length,
                IsIdentity = item.IsIdentity,
                IsForeignKey = item.IsForeignKey,
                Nullable = item.Nullable,
                IsPrimaryKey = item.IsPrimaryKey,
                ForeignKeyTable = item.ForeignKeyTable == null
                    ? null
                    : new Table()
                    {
                        TableName = item.ForeignKeyTable.TableName,
                        Properties = new List<Column>()
                        {
                            new Column()
                            {
                                IsPrimaryKey = true,
                                Name = item.ForeignKeyTable.Properties.FirstOrDefault()!.Name
                            }
                        }
                    }
            };
            columns.Add(column);
        }
        var convertedTable = new Table()
        {
            TableName = parsedTable.TableName,
            DbName = parsedTable.DbName,
            Properties = columns
        };
        Dumper.Dumper dm = new Dumper.Dumper(convertedTable);
        BCompare(dm.Dump(),AdvancedScript);
    }
}