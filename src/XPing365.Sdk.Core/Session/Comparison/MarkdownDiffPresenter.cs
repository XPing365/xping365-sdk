using System.Globalization;
using System.Text;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Session.Comparison;

/// <summary>
/// Formats the differences between two TestSession instances into a readable format.
/// </summary>
internal class MarkdownDiffPresenter : IDiffPresenter
{
    private const string Title =
        "# Test Session Comparison Report";
    private const string Overview =
        "## Overview\r\nThis comparison provides insights into the key differences and similarities between two " +
        "distinct Test Sessions. It serves as a quick reference to gauge the overall performance and outcome of the " +
        "test executions.";
    private readonly CompositeFormat Summary = CompositeFormat.Parse(
        "## Summary\r\n" +
        "- **Start Date:**\r\n  - **Session 1:** `{0}`\r\n  - **Session 2:** `{1}`\r\n" +
        "- **URL:**\r\n  - **Session 1:** `{2}`\r\n  - **Session 2:** `{3}`\r\n" +
        "- **Number of Steps:**\r\n  - **Session 1:** `{4}`\r\n  - **Session 2:** `{5}`\r\n" +
        "- **Session Execution Duration:**\r\n  - **Session 1:** `{6}`\r\n  - **Session 2:** `{7}`\r\n" +
        "- **Test Failures:**\r\n  - **Session 1:** `{8}`\r\n  - **Session 2:** `{9}`");
    private const string DetailedComparison =
        "## Detailed Comparison";
    private readonly CompositeFormat DetailedStartDate = CompositeFormat.Parse(
        "### Start Date\r\nThe start date marks the initiation of each test session. A comparison of start dates can " +
        "indicate scheduling efficiency and potential delays in the testing process.\r\n" +
        "- **Session 1 Start Date:** `{0}`\r\n- **Session 2 Start Date:** `{1}`");
    private readonly CompositeFormat DetailedExecutionDuration = CompositeFormat.Parse(
        "### Execution Duration\r\nThe duration of each session is a measure of test efficiency and can highlight " +
        "performance issues.\r\n- **Session 1 Duration:** `{0}`\r\n- **Session 2 Duration:** `{1}`");
    private readonly CompositeFormat DetailedFailuresCount = CompositeFormat.Parse(
        "### Test Failures\r\nFailures are critical to identify areas of concern and focus on improving test stability.\r\n" +
        "- **Session 1 Failures:** `{0}`\r\n- **Session 2 Failures:** `{0}`");
    private readonly CompositeFormat DetailedFailuresAnalysis = CompositeFormat.Parse(
        "- **Session {0} Failures:**\r\n{1}\r\n");
    private readonly CompositeFormat DetailedFailureStep = CompositeFormat.Parse(
        "`{0}` failed after `{1}` with error: `{2}`");
    private const string DetailedStepByStepAnalysis =
        "### Step-by-Step Analysis\r\nA granular analysis of each step allows for a deeper understanding of specific " +
        "issues and successes within the test sessions.";
    private readonly CompositeFormat DetailedStepByStepChanges = CompositeFormat.Parse(
        "### Changes Between Session 1 and Session 2\r\nThis section highlights the differences that were " +
        "present in both sessions but have undergone changes.\r\n{0}");
    private readonly CompositeFormat DetailedStepByStepSingleChange = CompositeFormat.Parse(
        "- `{0}:` changed from `{1}` to `{2}`");
    private readonly CompositeFormat DetailedStepByStepAdditions = CompositeFormat.Parse(
        "### Additions in Session 2\r\nThis section lists the new steps that were added to Session 2 that were not " +
        "present in Session 1.\r\n{0}");
    private readonly CompositeFormat DetailedStepByStepSingleAdd = CompositeFormat.Parse(
        "- `{0}` added with value: `{1}`");
    private readonly CompositeFormat DetailedStepByStepRemovals = CompositeFormat.Parse(
        "### Removals from Session 1\r\nThis section outlines the steps that were present in Session 1 but have been " +
        "removed in Session 2.\r\n{0}");
    private readonly CompositeFormat DetailedStepByStepSingleRemove = CompositeFormat.Parse(
        "- `{0}` removed with value: `{1}`");
    private const string Conclusion =
        "### Conclusion\r\nThis detailed comparison sheds light on the performance and reliability of the test " +
        "sessions, guiding future improvements and ensuring the robustness of the system under test.";
    private const string ComparisonComplete =
        "## Comparison Complete\r\n\r\nThe analysis has concluded, and it appears that the two test sessions are " +
        "identical. There are no differences to report. This indicates a consistent performance between the sessions," +
        " suggesting stability and reliability in the tested components or features.\r\n\r\nIf you expected changes " +
        "or discrepancies, please verify that the correct sessions were compared or consider reviewing the test " +
        "parameters.\r\n\r\nThank you for using our comparison tool. We're here to assist you with any further " +
        "testing needs.";

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

