using FGB.Dominio.Interfaces.Seguranca;
using System.Security.Claims;

namespace FGB.Api.Contexts
{
    public class HttpCurrentUserContext : ICurrentUserContext
    {
        private const string SubjectClaim = "sub";
        private const string NameClaim = "name";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string UserId => GetClaimValue(SubjectClaim, ClaimTypes.NameIdentifier);

        public string UserName => GetClaimValue(NameClaim, ClaimTypes.Name);

        private string GetClaimValue(params string[] claimTypes)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return string.Empty;
            }

            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirstValue(claimType);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }
    }
}
