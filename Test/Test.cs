using System.Diagnostics;
using Model;
using NUnit.Framework;

namespace Test;

public class Test
{
    public static string script = @"
CREATE TABLE [dbo].[Session]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[MasterId] [bigint] NOT NULL,
[WeekDay] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Time] [time] NOT NULL,
[ClassNum] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[LessonId] [bigint] NOT NULL
)
GO
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED ([Id])
GO
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [FK_Session_Lesson] FOREIGN KEY ([LessonId]) REFERENCES [dbo].[Lesson] ([Id])
GO
ALTER TABLE [dbo].[Session] ADD CONSTRAINT [FK_Session_Master] FOREIGN KEY ([MasterId]) REFERENCES [dbo].[Master] ([Id])
GO
";

    Table table = new Table()
    {
        DbName = "College",
        TableName = "Lesson",
        Properties = new List<Column>()
        {
            new Column
            {
                Name = "Id",
                Type = "bigint",
                IsIdentity = true,
                IsPrimaryKey = true
            },
            new Column
            {
                Name = "Title",
                Type = "nvarchar"
            },
            new Column
            {
                Name = "UnitNum",
                Type = "bigint"
            },
            new Column
            {
                Name = "MasterId",
                Type = "bigint",
                IsForeignKey = true,
                ForeignKeyTable = new Table()
                {
                    DbName = "College",
                    TableName = "Master",
                    Properties = new List<Column>()
                    {
                        new Column
                        {
                            Name = "Id",
                            Type = "bigint",
                            IsIdentity = true,
                            IsPrimaryKey = true
                        }
                    }
                }
            }
        }
    };


    string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

    [Test]
    public void SplitTest()
    {
        List<string> tokens = new()
        {
            "CREATE", "TABLE", "dbo", "Lesson", "(", "Id", "bigint",
            "NOT", "NULL,", "IDENTITY(1,1)", "Title", "nvarchar", "(max)", "COLLATE", "SQL_Latin1_General_CP1_CI_AS",
            "NOT", "NULL,", "UnitNum", "bigint", "NOT", "NULL", ")"
        };
        var splitted = Parser.Parser.Split(script);
        Assert.AreEqual(tokens, splitted);
    }

    [Test]
    public void GetDbNameTest()
    {
        Assert.AreEqual("College", Parser.Parser.Parse(script).DbName);
    }

    [Test]
    public void GetTableNameTest()
    {
        Assert.AreEqual("Lesson", Parser.Parser.Parse(script).TableName);
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
                Nullable = false
            },
            new()
            {
                Name = "Title",
                Type = "nvarchar",
                Nullable = false
            },
            new()
            {
                Name = "UnitNum",
                Type = "bigint",
                Nullable = false
            }
        };
        var parsed = Parser.Parser.Parse(script);
        Assert.AreEqual(columns, parsed.Properties);
    }

    [Test]
    public void Compare()
    {
        string expectedPath = $@"{projectPath}\DDlDumper\Expected\Expected.txt";
        string actualPath = $@"{projectPath}\DDlDumper\Actual\Actual.txt";
        var expected = new MemoryStream(File.ReadAllBytes(expectedPath));
        var actual = new MemoryStream(File.ReadAllBytes(actualPath));
        Process.Start("cmd.exe", $"/C start bcompare.exe {expectedPath} {actualPath}");
        FileAssert.AreEqual(expected, actual);
    }

    [Test]
    public void CycleTest()
    {
        var parsedTable = Parser.Parser.Parse(script);

        var columns = new List<Column>();
        foreach (var item in parsedTable.Properties)
        {
            var column = new Column()
            {
                Name = item.Name,
                Type = item.Type,
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
        string actualPath = $@"{projectPath}\DDlDumper\Actual\Actual.txt";
        File.WriteAllText(actualPath, dm.Dump());
    }
}