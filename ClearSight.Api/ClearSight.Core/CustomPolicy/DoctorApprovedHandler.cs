using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.CustomPolicy
{
    public class DoctorApprovedHandler : AuthorizationHandler<DoctorApprovedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       DoctorApprovedRequirement requirement)
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            var verificationClaim = context.User.FindFirst("VerificationStatus");

            if (roleClaim != null && roleClaim.Value == "Doctor" &&
                verificationClaim != null && verificationClaim.Value == "Approved")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
