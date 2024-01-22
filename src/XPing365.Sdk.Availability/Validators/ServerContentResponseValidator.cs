using System.Net.Http.Headers;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validators;

/// <summary>
/// The HttpStatusCodeValidator class is a concrete implementation of the <see cref="TestStepHandler"/> class that is 
/// used to validate server content response. It takes a Func&lt;byte[], HttpContentHeaders, bool&gt; delegate as a
/// parameter, which is used to validate the response content. The errorMessage parameter is an optional error message 
/// that can be used to provide additional information about the validation failure.
/// </summary>
/// <remarks>
/// Server response content is received as a byte array and stored as such. If needed, it can be converted to a string 
/// using the encoding which is available in the <see cref="HttpContentHeaders.ContentEncoding" />.
/// 
/// When an HTTP response contains multiple content encodings, the <see cref="HttpContentHeaders.ContentEncoding" /> 
/// property returns a collection of strings that represents the content encoding of the response content. The order of 
/// the encodings in the collection indicates the order in which they were applied to the response content.
/// 
/// To determine the correct content encoding to use, you should start with the first encoding in the collection and 
/// work your way down until you find an encoding that you can decode. If you are unable to decode any of the encodings, 
/// you should return an error.
/// </remarks>
/// <example>
/// <code>
/// var serverContentValidator = new ServerContentResponseValidator(
///     isValid: (byte[] buffer, HttpContentHeaders contentHeaders) =>
///     {
///         foreach (string encoding in contentHeaders.ContentEncoding)
///         {
///             try
///             {
///                 string contentString = Encoding.GetEncoding(encoding).GetString(buffer);
///                 return contentString.Contains("title", StringComparison.InvariantCulture);
///             }
///             catch (Exception)
///             {
///                 // Unable to decode content with this encoding, try the next one
///             }
///         }
/// 
///         return false;
///     },
///     errorMessage: (byte[] buffer, HttpContentHeaders contentHeaders) => 
///         $"The HTTP content response did not contain expected text.");
/// var validator = new Validator(serverContentValidator);
/// </code>
/// </example>
/// <param name="isValid">Func&lt;byte[], HttpContentHeaders, bool&gt; delegate used to validate the response 
/// content.
/// </param>
/// <param name="errorMessage">Optional information about the validation failure.</param>
public class ServerContentResponseValidator(
    Func<byte[], HttpContentHeaders, bool> isValid,
    Func<byte[], HttpContentHeaders, string>? errorMessage = null) : 
        TestStepHandler(StepName, TestStepType.ValidateStep)
{
    public const string StepName = "Server content response validation";

    private readonly Func<byte[], HttpContentHeaders, bool> _isValid = isValid;
    private readonly Func<byte[], HttpContentHeaders, string>? _errorMessage = errorMessage;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel this operation.
    /// </param>
    /// <returns><see cref="TestStep"/> object.</returns>
    /// <exception cref="ArgumentNullException">If any of the following parameters: url, settings or session is null.
    /// </exception>
    public override Task<TestStep> HandleStepAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        TestStep? sendHttpRequestStep = session.Steps.FirstOrDefault(step => step.Name == SendHttpRequest.StepName);

        if (sendHttpRequestStep == null)
        {
            return Task.FromResult(CreateFailedTestStep(Errors.InsufficientData(handler: this)));
        }
        
        TestStep testStep = null!;
        using var inst = new InstrumentationLog();
        try
        {
            byte[] contentBuffer = 
                sendHttpRequestStep.PropertyBag.GetProperty<byte[]>(PropertyBagKeys.HttpContent);
            HttpContentHeaders contentHeaders =
                sendHttpRequestStep.PropertyBag.GetProperty<HttpContentHeaders>(PropertyBagKeys.HttpContentHeaders);
        
            // Perform test step validation.
            bool isValid = _isValid(contentBuffer, contentHeaders);

            if (isValid)
            {
                testStep = CreateSuccessTestStep(inst.StartTime, inst.ElapsedTime, new PropertyBag());
            }
            else
            {
                string? errmsg = _errorMessage?.Invoke(contentBuffer, contentHeaders);
                testStep = CreateFailedTestStep(errmsg ?? Errors.ValidationFailed(handler: this));
            }
        }
        catch (Exception exception)
        {
            testStep = CreateTestStepFromException(exception, inst.StartTime, inst.ElapsedTime);
        }

        return Task.FromResult(testStep);
    }
}
