﻿using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Common;
using System.Runtime.Serialization;

namespace XPing365.Sdk.Core.Session;

/// <summary>
/// This record represents a step in a test execution. It provides a set of properties that can be used to store 
/// information about the step, such as its name, start date, duration, result, and error message.
/// </summary>
/// <remarks>
/// This record can be serialized and its state can be saved using serializers that support the ISerializable interface.
/// </remarks>
[Serializable] // Indicates that this class can be serialized
public sealed record TestStep : ISerializable
{
    private readonly string _name = null!;
    private readonly DateTime _startDate;

    /// <summary>
    /// Gets the name of the test step.
    /// </summary>
    public required string Name
    {
        get => _name;
        init => _name = value.RequireNotNull(nameof(Name));
    }

    /// <summary>
    /// Gets the start date of the test step.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> object that represents the start time of the test step.
    /// </value>
    public required DateTime StartDate
    {
        get => _startDate;
        init => _startDate = value.RequireCondition(
            // To prevent a difference between StartDate and this condition, we subtract 60 sec from the present date.
            // This difference can occur if StartDate is assigned just before 12:00 and this condition executes at 12:00. 
            condition: date => date >= DateTime.Today.ToUniversalTime() - TimeSpan.FromSeconds(60),
            parameterName: nameof(StartDate),
            message: Errors.IncorrectStartDate);
    }

    /// <summary>
    /// Gets or sets the duration of the test step.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> object that represents the duration of the test step.
    /// </value>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets or sets the type of the test step.
    /// </summary>
    /// <value>
    /// A <see cref="TestStepType"/> enumeration value that indicates the type of the test step.
    /// </value>
    public required TestStepType Type { get; init; }

    /// <summary>
    /// Gets or sets the result of the test step.
    /// </summary>
    /// <value>
    /// A <see cref="TestStepResult"/> enumeration value that indicates the result of the test step.
    /// </value>
    public required TestStepResult Result { get; init; }

    /// <summary>
    /// Gets or sets the property bag of the test session.
    /// </summary>
    /// <value>
    /// A PropertyBag&lt;IPropertyBagValue&gt; object that contains key-value pairs of various data related to the test 
    /// operation, such as resolved IP addresses from DNS lookup, HTTP response headers, HTML content, or captured 
    /// screenshots from the headless browsers. 
    /// </value>
    /// <remarks>
    /// This property bag requires all objects to inherit from the <see cref="IPropertyBagValue"/> interface, so that 
    /// they can be serialized and deserialized using the serializers that support the ISerializable interface. This 
    /// enables the property bag to be saved and loaded to and from XML writers and readers.
    /// </remarks>
    public required PropertyBag<IPropertyBagValue>? PropertyBag { get; init; }

    /// <summary>
    /// Gets or sets the error message of the test step.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestStep"/> class.
    /// </summary>
    public TestStep()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestStep"/> class with serialized data.
    /// </summary>
    /// <param name="info">
    /// The <see cref="SerializationInfo"/> that holds the serialized object data about the test step.
    /// </param>
    /// <param name="context">
    /// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
    /// </param>
    public TestStep(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        _name = (string)info.GetValue(nameof(Name), typeof(string)).RequireNotNull(nameof(Name));
        _startDate = (DateTime)info.GetValue(nameof(StartDate), typeof(DateTime)).RequireNotNull(nameof(StartDate));
        Duration = (TimeSpan)info.GetValue(nameof(Duration), typeof(TimeSpan)).RequireNotNull(nameof(Duration));
        Type = Enum.Parse<TestStepType>((string)info
            .GetValue(nameof(Type), typeof(string))
            .RequireNotNull(nameof(Type)));
        Result = Enum.Parse<TestStepResult>((string)info
            .GetValue(nameof(Result), typeof(string))
            .RequireNotNull(nameof(Result)));
        PropertyBag = (PropertyBag<IPropertyBagValue>?)info.GetValue(
                name: nameof(PropertyBag),
                type: typeof(PropertyBag<IPropertyBagValue>));
        ErrorMessage = info.GetValue(nameof(ErrorMessage), typeof(string)) as string;
    }

    public override string ToString()
    {
        string msg = $"{StartDate} " +
            $"({Duration.GetFormattedTime()}) " +
            $"[{Type}]: " +
            $"{Name} " +
            $"{Result.GetDisplayName()}.";

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            msg += $" {ErrorMessage}.";
        }

        return msg;
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Name), Name, typeof(string));
        info.AddValue(nameof(StartDate), StartDate, typeof(DateTime));
        info.AddValue(nameof(Duration), Duration, typeof(TimeSpan));
        info.AddValue(nameof(Type), Type.ToString(), typeof(string));
        info.AddValue(nameof(Result), Result.ToString(), typeof(string));
        info.AddValue(nameof(PropertyBag), PropertyBag, typeof(PropertyBag<IPropertyBagValue>));
        info.AddValue(nameof(ErrorMessage), ErrorMessage, typeof(string));
    }
}
