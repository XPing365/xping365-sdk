using System.Net.Http.Headers;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestValidators;

/// <summary>
/// The HttpResponseContentValidator class is a concrete implementation of the <see cref="TestComponent"/> class that is 
/// used to validate server content response. It takes a Func&lt;byte[], HttpContentHeaders, bool&gt; delegate as a
/// parameter, which is used to validate the response content. The onError parameter is an optional error message 
/// that can be used to provide additional information about the validation failure.
/// </summary>
/// <remarks>
/// <note>
/// The HttpResponseContentValidator component requires the HttpRequestSender component to be registered before it in 
/// the pipeline, because it depends on the HTTP response results from the HttpRequestSender component.
/// </note>
/// <para>
/// Server response content is received as a byte array and stored as such. If needed, it can be converted to a string 
/// using the encoding which is available in the <see cref="HttpContentHeaders.ContentEncoding" />.
/// </para>
/// <para>
/// When storing the server response content, it is generally recommended to store it as a byte array rather than a 
/// string. This is because the response content may contain binary data that cannot be represented as a string.
/// </para>
/// <para>
/// When an HTTP response contains multiple content encodings, the <see cref="HttpContentHeaders.ContentEncoding" /> 
/// property returns a collection of strings that represents the content encoding of the response content. The order of 
/// the encodings in the collection indicates the order in which they were applied to the response content.
/// 
/// To determine the correct content encoding to use, you should start with the first encoding in the collection and 
/// work your way down until you find an encoding that you can decode. If you are unable to decode any of the encodings, 
/// you should return an error.
/// </para>
/// <example>
/// <code>
/// var serverContentValidator = new HttpResponseContentValidator(
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
///     onError: (byte[] buffer, HttpContentHeaders contentHeaders) => 
///         $"The HTTP content response did not contain expected text.");
/// var validator = new Pipeline(serverContentValidator);
/// </code>
/// </example>
/// </remarks>
/// <param name="isValid">Func&lt;byte[], HttpContentHeaders, bool&gt; delegate used to validate the response 
/// content.
/// </param>
/// <param name="onError">Optional information about the validation failure.</param>
public class HttpResponseContentValidator(
    Func<byte[], HttpContentHeaders, bool> isValid,
    Func<byte[], HttpContentHeaders, string>? onError = null) :
        TestComponent(StepName, TestStepType.ValidateStep)
{
    /// <summary>
    /// The name of the test component that represents a HttpResponseContentValidator test operation.
    /// </summary>
    /// <remarks>
    /// This constant is used to register the HttpResponseContentValidator class in the test framework.
    /// </remarks>
    public const string StepName = "Server content response validation";

    private readonly Func<byte[], HttpContentHeaders, bool> _isValid = isValid;
    private readonly Func<byte[], HttpContentHeaders, string>? _onError = onError;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">
    /// An optional CancellationToken object that can be used to cancel this operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If any of the following parameters: url, settings or context is null.
    /// </exception>
    public override Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        using var instrumentation = new InstrumentationLog();
        TestStep testStep = null!;

        try
        {
            var response = context.GetNonSerializablePropertyBagValue<HttpResponseMessage>(
                PropertyBagKeys.HttpResponseMessage);
            var content = context.GetPropertyBagValue<byte[]>(PropertyBagKeys.HttpContent);

            if (response == null || content == null)
            {
                testStep = context.SessionBuilder.Build(
                    component: this,
                    instrumentation: instrumentation,
                    error: Errors.InsufficientData(component: this));
            }
            else
            {
                // Perform test step validation.
                bool isValid = _isValid(content, response.Content.Headers);

                if (isValid)
                {
                    testStep = context.SessionBuilder.Build(component: this, instrumentation);
                }
                else
                {
                    string? errmsg = _onError?.Invoke(content, response.Content.Headers);
                    testStep = context.SessionBuilder.Build(
                        component: this,
                        instrumentation: instrumentation,
                        error: Errors.ValidationFailed(component: this, errmsg));
                }
            }
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(component: this, instrumentation, exception);
        }
        finally
        {
            context.Progress?.Report(testStep);
        }

        return Task.FromResult(testStep);
    }
}
