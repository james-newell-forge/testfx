// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Helpers;

namespace Microsoft.Testing.Platform.Services;

internal static class ExitCodeIgnorePolicy
{
    /// <summary>
    /// Applies the shared <c>--ignore-exit-code</c> / <c>TESTINGPLATFORM_EXITCODE_IGNORE</c> policy to the
    /// given exit code, returning <see cref="ExitCode.Success"/> when the code is in the ignore list.
    /// </summary>
    /// <remarks>
    /// Kept as a single shared implementation so every exit-code verdict (test results, coverage threshold,
    /// ...) is filtered consistently. Verdicts that are computed after
    /// <see cref="ITestApplicationProcessExitCode.GetProcessExitCode"/> has already run (for example the
    /// coverage threshold verdict applied by the hosts) must be routed through this so they can be ignored
    /// the same way the built-in verdicts are.
    /// </remarks>
    public static int Apply(int exitCode, ICommandLineOptions commandLineOptions, IEnvironment environment)
    {
        // If the user has specified the IgnoreExitCode, then we don't want to return a non-zero exit code if the exit code matches the one specified.
        string? exitCodeToIgnore = environment.GetEnvironmentVariable(EnvironmentVariableConstants.TESTINGPLATFORM_EXITCODE_IGNORE);
        if (RoslynString.IsNullOrEmpty(exitCodeToIgnore))
        {
            if (commandLineOptions.TryGetOptionArgumentList(PlatformCommandLineProvider.IgnoreExitCodeOptionKey, out string[]? commandLineExitCodes) && commandLineExitCodes.Length > 0)
            {
                exitCodeToIgnore = commandLineExitCodes[0];
            }
        }

        return exitCodeToIgnore is not null
            && exitCodeToIgnore.Split(';').Any(code => int.TryParse(code, out int parsedExitCode) && parsedExitCode == exitCode)
            ? (int)ExitCode.Success
            : exitCode;
    }
}
