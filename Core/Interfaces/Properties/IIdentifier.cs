namespace api.Core.Interfaces.Properties;

public interface IIdentifier<TKey>
{
    TKey Id { get; set; }
}