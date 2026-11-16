using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.Domain.Interfaces
{
    public interface ILlmClient
    {
        Task<LlmResponse> CompleteAsync(string systemPrompt, string userContent, CancellationToken ct);
    }
    public record LlmResponse(string Content, int Tokens);
}
