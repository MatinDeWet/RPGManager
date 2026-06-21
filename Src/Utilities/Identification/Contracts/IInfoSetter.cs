using System.Security.Claims;

namespace Identification.Contracts;

public interface IInfoSetter : IList<Claim>
{
    void SetUser(IEnumerable<Claim> claims);
}
