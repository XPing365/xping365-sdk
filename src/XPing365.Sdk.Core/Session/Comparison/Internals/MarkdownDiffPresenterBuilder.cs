using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using XPing365.Sdk.Core.Session.Comparison.Internals.MarkdownDecorators;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Session.Comparison.Internals;

internal partial class MarkdownDiffPresenterBuilder
{
    public static string Build(MarkdownDiffPresenter presenter, DiffResult result)
    {
        bool empty = result == DiffResult.Empty;

        var builder = new StringBuilder()
            .Append(Include(presenter.IncludeTitle, Title))
            .Append(Include(presenter.IncludeOverview, () => OverviewSection))
            .Append(Include(empty, () => ComparisonCompleted))
            .Append(Include(!empty && presenter.IncludeSummary, () => SummarySection(result)))
            .Append(Include(
                !empty && presenter.IncludeDetailedComparison && presenter.IncludeDetailedStartDate,
                () => DetailedStartDate(result)))
            .Append(Include(
                !empty && presenter.IncludeDetailedComparison && presenter.IncludeDetailedExecutionDuration,
                () => DetailedExecutionDuration(result)))
            .Append(Include(
                !empty && presenter.IncludeDetailedComparison && presenter.IncludeDetailedFailures,
                () => DetailedFailures(result)))
            .Append(Include(
                !empty && presenter.IncludeDetailedComparison && presenter.IncludeDetailedStepByStepAnalysis,
                () => DetailedStepByStepAnalysis(result)))
            .Append(Include(!empty && presenter.IncludeConclusion, () => ConclusionSection));

        return builder.ToString();
    }

    private static readonly HeaderMdDecorator Title = H1(T("Test Session Comparison Report"));
    
    private static List<ITextReport> OverviewSection =>
    [
        H2(T("Overview")),
        P(T("This comparison provides insights into the key differences and similarities between two distinct Test " +
            "Sessions. It serves as a quick reference to gauge the overall performance and outcome of the test " +
            "executions."))
    ];

    private static List<ITextReport> DetailedSection() =>
    [
        P(H2(T("Detailed Comparison")))
    ];

    private static List<ITextReport> DetailedStartDate(DiffResult result) =>
    [
        P(H3(T("Start Date"))),
        P(T("The start date marks the initiation of each test session. A comparison of start dates can indicate " +
            "scheduling efficiency and potential delays in the testing process.")),
        L0(T("Session 1 Start Date: ")), P(C(T(result.Session1.StartDate.ToString(CultureInfo.InvariantCulture)))),
        L0(T("Session 2 Start Date: ")), P(C(T(result.Session2.StartDate.ToString(CultureInfo.InvariantCulture)))),
    ];

    private static List<ITextReport> DetailedExecutionDuration(DiffResult result) =>
    [
        P(H3(T("Execution Duration"))),
        P(T("The duration of each session is a measure of test efficiency and can highlight performance issues.")),
        L0(T("Session 1 Duration: ")), P(C(T(result.Session1.Duration.GetFormattedTime(TimeSpanUnit.Millisecond)))),
        L0(T("Session 2 Duration: ")), P(C(T(result.Session2.Duration.GetFormattedTime(TimeSpanUnit.Millisecond)))),
    ];

    private static List<ITextReport> DetailedFailures(DiffResult result) =>
    [
        P(H3(T("Test Failures"))),
        P(T("Failures are critical to identify areas of concern and focus on improving test stability.")),
        L0(T("Session 1 Failures: ")), P(C(T($"{result.Session1.Failures.Count}"))),
        T(string.Join("\r\n", result.Session1.Failures.Select(ToTestStepFailures))),
        L0(T("Session 2 Failures: ")), P(C(T($"{result.Session2.Failures.Count}"))),
        T(string.Join("\r\n", result.Session2.Failures.Select(ToTestStepFailures))),
    ];

    private static string ToTestStepFailures(TestStep testStep)
    {
        StringBuilder builder = new StringBuilder()
            .Append(L1(C(T(testStep.Name))).Generate())
            .Append(T(" failed after ").Generate())
            .Append(C(T(testStep.Duration.GetFormattedTime())))
            .Append(T(" with error: ").Generate())
            .Append(C(T(testStep.ErrorMessage)).Generate());

        return builder.ToString();
    }

