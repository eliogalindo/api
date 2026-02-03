namespace api.Core.Interfaces.Properties;

public interface ISoftDeletable
{
    public bool Deletable { get; set; }
    DateTime? DeletedAt { get; set; }
}