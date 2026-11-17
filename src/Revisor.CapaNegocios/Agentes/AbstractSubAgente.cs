using Revisor.CapaDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaNegocios.Agentes
{
    // PATRON STRATEGY: Contrato común de todos los subagentes.
    // El orquestador solo conoce esta clase abstracta.
    public abstract class AbstractSubAgente
    {
        public abstract string Nombre { get; }
        public abstract Task<EjecucionAgente> Revisar(string codigo, CancellationToken ct);
    }
}
