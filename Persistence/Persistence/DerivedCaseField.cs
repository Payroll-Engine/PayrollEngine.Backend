﻿using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

// <summary>Derived case field</summary>
public sealed class DerivedCaseField : ChildCaseField
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}