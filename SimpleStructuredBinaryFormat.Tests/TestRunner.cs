using System.Reflection;
using System.Runtime.ExceptionServices;

namespace SimpleStructuredBinaryFormat.Tests;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class FactAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class TheoryAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class InlineDataAttribute(params object?[] data) : Attribute
{
    public object?[] Data { get; } = data;
}

internal static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual))
            throw new Exception($"Assert.Equal failed.\n  Expected: {Format(expected)}\n  Actual:   {Format(actual)}");
    }

    public static void Equal(byte[] expected, byte[] actual)
    {
        if (!expected.SequenceEqual(actual))
            throw new Exception($"Assert.Equal (byte[]) failed.\n  Expected: [{string.Join(", ", expected)}]\n  Actual:   [{string.Join(", ", actual)}]");
    }

    public static void True(bool condition, string? message = null)
    {
        if (!condition)
            throw new Exception($"Assert.True failed.{(message is null ? "" : " " + message)}");
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
            throw new Exception($"Assert.False failed.{(message is null ? "" : " " + message)}");
    }

    public static void Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
            throw new Exception($"Assert.Throws<{typeof(TException).Name}> failed: no exception was thrown.");
        }
        catch (TException) { /* expected */ }
    }

    private static string Format(object? value) => value is null ? "<null>" : value.ToString() ?? "<null>";
}

internal static class TestRunner
{
    public static int Run(Assembly assembly)
    {
        var testClasses = assembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<FactAttribute>() != null
                                                || m.GetCustomAttribute<TheoryAttribute>() != null))
            .OrderBy(t => t.Name)
            .ToList();

        var total = 0;
        var passed = 0;
        var failed = 0;

        foreach (var type in testClasses)
        {
            Console.WriteLine($"\n  {type.Name}");

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                       .OrderBy(m => m.Name))
            {
                // Fact
                if (method.GetCustomAttribute<FactAttribute>() is not null)
                {
                    RunCase(type, method, null, ref total, ref passed, ref failed);
                }

                // Theory + InlineData
                var inlineDataAttrs = method.GetCustomAttributes<InlineDataAttribute>().ToList();
                if (inlineDataAttrs.Count > 0)
                {
                    foreach (var attr in inlineDataAttrs)
                        RunCase(type, method, attr.Data, ref total, ref passed, ref failed);
                }
            }
        }

        Console.WriteLine($"\n{'─', -60}");
        Console.WriteLine($"  Results: {passed}/{total} passed, {failed} failed");

        return failed > 0 ? 1 : 0;
    }

    private static void RunCase(
        Type type, MethodInfo method, object?[]? args,
        ref int total, ref int passed, ref int failed)
    {
        total++;
        var label = args is null
            ? method.Name
            : $"{method.Name}({string.Join(", ", args.Select(a => a is null ? "null" : a.ToString()))})";

        try
        {
            var instance = Activator.CreateInstance(type)!;
            method.Invoke(instance, args);
            Console.WriteLine($"    ✓ {label}");
            passed++;
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            Console.WriteLine($"    ✗ {label}");
            Console.WriteLine($"        {tie.InnerException.Message}");
            failed++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    ✗ {label}");
            Console.WriteLine($"        {ex.Message}");
            failed++;
        }
    }
}