        var output = new StringBuilder();

        if (IncludeTitle)
        {
            output.AppendLine(Title);
        }

        if (IncludeOverview)
        {
            output.AppendLine(Overview);
        }

        if (diffResult == DiffResult.Empty)
        {
            output.AppendLine(ComparisonComplete);
            return output.ToString();
        }

        if (IncludeSummary)
        {
            output.AppendLine(string.Format(CultureInfo.InvariantCulture, Summary,
                diffResult.Session1?.StartDate, diffResult.Session2?.StartDate,
                diffResult.Session1?.Url, diffResult.Session2?.Url,
                diffResult.Session1?.Steps.Count, diffResult.Session2?.Steps.Count,
                diffResult.Session1?.Duration.GetFormattedTime(), diffResult.Session2?.Duration.GetFormattedTime(),
                diffResult.Session1?.Failures.Count, diffResult.Session2?.Failures.Count));
        }

        if (IncludeDetailedComparison)
        {
            output.AppendLine(DetailedComparison);

            if (IncludeDetailedStartDate)
            {
                output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStartDate,
                    diffResult.Session1?.StartDate, diffResult.Session2?.StartDate));
            }

            if (IncludeDetailedExecutionDuration)
            {
                output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedExecutionDuration,
                    diffResult.Session1?.Duration.GetFormattedTime(), diffResult.Session2?.Duration.GetFormattedTime()));
            }

            if (IncludeDetailedFailures)
            {
                output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedFailuresCount,
                    diffResult.Session1?.Failures.Count, diffResult.Session2?.Failures.Count));
                
                if (diffResult.Session1?.Failures.Count > 0)
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedFailuresAnalysis,
                        "1",
                        string.Join("\r\n", diffResult.Session1.Failures.Select(f => 
                            string.Format(CultureInfo.InvariantCulture, DetailedFailureStep, 
                                f.Name, f.Duration, f.ErrorMessage)))));
                }

                if (diffResult.Session2?.Failures.Count > 0)
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedFailuresAnalysis,
                        "2",
                        string.Join("\r\n", diffResult.Session2.Failures.Select(f =>
                            string.Format(CultureInfo.InvariantCulture, DetailedFailureStep,
                                f.Name, f.Duration.GetFormattedTime(), f.ErrorMessage)))));
                }
            }

            if (IncludeDetailedStepByStepAnalysis)
            {
                output.AppendLine(DetailedStepByStepAnalysis);

                var changes = diffResult.Differences.Where(d => d.Type == DifferenceType.Changed).ToList();

                if (changes.Count > 0)
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepChanges,
                        string.Join("\r\n", changes.Select(change =>
                            string.Format(CultureInfo.InvariantCulture, DetailedStepByStepSingleChange,
                                change.PropertyName, change.Value1?.ToString(), change.Value2?.ToString())))));
                }
                else
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepChanges, 
                        "- No changes have been found."));
                }

                var additions = diffResult.Differences.Where(d => d.Type == DifferenceType.Added).ToList();

                if (additions.Count > 0)
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepAdditions,
                        string.Join("\r\n", additions.Select(add =>
                            string.Format(CultureInfo.InvariantCulture, DetailedStepByStepSingleAdd,
                                add.PropertyName, add.Value2?.ToString())))));
                }
                else
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepAdditions,
                        "- No additions have been found."));
                }

                var removals = diffResult.Differences.Where(d => d.Type == DifferenceType.Removed).ToList();

                if (removals.Count > 0)
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepRemovals,
                        string.Join("\r\n", removals.Select(del =>
                            string.Format(CultureInfo.InvariantCulture, DetailedStepByStepSingleRemove,
                                del.PropertyName, del.Value1?.ToString())))));
                }
                else
                {
                    output.AppendLine(string.Format(CultureInfo.InvariantCulture, DetailedStepByStepRemovals,
                        "- No removals have been found."));
                }
            }

            if (IncludeConclusion)
            {
                output.AppendLine(Conclusion);
            }
        }

        return output.ToString();
    }
}
