// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Platform.Extensions.Messages;

/// <summary>
/// Represents the type of code coverage being measured.
/// </summary>
public enum CoverageType
{
    /// <summary>
    /// Line coverage.
    /// </summary>
    Line,

    /// <summary>
    /// Branch coverage.
    /// </summary>
    Branch,

    /// <summary>
    /// Method coverage.
    /// </summary>
    Method,
}

/// <summary>
/// Represents a test coverage data message for a specific module.
/// </summary>
public sealed class TestCoverageMessage : PropertyBagData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCoverageMessage"/> class.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="value">The coverage value (percentage).</param>
    /// <param name="coverageType">The type of coverage measurement.</param>
    public TestCoverageMessage(string moduleName, double value, CoverageType coverageType)
        : base("Test coverage", "Reports code coverage data for a module.")
    {
        ModuleName = moduleName;
        Value = value;
        CoverageType = coverageType;
    }

    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Gets the coverage value (percentage).
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the type of coverage measurement.
    /// </summary>
    public CoverageType CoverageType { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder("TestCoverageMessage { DisplayName = ")
            .Append(DisplayName)
            .Append(", Description = ")
            .Append(Description)
            .Append(", ModuleName = ")
            .Append(ModuleName)
            .Append(", Value = ")
            .Append(Value.ToString("F1", CultureInfo.InvariantCulture))
            .Append(", CoverageType = ")
            .Append(CoverageType)
            .Append(", Properties = [");

        bool hasAnyProperty = false;
        foreach (IProperty property in Properties)
        {
            if (!hasAnyProperty)
            {
                hasAnyProperty = true;
            }
            else
            {
                builder.Append(',');
            }

            builder.Append(' ').Append(property);
        }

        if (hasAnyProperty)
        {
            builder.Append(' ');
        }

        builder.Append("] }");

        return builder.ToString();
    }
}

/// <summary>
/// Represents the statistical method used for the threshold comparison.
/// </summary>
public enum CoverageThresholdStatistic
{
    /// <summary>
    /// Minimum coverage across all modules.
    /// </summary>
    Minimum,

    /// <summary>
    /// Total (aggregate) coverage.
    /// </summary>
    Total,

    /// <summary>
    /// Average coverage across all modules.
    /// </summary>
    Average,
}

/// <summary>
/// Represents the result status of a coverage threshold check.
/// </summary>
public enum CoverageThresholdStatus
{
    /// <summary>
    /// The threshold was met.
    /// </summary>
    Passed,

    /// <summary>
    /// The threshold was not met.
    /// </summary>
    Failed,
}

/// <summary>
/// Represents a test coverage threshold evaluation result.
/// </summary>
public sealed class TestCoverageThresholdMessage : PropertyBagData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCoverageThresholdMessage"/> class.
    /// </summary>
    /// <param name="value">The actual coverage value.</param>
    /// <param name="threshold">The required threshold value.</param>
    /// <param name="coverageType">The type of coverage measurement.</param>
    /// <param name="status">The pass/fail status of the threshold check.</param>
    /// <param name="statistic">The statistical method used for comparison.</param>
    public TestCoverageThresholdMessage(double value, double threshold, CoverageType coverageType, CoverageThresholdStatus status, CoverageThresholdStatistic statistic)
        : base("Test coverage threshold", "Reports the result of a coverage threshold evaluation.")
    {
        Value = value;
        Threshold = threshold;
        CoverageType = coverageType;
        Status = status;
        Statistic = statistic;
    }

    /// <summary>
    /// Gets the actual coverage value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the required threshold value.
    /// </summary>
    public double Threshold { get; }

    /// <summary>
    /// Gets the type of coverage measurement.
    /// </summary>
    public CoverageType CoverageType { get; }

    /// <summary>
    /// Gets the pass/fail status of the threshold check.
    /// </summary>
    public CoverageThresholdStatus Status { get; }

    /// <summary>
    /// Gets the statistical method used for comparison.
    /// </summary>
    public CoverageThresholdStatistic Statistic { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder("TestCoverageThresholdMessage { DisplayName = ")
            .Append(DisplayName)
            .Append(", Description = ")
            .Append(Description)
            .Append(", Value = ")
            .Append(Value.ToString("F1", CultureInfo.InvariantCulture))
            .Append(", Threshold = ")
            .Append(Threshold.ToString("F1", CultureInfo.InvariantCulture))
            .Append(", CoverageType = ")
            .Append(CoverageType)
            .Append(", Status = ")
            .Append(Status)
            .Append(", Statistic = ")
            .Append(Statistic)
            .Append(", Properties = [");

        bool hasAnyProperty = false;
        foreach (IProperty property in Properties)
        {
            if (!hasAnyProperty)
            {
                hasAnyProperty = true;
            }
            else
            {
                builder.Append(',');
            }

            builder.Append(' ').Append(property);
        }

        if (hasAnyProperty)
        {
            builder.Append(' ');
        }

        builder.Append("] }");

        return builder.ToString();
    }
}
