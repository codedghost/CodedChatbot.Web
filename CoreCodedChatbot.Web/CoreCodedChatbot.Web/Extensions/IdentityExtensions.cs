using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CoreCodedChatbot.Web.Extensions
{
    public static class IdentityExtensions
    {
        public static bool IsMod(this IEnumerable<ClaimsIdentity> identities)
        {
            return identities.Any(
                identity => identity.HasClaim(claim =>
                    claim.Type == "IsModerator" &&
                    string.Equals(claim.Value, "True", StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}