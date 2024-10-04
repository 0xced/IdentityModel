// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using IdentityModel.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace IdentityModel.UnitTests;

public static class ProtocolResponseExtensions
{
    public static ProtocolResponseAssertions Should(this ProtocolResponse response) => new(response);
}

public class ProtocolResponseAssertions : ReferenceTypeAssertions<ProtocolResponse, ProtocolResponseAssertions>
{
    public ProtocolResponseAssertions(ProtocolResponse response) : base(response)
    {
    }

    protected override string Identifier => "response";

    public AndConstraint<ProtocolResponseAssertions> BeSuccessful(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(response => response.Error == null)
            .FailWith($"Expected response to be successful {{reason}}, but had error: {{0}}.{Environment.NewLine}{{1}}", Subject.Error, Subject.Raw)
            .Then
            .ForCondition(response => response.ErrorType == ResponseErrorType.None)
            .FailWith($"Expected response to be successful {{reason}}, but had error type: {{0}}.{Environment.NewLine}{{1}}", Subject.ErrorType, Subject.Raw)
            .Then
            .ForCondition(response => response.Exception == null)
            .FailWith($"Expected response to be successful {{reason}}, but had exception: {{0}}.{Environment.NewLine}{{1}}", Subject.Exception, Subject.Raw)
            .Then
            .ForCondition(response => response.IsError == false)
            .FailWith($"Expected response to be successful {{reason}}, but IsError is true.{Environment.NewLine}{{0}}", Subject.Raw);
        return new AndConstraint<ProtocolResponseAssertions>(this);
    }
}
