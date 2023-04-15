namespace Model;

public class Column
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Nullable { get; set; }

    public bool IsIdentity { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsForeignKey { get; set; }
    
    public Table? ForeignKeyTable { get; set; }
}