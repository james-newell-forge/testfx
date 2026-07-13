// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Testing.Platform.Services;

internal interface ITestCoverageResult : IDataConsumer
{
    bool HasCoverageThresholdFailure { get; }

    IReadOnlyList<TestCoverageThresholdMessage> ThresholdEntries { get; }

    IReadOnlyList<TestCoverageMessage> CoverageEntries { get; }
}
