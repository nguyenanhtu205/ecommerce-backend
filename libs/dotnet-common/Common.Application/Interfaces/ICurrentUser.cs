namespace Common.Application.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsSeller { get; }
}
