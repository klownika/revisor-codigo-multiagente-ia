using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Llm
{
    public record RespuestaLlm(string Contenido, int Tokens);
    public abstract class AbstractLlm
    {
        public abstract Task<RespuestaLlm> Completar(string instruccionSistema, string contenidoUsuario, CancellationToken ct);
    }
}
