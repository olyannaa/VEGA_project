using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace vega.Logic;

public class JwtOptions
{
    public const string ISSUER = "VegaServer";
    public const string AUDIENCE = "VegaClient";
    public const int TIME = 45;
    private const string _KEY = "vega-super-secret-key-2024";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_KEY));
}