﻿using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// A static class that provides factory methods for creating different types of errors.
/// </summary>
public static class Errors
{
    /// <summary>
    /// Creates an error from an exception
    /// </summary>
    /// <param name="ex">The exception to create the error from</param>
    /// <returns>An error with code 1000 and the exception message</returns>
    public static Error ExceptionError(Exception ex) =>
        new("1000", $"Message: {ex.RequireNotNull(nameof(ex)).Message}");

    /// <summary>
    /// Creates an error when no Http clients are registered in the service provider
    /// </summary>
    /// <returns>An error with code 1010 and a message instructing to invoke AddHttpClients()</returns>
    public static Error HttpClientsNotFound =>
        new("1010", $"The service provider does not have any Http clients registered. You need to invoke " +
            $"`AddHttpClients()` to add them before you can use them.");

    /// <summary>
    /// Creates an error when no Headless browsers are registered in the service provider
    /// </summary>
    /// <returns>An error with code 1011 and a message instructing to invoke AddBrowserClients()</returns>
    public static Error HeadlessBrowserNotFound =>
        new("1011", $"The service provider does not have any Headless browsers registered. You need to invoke " +
            $"`AddBrowserClients()` to add them before you can use them.");

    /// <summary>
    /// Creates an error when there is insufficient data to perform a test step
    /// </summary>
    /// <param name="component">The test component that requires data</param>
    /// <returns>An error with code 1100 and a message indicating the test component name</returns>
    public static Error InsufficientData(TestComponent component) =>
        new("1100", $"Insufficient data to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step.");

    /// <summary>
    /// Creates an error when the validation fails to perform a test step
    /// </summary>
    /// <param name="component">The test component that failed validation</param>
    /// <returns>An error with code 1101 and a message indicating the test component name</returns>
    public static Error ValidationFailed(TestComponent component) =>
        new("1101", $"Validation failed to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step.");

    /// <summary>
    /// Creates an error when the validation fails to perform a test step with an optional error message
    /// </summary>
    /// <param name="component">The test component that failed validation</param>
    /// <param name="errorMessage">The optional error message to include</param>
    /// <returns>An error with code 1101 and a message indicating the test component name and the error message</returns>
    public static Error ValidationFailed(TestComponent component, string? errorMessage) =>
        new("1101", $"Validation failed to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step." +
                    $" {errorMessage}");

    /// <summary>
    /// Creates an error when the DNS lookup fails to resolve the hostname
    /// </summary>
    /// <returns>An error with code 1110 and a message indicating the DNS lookup failure</returns>
    public static Error DnsLookupFailed =>
        new("1110", $"Could not resolve the hostname to any IP address.");

    /// <summary>
    /// Creates an error when the ping request fails
    /// </summary>
    /// <returns>An error with code 1111 and a message indicating the ping request failure</returns>
    public static Error PingRequestFailed =>
        new("1111", $"An error occurred while sending the ping request.");

    /// <summary>
    /// Creates an error when no test step handlers are found
    /// </summary>
    /// <returns>An error with code 1200 and a message indicating the absence of test step handlers</returns>
    public static Error NoTestStepHandlers =>
        new("1200", $"No test step handlers were found to perform any steps. " +
                    $"The test session has been marked as declined.");

    /// <summary>
    /// Creates an error when the URL is missing in the test session
    /// </summary>
    /// <returns>An error with code 1201 and a message indicating the missing URL</returns>
    public static Error MissingUrlInTestSession =>
        new("1201", $"Missing URL in test session");

    /// <summary>
    /// Creates an error when the start time is missing in the test session
    /// </summary>
    /// <returns>An error with code 1202 and a message indicating the missing start time</returns>
    public static Error MissingStartTimeInTestSession =>
        new("1202", $"Missing start time in test session");

    /// <summary>
    /// Creates an error when the start date is in the past
    /// </summary>
    /// <returns>An error with code 1203 and a message indicating the incorrect start date</returns>
    public static Error IncorrectStartDate =>
        new("1203", $"StartDate cannot be in the past.");
}

