using System;
using System.Collections.Generic;

//Add this in the omnicat utils later
public static class IEnumerableExtensions
{
    public static string DebugFormat<T>(this IEnumerable<T> collection)
    {
        return "[" + string.Join(",", collection) + "]";
    }
}

public static class TypeExtensions
{
    public static string ToSpacedString(this Type type)
    {
        string typeName = type.Name;

        if (string.IsNullOrWhiteSpace(typeName))
            return typeName;

        var newText = new System.Text.StringBuilder(typeName.Length * 2);
        newText.Append(typeName[0]);

        for (int i = 1; i < typeName.Length; i++)
        {
            if (char.IsUpper(typeName[i]) && !char.IsWhiteSpace(typeName[i - 1]))
            {
                newText.Append(' ');
            }
            newText.Append(typeName[i]);
        }

        return newText.ToString();
    }
}
