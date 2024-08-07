namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal interface IIterator<T>
{
    void First();
    void Last();
    void Nth(int index);
    T? Current();
    bool IsAdvanced { get; }
}
