using System.Text;

namespace Dumper;

public class StringCreator
{      
    private int indentation; 

    private StringBuilder _sb;

    public StringCreator()
    {
        _sb = new StringBuilder();
    }

    public void Append(string input)
    {
        var tabs = new String(' ',indentation);
        _sb.Append($"{tabs}{input}");
    }
    
    public string ToString()
    {
        return _sb.ToString();
    }

    public void Indent()
    {
        indentation ++;   
    }

    public void Unindent()
    {
        indentation --;
    }

    public void WithIndent(Action action)
    {
        Indent();
        action();
        Unindent();
    }
}
