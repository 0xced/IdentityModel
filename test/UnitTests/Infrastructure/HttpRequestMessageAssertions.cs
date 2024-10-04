// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace IdentityModel.UnitTests;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessageAssertions Should(this HttpRequestMessage handler) => new(handler);
}

public class HttpRequestMessageAssertions : ReferenceTypeAssertions<HttpRequestMessage, HttpRequestMessageAssertions>
{
    public HttpRequestMessageAssertions(HttpRequestMessage request) : base(request)
    {
    }

    protected override string Identifier => "HTTP request";

    public AndConstraint<HttpRequestMessageAssertions> HaveRequestUri(Uri uri, string because = "", params object[] becauseArgs)
    {
        // TODO: Execute.Assertion with proper messages
        Subject.RequestUri.Should().Be(uri, because, becauseArgs);
        return new AndConstraint<HttpRequestMessageAssertions>(this);
    }

    public AndConstraint<HttpRequestMessageAssertions> HaveNoAuthorizationHeader(string because = "", params object[] becauseArgs)
    {
        // TODO: Execute.Assertion with proper messages
        Subject.Headers.Authorization.Should().BeNull(because, becauseArgs);
        return new AndConstraint<HttpRequestMessageAssertions>(this);
    }

    public AndConstraint<HttpRequestMessageAssertions> HaveBasicAuthorizationHeader(string user, string password, string because = "", params object[] becauseArgs)
    {
        var parameter = BasicAuthenticationOAuthHeaderValue.EncodeCredential(user, password);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(request => request.Headers.Authorization != null)
            .FailWith("Expected HTTP request authorization header scheme to be be basic {reason}, but authorization header is null.")
            .Then
            .ForCondition(request => request.Headers.Authorization?.Scheme == "Basic")
            .FailWith("Expected HTTP request authorization header scheme to be be basic {reason}, but was: {0}.", Subject.Headers.Authorization?.Scheme)
            .Then
            .ForCondition(request => request.Headers.Authorization?.Parameter == parameter)
            .FailWith("Expected HTTP request authorization header parameter to be {0} {reason}, but was: {1}.", parameter, Subject.Headers.Authorization?.Parameter);
        return new AndConstraint<HttpRequestMessageAssertions>(this);
    }
}
