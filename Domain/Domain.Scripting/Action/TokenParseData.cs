using System;

namespace PayrollEngine.Domain.Scripting.Action;

internal class TokenParseData
{
    internal TokenParseData(int endIndex, string text = null,
        string property = null, string parameters = null,
        string postCode = null, int parameterIndex = 0)
    {
        EndIndex = endIndex;
        Text = text;
        Property = property;
        Parameters = parameters;
        ParameterIndex = parameterIndex;
        PostCode = postCode;
    }

    internal TokenParseData(TokenParseData source)
    {
        EndIndex = source.EndIndex;
        Text = source.Text;
        Property = source.Property;
        Parameters = source.Parameters;
        ParameterIndex = source.ParameterIndex;
        PostCode = source.PostCode;
    }

    internal int EndIndex { get; }
    internal string Text { get; }
    internal string Parameters { get; }
    internal int ParameterIndex { get; }
    private string Property { get; }
    internal string PostCode { get; }

    internal T GetProperty<T>(T defaultValue = default) where T : struct, IConvertible
    {
        if (string.IsNullOrWhiteSpace(Property) || !Enum.TryParse<T>(Property, out var result))
        {
            return defaultValue;
        }
        return result;
    }
}