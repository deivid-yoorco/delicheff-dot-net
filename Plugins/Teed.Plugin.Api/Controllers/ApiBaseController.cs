using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;
using System.Linq;
using System.Security.Claims;
using Teed.Plugin.Api.Attributes;

namespace Teed.Plugin.Api.Controllers
{
    [EnableCors("CorsPolicy")]
    [ApiAuthorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("Api/[controller]/[action]")]
    [ResponseFilter]
    public class ApiBaseController : BasePluginController
    {
        private string _userId;
        protected string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(_userId))
                {
                    Claim claim = User.Claims.FirstOrDefault(c => c.Type == "user_id");
                    _userId = claim == null ? "" : claim.Value;
                }
                return _userId;
            }
        }

        private string _userEmail;
        protected string UserEmail
        {
            get
            {
                if (string.IsNullOrEmpty(_userEmail))
                {
                    Claim claim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"));
                    _userEmail = claim == null ? "" : claim.Value;
                }
                return _userEmail;
            }
        }

        private string _roles;
        protected string Roles
        {
            get
            {
                if (string.IsNullOrEmpty(_roles))
                {
                    Claim claim = User.Claims.FirstOrDefault(c => c.Type.Contains("role"));
                    _roles = claim == null ? "" : claim.Value;
                }

                return _roles;
            }
        }

        protected bool UserIsInRole(string role)
        {
            return Roles.Contains(role);
        }
    }
}
