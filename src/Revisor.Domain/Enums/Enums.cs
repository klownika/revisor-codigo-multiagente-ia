using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.Domain.Enums
{
    public enum ReviewStatus { Pending, Running, AwaitingApproval, Completed, Failed }


    public enum AgentStatus { Pending, Running, Ok, Failed, TimedOut }
    public enum Severity { Info, Low, Medium, High, Critical }
}
