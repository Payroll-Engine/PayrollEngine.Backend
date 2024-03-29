﻿using System;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Payroll script compile exception
/// </summary>
public abstract class PayrollScriptException : PayrollException
{
    /// <summary>Initializes a new instance of the <see cref="T:PayrollEngine.CSharpException"></see> class.</summary>
    protected PayrollScriptException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:PayrollEngine.CSharpException"></see> class with a specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    protected PayrollScriptException(string message) :
        base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:PayrollEngine.CSharpException"></see> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    protected PayrollScriptException(string message, Exception innerException) :
        base(message, innerException)
    {
    }
}