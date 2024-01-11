namespace XPing365.Sdk.Core.Validators;

public interface IValidator
{
    Task ValidateAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        IProgress<TestStep>? progress = null,
        CancellationToken cancellationToken = default);
}
