namespace Identification.Contracts;

public interface IIdentityInfo
{
    string GetExternalUserId();

    long GetInternalUserId();

    bool IsAdmin();

    bool HasRole(string role);

    bool HasValue(string name);

    string GetValue(string name);
}
