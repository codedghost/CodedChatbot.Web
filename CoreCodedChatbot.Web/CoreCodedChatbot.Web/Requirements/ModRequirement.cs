using System.Threading.Tasks;
using CoreCodedChatbot.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace CoreCodedChatbot.Web.Requirements;

public class ModRequirement : AuthorizationHandler<ModRequirement>, IAuthorizationRequirement
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ModRequirement requirement)
    {
        if (context.User.Identities.IsMod())
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}