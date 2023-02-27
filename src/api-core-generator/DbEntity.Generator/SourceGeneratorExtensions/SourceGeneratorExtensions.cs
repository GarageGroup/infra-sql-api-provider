using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GGroupp.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string DefaultNamespace = "GGroupp.Infra";

    private static readonly string[] DefaultDbFieldValueTypes = new[]
    {
        "Boolean", "Byte", "DateOnly", "DateTime", "DateTimeOffset",
        "Decimal", "Double", "Single", "Guid", "Int16", "Int32", "Int64"
    };

    private static bool IsNullableType(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return false;
        }

        if (namedTypeSymbol.IsValueType)
        {
            return namedTypeSymbol.GetNullableStructType() is not null;
        }

        return namedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated;
    }

    private static ITypeSymbol? GetNullableStructType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return null;
        }

        if (namedTypeSymbol.TypeArguments.Length is 1 && namedTypeSymbol.IsSystemType("Nullable"))
        {
            return namedTypeSymbol.TypeArguments[0];
        }

        return null;
    }

    private static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item is null)
            {
                continue;
            }

            yield return item;
        }
    }

    private static string ToStringOr(this object? source, string other)
    {
        var sourceValue = source?.ToString();
        if (string.IsNullOrEmpty(sourceValue))
        {
            return other;
        }

        return sourceValue!;
    }
}