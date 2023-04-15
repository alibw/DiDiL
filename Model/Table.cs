namespace Model;

public class Table
{
    public string DbName { get; set; }
    
    public string TableName { get; set; }

    public List<Column> Properties { get; set; }
}

