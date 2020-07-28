# EtwAppDomains
A simple console tool showing how to use ETW to force a GC in a .NET process. All relevant code is in [AppDomainHelper.cs](EtwAppDomains/AppDomainHelper.cs) and could be trivially moved to other libraries or tools (it technically depends on [Microsoft.Diagnostics.Tracing.TraceEvent](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent/) package only; however you might also not to reference [System.Runtime.InteropServices.RuntimeInformation](https://www.nuget.org/packages/System.Runtime.InteropServices.RuntimeInformation)).

Usage:

    EtwAppDomains.exe <PID>

This tool is based upon the code found [here](https://gist.github.com/lowleveldesign/bf4cf3e02e06d15f446658dd84296074) by [lowleveldesign](https://gist.github.com/lowleveldesign).
I have changed it to use typed events, a Task and CancellationToken instead of the ThreadPool (for timeouts) and to only collect information about one process,
and thereby enabling a definite end (so that the timeout is only a fallback).
