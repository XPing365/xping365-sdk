using System.Runtime.Serialization;

namespace XPing365.Sdk.Core.Common;

public interface IPropertyBag : ISerializable
{
    Type DataContractType { get; }
}
