using System.Text;

namespace HySite.Application.Extensions;

public static class StringExtensions
{
    public static string ConvertToString(this byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            builder.AppendFormat("{0:x2}", b);

        return builder.ToString();
    }
}