    private static List<ITextReport> SummarySection(DiffResult result) =>
    [
        P(L0(B(T("Start Date:")))),
        L1(T("Session 1: ")), P(C(T(result.Session1.StartDate.ToString(CultureInfo.InvariantCulture)))),
        L1(T("Session 2: ")), P(C(T(result.Session2.StartDate.ToString(CultureInfo.InvariantCulture)))),
        P(L0(B(T("Url:")))),
        L1(T("Session 1: ")), P(C(T(result.Session1.Url.AbsoluteUri))),
        L1(T("Session 2: ")), P(C(T(result.Session2.Url.AbsoluteUri))),
        P(L0(B(T("Number of Steps:")))),
        L1(T("Session 1: ")), P(C(T($"{result.Session1.Steps.Count}"))),
        L1(T("Session 2: ")), P(C(T($"{result.Session2.Steps.Count}"))),
        P(L0(B(T("Session Execution Duration:")))),
        L1(T("Session 1: ")), P(C(T(result.Session1.Duration.GetFormattedTime(TimeSpanUnit.Millisecond)))),
        L1(T("Session 2: ")), P(C(T(result.Session2.Duration.GetFormattedTime(TimeSpanUnit.Millisecond)))),
        P(L0(B(T("Test Failures:")))),
        L1(T("Session 1: ")), P(C(T($"{result.Session1.Failures.Count}"))),
        L1(T("Session 2: ")), P(C(T($"{result.Session2.Failures.Count}"))),
    ];

    private static List<ITextReport> DetailedStepByStepAnalysis(DiffResult result) =>
    [
        P(H3(T("Step-by-Step Analysis"))),
        P(T("A granular analysis of each step allows for a deeper understanding of specific issues and successes " +
            "within the test sessions.")),
        P(H4(T("Changes Between **Session 1** and **Session 2**"))),
        P(T("This section highlights the differences that were present in both sessions but have undergone changes.")),
        T(string.Join("\r\n", result.Differences
            .Where(d => d.Type == DifferenceType.Changed)
            .Where(d => !d.PropertyName.StartsWith(nameof(TestStep), StringComparison.InvariantCulture))
            .Select(ToSessionChanges))),
        T(ToStepChanges(result.Differences
            .Where(d => d.Type == DifferenceType.Changed)
            .Where(d => d.PropertyName.StartsWith(nameof(TestStep), StringComparison.InvariantCulture)))),
        T(!result.Differences.Where(d => d.Type == DifferenceType.Changed).Any() ? "- No changes have been found.":""),
        P(H4(T("Additions in **Session 2**"))),
        P(T("This section lists the new steps that were added to Session 2 that were not present in Session 1.")),
        T(string.Join("\r\n", result.Differences
            .Where(d => d.Type == DifferenceType.Added)
            .Select(d => new
            {
                Difference = d,
                Match = TestStepNameRegex().Match(d.PropertyName)
            })
            .Where(x => x.Match.Success)
            .Select(x => ToStepAdditions(x.Match.Groups[1].Value, x.Match.Groups[2].Value, x.Difference)))),
        T(!result.Differences.Where(d => d.Type == DifferenceType.Added).Any() ? "- No changes have been found.":""),
        P(H4(T("Removals from **Session 1**"))),
        P(T("This section outlines the steps that were present in Session 1 but have been removed in Session 2.")),
        T(string.Join("\r\n", result.Differences
            .Where(d => d.Type == DifferenceType.Removed)
            .Select(d => new
            {
                Difference = d,
                Match = TestStepNameRegex().Match(d.PropertyName)
            })
            .Where(x => x.Match.Success)
            .Select(x => ToStepRemovals(x.Match.Groups[1].Value, x.Match.Groups[2].Value, x.Difference)))),
        T(!result.Differences.Where(d => d.Type == DifferenceType.Removed).Any() ? "- No changes have been found.":"")
    ];

    private static string ToStepAdditions(string name, string propertyName, Difference difference)
    {
        var builder = new StringBuilder()
            .Append(L0(C(T(name))).Generate())
            .Append(T(" added with value ").Generate())
            .Append(P(C(T(difference.Value2?.ToString()))).Generate());

        return builder.ToString();
    }

