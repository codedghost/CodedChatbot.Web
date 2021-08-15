using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AspNet.Security.OAuth.Twitch;

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

        public static string GetTwitchUsername(this ClaimsPrincipal claims)
        {
            var usernameClaim = claims.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName);

            return usernameClaim?.Value;
        }
    }
}