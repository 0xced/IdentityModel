// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace IdentityModel.UnitTests;

public static class NetworkHandlerExtensions
{
    public static NetworkHandlerAssertions Should(this NetworkHandler handler) => new(handler);
}

public class NetworkHandlerAssertions : ReferenceTypeAssertions<NetworkHandler, NetworkHandlerAssertions>
{
    public NetworkHandlerAssertions(NetworkHandler handler) : base(handler)
    {
    }

    protected override string Identifier => "handler";

    public AndConstraint<NetworkHandlerAssertions> HaveBody(Dictionary<string, StringValues> values, string because = "", params object[] becauseArgs)
    {
        // TODO: Execute.Assertion with proper messages
        var requestBody = QueryHelpers.ParseQuery(Subject.Body);
        requestBody.Should().BeEquivalentTo(values, because, becauseArgs);
        return new AndConstraint<NetworkHandlerAssertions>(this);
    }
}
