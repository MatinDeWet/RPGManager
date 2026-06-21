using System.Security.Claims;
using Identification.Contracts;

namespace Identification.Implementation;

internal sealed class InfoSetter : List<Claim>, IInfoSetter
{
    public void SetUser(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        Clear();

        AddRange(claims);
    }

    public new void Clear()
    {
        base.Clear();
    }

    public new void AddRange(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        base.AddRange(claims);
    }
}
