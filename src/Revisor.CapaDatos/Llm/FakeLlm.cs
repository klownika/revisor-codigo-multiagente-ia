using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaDatos.Llm
{

    // Adaptador falso: desarrollar y probar sin gastar cuota de Gemini.
    public class FakeLlm : AbstractLlm
    {
        private readonly string respuesta;
        private readonly TimeSpan demora;

        public FakeLlm(string? respuesta = null, TimeSpan? demora = null)
        {
            this.respuesta = respuesta ??
                """{"hallazgos":[{"severidad":"Baja","linea":1,"regla":"FAKE","mensaje":"Hallazgo de prueba del LLM falso","sugerencia":"Ninguna"}]}""";
            this.demora = demora ?? TimeSpan.FromMilliseconds(300);
        }

        public override async Task<RespuestaLlm> Completar(string instruccionSistema, string contenidoUsuario, CancellationToken ct)
        {
            await Task.Delay(demora, ct);
            return new RespuestaLlm(respuesta, 0);
        }
    }

}
