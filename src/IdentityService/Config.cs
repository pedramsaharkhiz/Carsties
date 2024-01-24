using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new("auctionApp","Auction full app access")
        ];

    public static IEnumerable<Client> Clients =>
        [
            // m2m client credentials flow client
            new Client
            {
                ClientId="postman",
                ClientName="Postman",
                AllowedScopes={"openid","profile","auctionApp"},
                RedirectUris={"https://getpostman.com/oauth2/callback"},
                ClientSecrets=[new Secret("NotASecret".Sha256())],
                AllowedGrantTypes={GrantType.ResourceOwnerPassword}
            },
            new Client
            {
                ClientId="nextApp",
                ClientName="nextApp",
                AllowedScopes={"openid","profile","auctionApp"},
                RedirectUris={"http://localhost:3000/api/auth/callback/id-server"},
                ClientSecrets=[new Secret("secret".Sha256())],
                AllowedGrantTypes=GrantTypes.CodeAndClientCredentials,
                AllowOfflineAccess=true,
                RequirePkce=false,
                AccessTokenLifetime=3600*24*30
            }
        ];
}
