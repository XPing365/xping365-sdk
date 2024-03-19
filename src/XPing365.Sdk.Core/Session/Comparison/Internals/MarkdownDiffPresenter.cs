﻿using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Session.Comparison.Internals;

/// <summary>
/// Formats the differences between two TestSession instances into a readable format.
/// </summary>
internal class MarkdownDiffPresenter : IDiffPresenter
{
    /// <summary>
    /// Gets or sets a value indicating whether the title should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the title is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeTitle { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the overview should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the overview is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeOverview { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the summary should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the summary is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a detailed comparison should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the detailed comparison is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDetailedComparison { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether detailed start date information should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the detailed start date information is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDetailedStartDate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether detailed failures information should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the detailed failures information is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDetailedFailures { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether detailed execution duration information should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the detailed execution duration information is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDetailedExecutionDuration { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a detailed step-by-step analysis should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the detailed step-by-step analysis is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeDetailedStepByStepAnalysis { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the conclusion should be included in the output.
    /// </summary>
    /// <value>
    /// <c>true</c> if the conclusion is to be included; otherwise, <c>false</c>.
    /// </value>
    public bool IncludeConclusion { get; set; } = true;

    /// <summary>
    /// Formats the given DiffResult into a human-readable string.
    /// </summary>
    /// <param name="diffResult">The DiffResult to format.</param>
    /// <returns>A string representing the formatted differences.</returns>
    public string FormatDiff(DiffResult diffResult)
    {
        ArgumentNullException.ThrowIfNull(diffResult, nameof(diffResult));

        return MarkdownDiffPresenterBuilder.Build(this, diffResult);
    }
}
