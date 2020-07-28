using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;

namespace EtwAppDomains
{
    internal class AppDomainHelper : IDisposable
    {
        public static List<AppDomainInfo> GetAppDomains(int processId, TimeSpan timeout = default)
        {
            if (timeout == default)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            using (var helper = new AppDomainHelper(processId, timeout))
            {
                helper.Run();
                return helper.AppDomains;
            }
        }

        private TimeSpan m_timeout;
        private readonly int m_processId;
        private TraceEventSession m_session;
        private ClrRundownTraceEventParser m_parser;
        private Stopwatch m_elapsed = new Stopwatch();
        private CancellationTokenSource m_cts = new CancellationTokenSource();

        private AppDomainHelper(int processId, TimeSpan timeout)
        {
            m_timeout = timeout;
            m_processId = processId;
        }

        private List<AppDomainInfo> AppDomains { get; } = new List<AppDomainInfo>();

        private void Run()
        {
            m_session = new TraceEventSession("AppDomains-" + m_processId);
            m_parser = new ClrRundownTraceEventParser(m_session.Source);
            m_session.EnableProvider("Microsoft-Windows-DotNETRuntimeRundown", TraceEventLevel.Verbose,
                (ulong)(ClrRundownTraceEventParser.Keywords.StartEnumeration | ClrRundownTraceEventParser.Keywords.Loader),
                new TraceEventProviderOptions
                {
                    ProcessIDFilter = new List<int> { m_processId }
                });

            RegisterEvents();

            m_elapsed.Start();
            StartTimeoutHandler();

            // This is blocking, and will be stopped by "m_session.Stop()"
            m_session.Source.Process();
        }

        private void RegisterEvents()
        {
            m_parser.MethodDCStartInit += e =>
            {
                // Nothing, currently. AppDomain enumerate technically starts now.
            };
            m_parser.MethodDCStartComplete += e =>
            {
                if (e.ProcessID != m_processId)
                {
                    return;
                }
                m_session?.Stop();
                m_cts.Cancel();
            };
            m_parser.LoaderAppDomainDCStart += e =>
            {
                if (e.ProcessID != m_processId)
                {
                    return;
                }
                AppDomains.Add(new AppDomainInfo()
                {
                    Id = e.AppDomainID,
                    Name = e.AppDomainName,
                    Index = e.AppDomainIndex
                });
            };
        }

        private void StartTimeoutHandler()
        {
            m_cts.CancelAfter(m_timeout);

            var token = m_cts.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(1);

                    if (!m_session.IsActive)
                    {
                        break;
                    }

                    if (m_elapsed.Elapsed > m_timeout)
                    {
                        // rundown should be finished by now
                        m_elapsed.Stop();
                        m_session?.Stop();
                        break;
                    }
                }
            });
        }

        void IDisposable.Dispose()
        {
            m_session?.Dispose();
            m_session = null;
            m_cts.Dispose();
        }
    }
}
