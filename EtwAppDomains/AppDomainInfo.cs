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
    internal class AppDomainInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
