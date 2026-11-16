using Revisor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.Domain.Interfaces
{
    public interface ISubAgent
    {
        string Name { get; }
        Task<AgentExecution> ReviewAsync(string code, CancellationToken ct);
    }
}
