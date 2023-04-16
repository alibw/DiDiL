namespace Model;

public class Column : IEquatable<Column>
{
    public string Name { get; set; }
    public string Type { get; set; }
    
    public string Length { get; set; }
    public bool Nullable { get; set; }

    public bool IsIdentity { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsForeignKey { get; set; }

    public Table? ForeignKeyTable { get; set; }


    public bool Equals(Column? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Type == other.Type && Nullable == other.Nullable && IsIdentity == other.IsIdentity && IsPrimaryKey == other.IsPrimaryKey && IsForeignKey == other.IsForeignKey && Equals(ForeignKeyTable, other.ForeignKeyTable);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Column)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type, Nullable, IsIdentity, IsPrimaryKey, IsForeignKey, ForeignKeyTable);
    }
}