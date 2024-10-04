using FluentAssertions;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace IdentityModel.UnitTests
{
    public class TokenClientRequestTests
    {
        private const string Endpoint = "http://server/token";

        private readonly HttpClient _client;
        private readonly NetworkHandler _handler;

        public TokenClientRequestTests()
        {
            var document = File.ReadAllText(FileName.Create("success_token_response.json"));
            _handler = new NetworkHandler(document, HttpStatusCode.OK);

            _client = new HttpClient(_handler)
            {
                BaseAddress = new Uri(Endpoint)
            };
        }

        [Fact]
        public async Task No_explicit_endpoint_address_should_use_base_address()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestClientCredentialsTokenAsync();

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveRequestUri(new Uri(Endpoint));
        }

        [Fact]
        public async Task Client_credentials_request_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestClientCredentialsTokenAsync(scope: "scope");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.ClientCredentials,
                ["client_id"] = "client",
                ["scope"] = "scope",
            });
        }

        [Fact]
        public async Task Device_request_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "device" });

            var response = await tokenClient.RequestDeviceTokenAsync(deviceCode: "device_code");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.DeviceCode,
                ["client_id"] = "device",
                ["device_code"] = "device_code",
            });
        }

        [Fact]
        public async Task Device_request_without_device_code_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "device" });

            Func<Task> act = async () => await tokenClient.RequestDeviceTokenAsync(null);

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("device_code");
        }

        [Fact]
        public async Task Password_request_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestPasswordTokenAsync(userName: "user", password: "password", scope: "scope");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.Password,
                ["client_id"] = "client",
                ["username"] = "user",
                ["password"] = "password",
                ["scope"] = "scope",
            });
        }

        [Fact]
        public async Task Password_request_without_password_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestPasswordTokenAsync(userName: "user");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.Password,
                ["client_id"] = "client",
                ["username"] = "user",
                ["password"] = "",
            });
        }

        [Fact]
        public async Task Password_request_without_username_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            Func<Task> act = async () => await tokenClient.RequestPasswordTokenAsync(userName: null);

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("username");
        }

        [Fact]
        public async Task Code_request_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestAuthorizationCodeTokenAsync(code: "code", redirectUri: "uri", codeVerifier: "verifier");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.AuthorizationCode,
                ["client_id"] = "client",
                ["code"] = "code",
                ["redirect_uri"] = "uri",
                ["code_verifier"] = "verifier",
            });
        }

        [Fact]
        public async Task Code_request_without_code_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            Func<Task> act = async () => await tokenClient.RequestAuthorizationCodeTokenAsync(code: null, redirectUri: "uri", codeVerifier: "verifier");

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("code");
        }

        [Fact]
        public async Task Code_request_without_redirect_uri_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            Func<Task> act = async () => await tokenClient.RequestAuthorizationCodeTokenAsync(code: "code", redirectUri: null, codeVerifier: "verifier");

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("redirect_uri");
        }

        [Fact]
        public async Task Refresh_request_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { ClientId = "client" });

            var response = await tokenClient.RequestRefreshTokenAsync(refreshToken: "rt", scope: "scope");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = OidcConstants.GrantTypes.RefreshToken,
                ["client_id"] = "client",
                ["refresh_token"] = "rt",
                ["scope"] = "scope",
            });
        }

        [Fact]
        public async Task Refresh_request_without_refresh_token_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            Func<Task> act = async () => await tokenClient.RequestRefreshTokenAsync(refreshToken: null, scope: "scope");

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("refresh_token");
        }

        [Fact]
        public async Task Setting_no_grant_type_should_fail()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            Func<Task> act = async () => await tokenClient.RequestTokenAsync(grantType: null);

            (await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("grant_type");
        }

        [Fact]
        public async Task Setting_custom_parameters_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions());

            var parameters = new Parameters
            {
                { "client_id", "custom" },
                { "client_secret", "custom" },
                { "custom", "custom" }
            };

            var response = await tokenClient.RequestTokenAsync(grantType: "test", parameters: parameters);

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveNoAuthorizationHeader();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
                ["client_id"] = "custom",
                ["client_secret"] = "custom",
                ["custom"] = "custom",
            });
        }

        [Fact]
        public async Task Mixing_local_and_global_custom_parameters_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions { Parameters = { { "global", "global" } } });

            var parameters = new Parameters
            {
                { "client_id", "custom" },
                { "client_secret", "custom" },
                { "custom", "custom" }
            };

            var response = await tokenClient.RequestTokenAsync(grantType: "test", parameters: parameters);

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveNoAuthorizationHeader();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
                ["client_id"] = "custom",
                ["client_secret"] = "custom",
                ["custom"] = "custom",
                ["global"] = "global",
            });
        }

        [Fact]
        public async Task Local_custom_parameters_should_not_interfere_with_global()
        {
            var globalOptions = new TokenClientOptions { Parameters = { { "global", "value" } } };
            var tokenClient = new TokenClient(_client, globalOptions);

            var localParameters = new Parameters
            {
                { "client_id", "custom" },
                { "client_secret", "custom" },
                { "custom", "custom" }
            };
            
            var response = await tokenClient.RequestTokenAsync(grantType: "test", parameters: localParameters);

            response.Should().BeSuccessful();

            globalOptions.Parameters.Should().HaveCount(1);
            var globalValue = globalOptions.Parameters.FirstOrDefault(p => p.Key == "global").Value;
            globalValue.Should().Be("value");
        }

        [Fact]
        public async Task Setting_basic_authentication_style_should_send_basic_authentication_header()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions
            {
                ClientId = "client",
                ClientSecret = "secret",
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader
            });

            var response = await tokenClient.RequestTokenAsync(grantType: "test");

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveBasicAuthorizationHeader("client", "secret");
        }

        [Fact]
        public async Task Setting_post_values_authentication_style_should_post_values()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions
            {
                ClientId = "client",
                ClientSecret = "secret",
                ClientCredentialStyle = ClientCredentialStyle.PostBody
            });

            var response = await tokenClient.RequestTokenAsync(grantType: "test");

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveNoAuthorizationHeader();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
                ["client_id"] = "client",
                ["client_secret"] = "secret",
            });
        }

        [Fact]
        public async Task Setting_client_id_only_and_post_should_put_client_id_in_post_body()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions
            {
                ClientId = "client",
                ClientCredentialStyle = ClientCredentialStyle.PostBody
            });

            var response = await tokenClient.RequestTokenAsync(grantType: "test");

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveNoAuthorizationHeader();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
                ["client_id"] = "client",
            });
        }

        [Fact]
        public async Task Setting_client_id_only_and_header_should_put_client_id_in_header()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions
            {
                ClientId = "client",
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader
            });

            var response = await tokenClient.RequestTokenAsync(grantType: "test");

            response.Should().BeSuccessful();
            _handler.Request.Should().HaveBasicAuthorizationHeader("client", "");
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
            });
        }

        [Fact]
        public async Task Setting_client_id_and_assertion_should_have_correct_format()
        {
            var tokenClient = new TokenClient(_client, new TokenClientOptions
            {
                ClientId = "client",
                ClientAssertion = { Type = "type", Value = "value" }
            });

            var response = await tokenClient.RequestTokenAsync(grantType: "test");

            response.Should().BeSuccessful();
            _handler.Should().HaveBody(new Dictionary<string, StringValues>
            {
                ["grant_type"] = "test",
                ["client_id"] = "client",
                ["client_assertion_type"] = "type",
                ["client_assertion"] = "value",
            });
        }
    }
}
