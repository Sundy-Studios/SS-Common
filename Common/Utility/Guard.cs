namespace Common.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exception.Models;

public static class Guard
{
    public static T AgainstNull<T>(T parameter, string? name = null) where T : class => parameter ?? throw new ArgumentNullException(name ?? "", $"guarded argument '{name ?? ""}' was null");

    public static void AgainstNullOrEmpty<T>(IEnumerable<T> enumerable, string? name = null)
    {
        if (enumerable == null || !enumerable.Any())
        {
            throw new BadRequestException($"expected at least one element in '{name ?? ""}' but it was null or empty");
        }
    }

    public static void AgainstNullOrEmptyWithNullOrWhitespaceValues(IEnumerable<string> strings, string? name = null)
    {
        if (strings == null || !strings.Any())
        {
            throw new BadRequestException($"expected at least one element in '{name ?? ""}' but it was null or empty");
        }

        if (strings.Any(string.IsNullOrWhiteSpace))
        {
            throw new BadRequestException($"'{name ?? ""}' contains null or whitespace values");
        }
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException($"Parameter '{paramName}' cannot be null, empty, or whitespace.");
        }
    }

    public static void AgainstOutOfRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Must be between {min} and {max}.");
        }
    }
}