    private static string ToStepRemovals(string name, string propertyName, Difference difference)
    {
        var builder = new StringBuilder()
            .Append(L0(C(T(name))).Generate())
            .Append(T(" removed with value ").Generate())
            .Append(P(C(T(difference.Value1?.ToString()))).Generate());

        return builder.ToString();
    }

    private static string ToSessionChanges(Difference difference)
    {
        var builder = new StringBuilder()
            .Append(L0(C(T(difference.PropertyName))).Generate())
            .Append(T(" changed from ").Generate())
            .Append(C(T(GetFormattedString(difference.Value1))).Generate())
            .Append(T(" to ").Generate())
            .Append(P(C(T(GetFormattedString(difference.Value2)))).Generate());

        return builder.ToString();
    }

    private static string ToStepChanges(IEnumerable<Difference> differences)
    {
        Dictionary<string, IList<Tuple<string, Difference>>> stepsDict = ToStepsDictionary(differences);
        var builder = new StringBuilder();

        foreach (var step in stepsDict)
        {
            builder.Append(P(L0(B(T(step.Key)))).Generate());

            foreach (var diff in step.Value)
            {
                builder
                    .Append(L1(C(T(diff.Item1))).Generate())
                    .Append(T(" changed from ").Generate())
                    .Append(C(T(GetFormattedString(diff.Item2.Value1))).Generate())
                    .Append(T(" to ").Generate())
                    .Append(P(C(T(GetFormattedString(diff.Item2.Value2)))).Generate());
            }
        }

        return builder.ToString();
    }

    private static Dictionary<string, IList<Tuple<string, Difference>>> ToStepsDictionary(
        IEnumerable<Difference> differences)
    {
        return differences
            .Select(diff => new
            {
                Difference = diff,
                Match = TestStepNameRegex().Match(diff.PropertyName)
            })
            .Where(x => x.Match.Success)
            .GroupBy(x => x.Match.Groups[1].Value, x => Tuple.Create(x.Match.Groups[2].Value, x.Difference))
            .ToDictionary(g => g.Key, g => (IList<Tuple<string, Difference>>)[.. g]);
    }

    private static List<ITextReport> ConclusionSection =>
    [
        P(H3(T("Conclusion"))),
        P(T("This detailed comparison sheds light on the performance and reliability of the test sessions, guiding " +
            "future improvements and ensuring the robustness of the system under test."))
    ];

    private static List<ITextReport> ComparisonCompleted =>
    [
        P(H2(T("Comparison Complete"))),
        P(T("The analysis has concluded, and it appears that the two test sessions are identical. There are no " +
            "differences to report. This indicates a consistent performance between the sessions, suggesting " +
            "stability and reliability in the tested components or features.")),
        P(T("If you expected changes or discrepancies, please verify that the correct sessions were compared or " +
            "consider reviewing the test parameters.")),
        P(T("Thank you for using our comparison tool. We're here to assist you with any further testing needs."))
    ];

    private static string Include(bool hasFlag, HeaderMdDecorator report) =>
        hasFlag ? report.Generate() : string.Empty;

    private static string Include(bool hasFlag, Func<IList<ITextReport>> reports) =>
        hasFlag ? reports.Invoke().Aggregate(string.Empty, (str, report) => str += report.Generate()) : string.Empty;

    private static string GetFormattedString(object? value)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.GetFormattedTime();
        }

        return value?.ToString() ?? string.Empty;
    }

    private static HeaderMdDecorator H1(ITextReport report) => new(report, HeaderType.H1);
    private static HeaderMdDecorator H2(ITextReport report) => new(report, HeaderType.H2);
    private static HeaderMdDecorator H3(ITextReport report) => new(report, HeaderType.H3);
    private static HeaderMdDecorator H4(ITextReport report) => new(report, HeaderType.H4);
    private static ParagraphMdDecorator P(ITextReport report) => new(report);
    private static ListMdDecorator L0(ITextReport report) => new(report, nestedLevel: 0);
    private static ListMdDecorator L1(ITextReport report) => new(report, nestedLevel: 1);
    private static BoldTextMdDecorator B(ITextReport report) => new(report);
    private static CodeTextMdDecorator C(ITextReport report) => new(report);
    private static TextReport T(string? text) => new(text ?? string.Empty);
    [GeneratedRegex(@"TestStep\(""(.*?)""\)\.(.*)")]
    private static partial Regex TestStepNameRegex();
}
