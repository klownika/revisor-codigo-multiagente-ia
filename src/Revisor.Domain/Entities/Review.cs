using Revisor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.Domain.Entities
{
    public record Finding(
    string Agent,
    Severity Severity,
    int? Line,
    string Rule,
    string Message,
    string Suggestion);

    public class AgentExecution
    {
        public string Agent { get; set; } = "";
        public AgentStatus Status { get; set; } = AgentStatus.Pending;
        public long DurationMs { get; set; }
        public int Tokens { get; set; }
        public string? Error { get; set; }
        public List<Finding> Findings { get; set; } = new();
    }

    public class Review
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Code { get; set; } = "";
        public string Language { get; set; } = "csharp";
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<AgentExecution> Executions { get; set; } = new();
        public List<Finding> FinalFindings { get; set; } = new();
        public string? Verdict { get; set; }            // "pass" | "fail"
        public bool Approved { get; set; }
    }
}
