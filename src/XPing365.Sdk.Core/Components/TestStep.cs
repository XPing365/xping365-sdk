﻿using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// This record represents a step in a test execution. It provides a set of properties that can be used to store 
/// information about the step, such as its name, start date, duration, result, and error message.
/// </summary>
/// <param name="Name">Represents the name of the test step.</param>
/// <param name="StartDate">Represents the start date of the test step.</param>
/// <param name="Duration">Represents the duration of the test step.</param>
/// <param name="Type">Represents the type of the test step.</param>
/// <param name="Result">Represents the result of the test step.</param>
/// <param name="PropertyBag">Represents the property bag which stores custom properties from test step.</param>
/// <param name="ErrorMessage">Represents the error message. It cannot be null if TestStepResult is Failed.</param>
public record TestStep(
    string Name,
    DateTime StartDate,
    TimeSpan Duration,
    TestStepType Type,
    TestStepResult Result,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Gets the name of the test step.
    /// </summary>
    public string Name { get; } = Name.RequireNotNullOrEmpty(nameof(Name));

    /// <summary>
    /// Gets the start date of the test step.
    /// </summary>
    public DateTime StartDate { get; } = StartDate.RequireCondition(
        // To prevent a difference between StartDate and this condition, we subtract 60 sec from the present date.
        // This difference can occur if StartDate is assigned just before 12:00 and this condition executes at 12:00. 
        condition: date => date >= (DateTime.Today.ToUniversalTime() - TimeSpan.FromSeconds(60)),
        parameterName: nameof(StartDate),
        message: Errors.IncorrectStartDate);

    /// <summary>
    /// Gets the error message if result is <see cref="TestStepResult.Failed"/>; otherwise null;
    /// </summary>
    public string? ErrorMessage { get; } = Result == TestStepResult.Failed ?
        ErrorMessage.RequireNotNullOrEmpty(nameof(ErrorMessage)) : null;

    public override string ToString()
    {
        string msg = $"{StartDate} " +
            $"({TotalTime(Duration)}) " +
            $"[{Type}] " +
            $"{Name} " +
            $"{Result.GetDisplayName()}.";

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            msg += $" {ErrorMessage}.";
        }

        return msg;
    }

    private string TotalTime(TimeSpan duration)
    {
        if (duration.TotalSeconds >= 1)
        {
            return $"{Math.Round(Duration.TotalSeconds, 2)}[s]";
        }

        return $"{Math.Round(Duration.TotalMilliseconds, 0)}[ms]";
    }
}
