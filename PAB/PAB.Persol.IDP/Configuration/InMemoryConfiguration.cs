using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;

namespace PAB.Persol.IDP.Configuration
{
    public class InMemoryConfiguration
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("pabapi", "Personal Address Book API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("mOUntains!@#$1954".Sha256()) }
                },
                new ApiResource("crosscomapi", "HCM Cross Common API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("HRI%^&09".Sha256()) }
                },
                new ApiResource("hrapi", "HCM HR API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("PrzZX*$^".Sha256()) }
                },
                new ApiResource("payrollapi", "HCM Pay API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("pAyS$cr@t".Sha256()) }
                },
                new ApiResource("globalapi", "HCM Global API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("AdminGloBS$cr@t".Sha256()) }
                },
                new ApiResource("stringsapi", "HCM Strings API",new List<string> { "role" })
                {
                    ApiSecrets = { new Secret("AdminSTRiN9gS$cr@t".Sha256()) }
                }

                //,new ApiResource {
                //    Name = "crosscomapi",
                //    DisplayName = "HCM Cross Common API",
                //    Description = "This has common APIs Cross HCM",
                //    UserClaims = new List<string> {"role"},
                //    ApiSecrets = new List<Secret> {new Secret("HRI%^&09".Sha256())},
                //    Scopes = new List<Scope>
                //    {
                //        new Scope("customAPI.read"),
                //        new Scope("customAPI.write")
                //    }
                //}
               
            };
        }

        //public static IEnumerable<ApiResource> GetApiResources()
        //{
        //    return new List<ApiResource>
        //    {
        //        new ApiResource("scope_used_for_hybrid_flow")
        //        {
        //            ApiSecrets =
        //            {
        //                new Secret("FaulTy!@#1962".Sha256())
        //            },
        //            Scopes =
        //            {
        //                new Scope
        //                {
        //                    Name = "scope_used_for_hybrid_flow",
        //                    DisplayName = "Scope for the scope_used_for_hybrid_flow ApiResource"
        //                }
        //            }
        //            //,UserClaims = { "role", "admin", "user", "some_api" }
        //        },
        //        new ApiResource("ProtectedApi")
        //        {
        //            DisplayName = "API protected",
        //            ApiSecrets =
        //            {
        //                new Secret("mOUntains!@#$1954".Sha256())
        //            },
        //            Scopes =
        //            {
        //                new Scope
        //                {
        //                    Name = "scope_used_for_api_in_protected_zone",
        //                    ShowInDiscoveryDocument = false
        //                }
        //            }
        //            ,UserClaims = { "role", "admin", "user", "safe_zone_api" }
        //        }
        //    };
        //}

        public static List<TestUser> GetUsers() => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                Username = "eric",
                Password = "password",

                Claims = new List<Claim>
                {
                    new Claim("given_name", "Eric"),
                    new Claim("family_name", "Boateng"),
                    new Claim("email", "eric.boateng@persol.net")
                }
            },
            new TestUser
            {
                SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                Username = "canibobo",
                Password = "password",

                Claims = new List<Claim>
                {
                    new Claim("given_name", "Jerry"),
                    new Claim("family_name", "Okantey"),
                    new Claim("email", "jerry.okantey@persol.net")
                }
            }
        };

        public static IEnumerable<IdentityResource> GetIdentityResources()  
        {
            return new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your role(s)", new List<string> { "role"}),
                new IdentityResource("country", "The country you're living in",
                    new List<string> { "country" }),
                new IdentityResource("subscriptionlevel", "Your subscription level",
                    new List<string> { "subscriptionlevel" })
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "PAB MVC Client",
                    ClientId = "pabmvcclient",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowOfflineAccess = true,
                    RedirectUris = new []
                    {
                        $"{configuration["URLs:PABMVCClient"]}/signin-oidc"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        $"{configuration["URLs:PABMVCClient"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "pabapi",
                        "roles",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("FaulTy!@#1962".Sha256())
                    },
                    PostLogoutRedirectUris = new []
                    {
                        //$"{configuration["URLs:ImageClientAddress"]}/signout-callback-oidc",
                        $"{configuration["URLs:PABMVCClient"]}/signout-callback-oidc"
                    }
                    //AlwaysIncludeUserClaimsInIdToken = true
                },
                new Client
                {
                    ClientName = "PAB Client",
                    ClientId = "pabimplicit",
                    ClientSecrets = new [] { new Secret("mOUntains!@#$1954".Sha256())},
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "pabapi"
                    },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = new []
                    {
                        //corsUrls.ToString()
                        $"{configuration["URLs:PABAPI"]}",
                        $"{configuration["URLs:PABClient1"]}",
                        $"{configuration["URLs:PABClient2"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:PABAPI"]}/swagger/oauth2-redirect.html",
                        $"{configuration["URLs:PABClient1"]}",
                        $"{configuration["URLs:PABClient2"]}",
                        $"{configuration["URLs:PABClient3"]}"

                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:PABAPI"]}/swagger",
                        $"{configuration["URLs:PABClient1"]}",
                        $"{configuration["URLs:PABClient2"]}/login"
                    }
                },
                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,

                    RedirectUris =           { "http://localhost:26317/callback.html" },
                    PostLogoutRedirectUris = { "http://localhost:26317/index.html" },
                    AllowedCorsOrigins =     { "http://localhost:26317" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "pabapi"
                    }
                },
                new Client
                {
                    ClientName = "Payroll Client",
                    ClientId = "payimplicit",
                    //ClientSecrets = new [] { new Secret("mOUntains!@#$1954".Sha256())},
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "payrollapi"
                    },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = new []
                    {
                        $"{configuration["URLs:PAYAPI"]}",
                        $"{configuration["URLs:PAYClient1"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:PAYAPI"]}/swagger/oauth2-redirect.html",
                        $"{configuration["URLs:PAYClient1"]}"

                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:PAYAPI"]}/swagger",
                        $"{configuration["URLs:PAYClient1"]}"
                    }
                },
                new Client
                {
                    ClientName = "HR Client",
                    ClientId = "hrimplicit",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "hrapi"
                    },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = new []
                    {
                        $"{configuration["URLs:HRAPI"]}",
                        $"{configuration["URLs:HRClient1"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:HRAPI"]}/swagger/oauth2-redirect.html",
                        $"{configuration["URLs:HRClient1"]}"

                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:HRAPI"]}/swagger",
                        $"{configuration["URLs:HRClient1"]}"
                    }
                },
                new Client
                {
                    ClientName = "HCM Cross Common Client",
                    ClientId = "crosscommonimplicit",
                    //ClientSecrets = new [] { new Secret("SyMbols!@#$4592".Sha256())},
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "crosscomapi"
                    },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = new []
                    {
                        $"{configuration["URLs:HCMCrossCommonAPI"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:HCMCrossCommonAPI"]}/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:HCMCrossCommonAPI"]}/swagger"
                    }
                },
                new Client
                {
                    ClientName = "HCM Cross Common",
                    ClientId = "crosscommon",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    RequireConsent = false,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "roles",
                        "crosscomapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("Y6LcHtyJQ3PDFovJYMEfRWJPB8c7rrSNsDHD-blie-Y".Sha256())
                    }

                    //AccessTokenType = AccessTokenType.Reference,
                   
                    //AccessTokenLifetime = 120,
                    //UpdateAccessTokenClaimsOnRefresh = true,
                    //AllowOfflineAccess = true,
                    //RedirectUris = new []
                    //{
                    //    $"{configuration["URLs:HRMVCClient1"]}/signin-oidc",
                    //    $"{configuration["URLs:HRMVCClient2"]}/signin-oidc",
                    //    $"{configuration["URLs:HRMVCClient3"]}/signin-oidc",
                    //    $"{configuration["URLs:HRMVCClient4"]}/signin-oidc",
                    //    $"{configuration["URLs:PAYMVCClient1"]}/signin-oidc",
                    //    $"{configuration["URLs:PAYMVCClient2"]}/signin-oidc",
                    //    $"{configuration["URLs:PAYMVCClient3"]}/signin-oidc",
                    //    $"{configuration["URLs:PAYMVCClient4"]}/signin-oidc"
                    //},
                    //AllowedCorsOrigins = new List<string>
                    //{
                    //    $"{configuration["URLs:HCMCrossCommonAPI"]}",
                    //    $"{configuration["URLs:HRMVCClient1"]}",
                    //    $"{configuration["URLs:HRMVCClient2"]}",
                    //    $"{configuration["URLs:HRMVCClient3"]}",
                    //    $"{configuration["URLs:HRMVCClient4"]}",
                    //    $"{configuration["URLs:PAYMVCClient1"]}",
                    //    $"{configuration["URLs:PAYMVCClient2"]}",
                    //    $"{configuration["URLs:PAYMVCClient3"]}",
                    //    $"{configuration["URLs:PAYMVCClient4"]}"
                    //},
                    
                    //,PostLogoutRedirectUris =
                    //{
                    //    $"{configuration["URLs:HRMVCClient1"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:HRMVCClient2"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:HRMVCClient3"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:HRMVCClient4"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:PAYMVCClient1"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:PAYMVCClient2"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:PAYMVCClient3"]}/signout-callback-oidc",
                    //    $"{configuration["URLs:PAYMVCClient4"]}/signout-callback-oidc"
                    //}
                    // ,AlwaysIncludeUserClaimsInIdToken = true
                
                },
                new Client
                {
                    ClientName = "HR MVC Client",
                    ClientId = "hrmvc",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AccessTokenType = AccessTokenType.Reference,
                    RequireConsent = false,
                    //AccessTokenLifetime = 120,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AllowOfflineAccess = true,
                    RedirectUris = new []
                    {
                        $"{configuration["URLs:HRMVCClient1"]}/signin-oidc",
                        $"{configuration["URLs:HRMVCClient2"]}/signin-oidc",
                        $"{configuration["URLs:HRMVCClient3"]}/signin-oidc",
                        $"{configuration["URLs:HRMVCClient4"]}/signin-oidc"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        $"{configuration["URLs:HRAPI"]}",
                        $"{configuration["URLs:HRMVCClient1"]}",
                        $"{configuration["URLs:HRMVCClient2"]}",
                        $"{configuration["URLs:HRMVCClient3"]}",
                        $"{configuration["URLs:HRMVCClient4"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "roles",
                        "hrapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("aIEa9h-q_cKsdg4HwaoIbOIkBw0HFSIbb424EfnY1R8".Sha256())
                    },

                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:HRMVCClient1"]}/signout-callback-oidc",
                        $"{configuration["URLs:HRMVCClient2"]}/signout-callback-oidc",
                        $"{configuration["URLs:HRMVCClient3"]}/signout-callback-oidc",
                        $"{configuration["URLs:HRMVCClient4"]}/signout-callback-oidc"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true
                
                },
                new Client
                {
                    ClientName = "Payroll MVC Client",
                    ClientId = "paymvc",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AccessTokenType = AccessTokenType.Reference,
                    RequireConsent = false,
                    //AccessTokenLifetime = 120,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AllowOfflineAccess = true,
                    RedirectUris = new []
                    {
                        $"{configuration["URLs:PAYMVCClient1"]}",
                        $"{configuration["URLs:PAYMVCClient2"]}",
                        $"{configuration["URLs:PAYMVCClient3"]}",
                        $"{configuration["URLs:PAYMVCClient4"]}"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        $"{configuration["URLs:PAYAPI"]}",
                        $"{configuration["URLs:PAYMVCClient1"]}",
                        $"{configuration["URLs:PAYMVCClient2"]}",
                        $"{configuration["URLs:PAYMVCClient3"]}",
                        $"{configuration["URLs:PAYMVCClient4"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "payrollapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("8triQLmhRFQCFMJqQ1yywpZ0Jx7HJur5lKi900p3SpU".Sha256())
                    },

                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:PAYMVCClient1"]}/signout-callback-oidc",
                        $"{configuration["URLs:PAYMVCClient2"]}/signout-callback-oidc",
                        $"{configuration["URLs:PAYMVCClient3"]}/signout-callback-oidc",
                        $"{configuration["URLs:PAYMVCClient4"]}/signout-callback-oidc"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true
                
                },
                new Client
                {
                    ClientName = "CrossCommon APIs On Behalf Of Client",
                    ClientId = "crosscommononbehalfofclient",
                    AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                    RequireConsent = false,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "crosscomapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("SyMbols!@#$9319285".Sha256())
                    }
                },
                new Client
                {
                    ClientName = "Admin Global APIs On Behalf Of Client",
                    //ClientName = "Admin Global Client",
                    ClientId = "adminglobalonbehalfofclient",
                    //ClientId = "globalimplicit",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "globalapi"
                    },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                     AllowedCorsOrigins = new []
                    {
                        $"{configuration["URLs:PAYAPI2"]}",
                        $"{configuration["URLs:PAYClient2"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:PAYAPI2"]}/swagger/oauth2-redirect.html",
                        $"{configuration["URLs:PAYClient2"]}"

                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:PAYAPI2"]}/swagger",
                        $"{configuration["URLs:PAYClient2"]}"
                    }
                },
                new Client
                {
                    ClientName = "Admin Strings APIs On Behalf Of Client",
                    ClientId = "adminstringsonbehalfofclient",
                    AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins = new []
                    {
                        $"{configuration["URLs:StringsAPI1"]}",
                        $"{configuration["URLs:StringsAPI2"]}"
                    },
                    RedirectUris =  new []
                    {
                        $"{configuration["URLs:StringsAPI1"]}/swagger/oauth2-redirect.html",
                        $"{configuration["URLs:StringsAPI2"]}"

                    },
                    PostLogoutRedirectUris =
                    {
                        $"{configuration["URLs:StringsAPI1"]}/swagger",
                        $"{configuration["URLs:StringsAPI2"]}"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "stringsapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("@%(!)&b2891haLyUYf".Sha256())
                    }
                }
            };
        }
    }
}
