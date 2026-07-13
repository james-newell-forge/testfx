// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Helpers;

namespace Microsoft.Testing.Platform.Acceptance.IntegrationTests;

/// <summary>
/// Exercises the in-process (<c>ConsoleTestHost</c>) coverage-threshold exit-code override end to end:
/// a failed threshold turns an otherwise-successful run into <see cref="ExitCode.CoverageThresholdFailed"/>
/// (14), a passed threshold leaves the run successful, and an already-failing run keeps its original
/// (non-success) exit code instead of being overwritten by the coverage override.
/// </summary>
[TestClass]
public sealed class CoverageThresholdExitCodeTests : AcceptanceTestBase<CoverageThresholdExitCodeTests.TestAssetFixture>
{
    private const string AssetName = "CoverageThresholdExitCode";

    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    [TestMethod]
    public async Task FailedThreshold_WithPassingTests_ReturnsCoverageThresholdFailedExitCode(string currentTfm)
    {
        var testHost = TestInfrastructure.TestHost.LocateFrom(AssetFixture.TargetAssetPath, AssetName, currentTfm);
        TestHostResult testHostResult = await testHost.ExecuteAsync(
            environmentVariables: new Dictionary<string, string?>
            {
                ["COVERAGE_THRESHOLD_STATUS"] = "Failed",
                ["FAIL_TEST"] = "0",
            },
            cancellationToken: TestContext.CancellationToken);

        testHostResult.AssertExitCodeIs(ExitCode.CoverageThresholdFailed);
    }

    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    [TestMethod]
    public async Task PassedThreshold_WithPassingTests_ReturnsSuccess(string currentTfm)
    {
        var testHost = TestInfrastructure.TestHost.LocateFrom(AssetFixture.TargetAssetPath, AssetName, currentTfm);
        TestHostResult testHostResult = await testHost.ExecuteAsync(
            environmentVariables: new Dictionary<string, string?>
            {
                ["COVERAGE_THRESHOLD_STATUS"] = "Passed",
                ["FAIL_TEST"] = "0",
            },
            cancellationToken: TestContext.CancellationToken);

        testHostResult.AssertExitCodeIs(ExitCode.Success);
    }

    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    [TestMethod]
    public async Task FailedThreshold_WithFailingTest_RetainsOriginalNonSuccessExitCode(string currentTfm)
    {
        var testHost = TestInfrastructure.TestHost.LocateFrom(AssetFixture.TargetAssetPath, AssetName, currentTfm);
        TestHostResult testHostResult = await testHost.ExecuteAsync(
            environmentVariables: new Dictionary<string, string?>
            {
                ["COVERAGE_THRESHOLD_STATUS"] = "Failed",
                ["FAIL_TEST"] = "1",
            },
            cancellationToken: TestContext.CancellationToken);

        // The run already failed because of a failing test, so the coverage override (which only applies
        // to an otherwise-successful run) must not overwrite the existing non-success exit code.
        testHostResult.AssertExitCodeIs(ExitCode.AtLeastOneTestFailed);
    }

    public sealed class TestAssetFixture() : TestAssetFixtureBase()
    {
        private const string Sources = """
#file CoverageThresholdExitCode.csproj

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseAppHost>true</UseAppHost>
    <Nullable>enable</Nullable>
    <LangVersion>preview</LangVersion>
    <NoWarn>$(NoWarn);NETSDK1201</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Testing.Platform" Version="$MicrosoftTestingPlatformVersion$" />
  </ItemGroup>
</Project>

#file Program.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;

public class Startup
{
    public static async Task<int> Main(string[] args)
    {
        var testApplicationBuilder = await TestApplication.CreateBuilderAsync(args);
        testApplicationBuilder.RegisterTestFramework(_ => new TestFrameworkCapabilities(), (_, __) => new DummyTestFramework());
        using ITestApplication app = await testApplicationBuilder.BuildAsync();
        return await app.RunAsync();
    }
}

public class DummyTestFramework : ITestFramework, IDataProducer
{
    public string Uid => nameof(DummyTestFramework);

    public string Version => "2.0.0";

    public string DisplayName => nameof(DummyTestFramework);

    public string Description => nameof(DummyTestFramework);

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Type[] DataTypesProduced => new[] { typeof(TestNodeUpdateMessage), typeof(TestCoverageThresholdMessage) };

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
        => Task.FromResult(new CreateTestSessionResult() { IsSuccess = true });

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
        => Task.FromResult(new CloseTestSessionResult() { IsSuccess = true });

    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        bool failTest = Environment.GetEnvironmentVariable("FAIL_TEST") == "1";
        IProperty state = failTest
            ? new FailedTestNodeStateProperty()
            : new PassedTestNodeStateProperty();

        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(context.Request.Session.SessionUid, new TestNode()
        {
            Uid = "Test1",
            DisplayName = "Test1",
            Properties = new PropertyBag(state),
        }));

        string? thresholdStatus = Environment.GetEnvironmentVariable("COVERAGE_THRESHOLD_STATUS");
        if (thresholdStatus == "Failed")
        {
            await context.MessageBus.PublishAsync(this, new TestCoverageThresholdMessage(70.0, 80.0, CoverageType.Line, CoverageThresholdStatus.Failed, CoverageThresholdStat.Minimum));
        }
        else if (thresholdStatus == "Passed")
        {
            await context.MessageBus.PublishAsync(this, new TestCoverageThresholdMessage(90.0, 80.0, CoverageType.Line, CoverageThresholdStatus.Passed, CoverageThresholdStat.Minimum));
        }

        context.Complete();
    }
}
""";

        public string TargetAssetPath => GetAssetPath(AssetName);

        public override (string ID, string Name, string Code) GetAssetsToGenerate() => (AssetName, AssetName,
                Sources
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$MicrosoftTestingPlatformVersion$", MicrosoftTestingPlatformVersion));
    }

    public TestContext TestContext { get; set; }
}
