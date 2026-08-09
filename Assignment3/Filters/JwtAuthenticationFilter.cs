using Assignment3.Services.Interfaces;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace Assignment3.Filters
{
    public class JwtAuthenticationFilter
        : Attribute, IAuthenticationFilter
    {
        private readonly IJwtService _jwtService;

        public JwtAuthenticationFilter(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        public async Task AuthenticateAsync(
            HttpAuthenticationContext context,
            CancellationToken cancellationToken)
        {
            var authorizationHeader =
                context.Request.Headers.Authorization;

            // No Authorization header
            if (authorizationHeader == null)
            {
                return;
            }

            // Must be Bearer
            if (!string.Equals(
                authorizationHeader.Scheme,
                "Bearer",
                StringComparison.OrdinalIgnoreCase))
            {
                context.ErrorResult =
                    new UnauthorizedResult(
                        new[]
                        {
                            new AuthenticationHeaderValue("Bearer")
                        },
                        context.Request);

                return;
            }

            var token = authorizationHeader.Parameter;

            // Empty token
            if (string.IsNullOrWhiteSpace(token))
            {
                context.ErrorResult =
                    new UnauthorizedResult(
                        new[]
                        {
                            new AuthenticationHeaderValue("Bearer")
                        },
                        context.Request);

                return;
            }

            try
            {
                ClaimsPrincipal principal =
                    _jwtService.ValidateToken(token);

                // Set authenticated user
                context.Principal = principal;
            }
            catch
            {
                context.ErrorResult =
                    new UnauthorizedResult(
                        new[]
                        {
                            new AuthenticationHeaderValue("Bearer")
                        },
                        context.Request);
            }

            await Task.CompletedTask;
        }

        public async Task ChallengeAsync(
            HttpAuthenticationChallengeContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
