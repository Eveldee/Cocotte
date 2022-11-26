using System.Numerics;
using System.Text;

namespace Cocotte.Utils;

public static class NumberUtils
{
    public static string FormatSpaced<T>(this INumber<T> number) where T : INumber<T>?
    {
        var stringNumber = number.ToString(null, null);

        Span<char> result = stackalloc char[stringNumber.Length + stringNumber.Length / 3];
        int resultOffset = 0;

        for (int i = 0; i < stringNumber.Length; i++)
        {
            // Add a space
            if (i > 2 && (i + 1) % 3 == 1)
            {
                result[resultOffset] = ' ';
                resultOffset++;
            }

            result[resultOffset] = stringNumber[^(i + 1)];
            resultOffset++;
        }

        var realResult = result[..resultOffset];
        Span<char> reversed = stackalloc char[resultOffset];

        for (int i = 0; i < reversed.Length; i++)
        {
            reversed[i] = realResult[^(i + 1)];
        }

        return reversed.ToString();
    }
}