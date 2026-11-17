using System;
using System.Collections.Generic;
using System.Text;
using Revisor.CapaDatos.Entidades;
using Revisor.CapaDatos.Llm;
using Revisor.CapaNegocios.Herramientas;

namespace Revisor.CapaNegocios.Agentes
{
    // Subagentes de Seguridad, Calidad y Pruebas: usan LLM + regex para revisar el código.
    public class AgenteCalidad : AgenteLlm
    {
        public AgenteCalidad(AbstractLlm llm) : base(llm) { }
        public override string Nombre => "Calidad";
        protected override string ArchivoPrompt => "calidad.md";
        protected override List<Hallazgo> UsarHerramienta(string codigo) =>
            DetectorRegex.Analizar(codigo, Nombre, DetectorRegex.ReglasCalidad);
    }

    public class AgenteSeguridad : AgenteLlm
    {
        public AgenteSeguridad(AbstractLlm llm) : base(llm) { }
        public override string Nombre => "Seguridad";
        protected override string ArchivoPrompt => "seguridad.md";
        protected override List<Hallazgo> UsarHerramienta(string codigo) =>
            DetectorRegex.Analizar(codigo, Nombre, DetectorRegex.ReglasSeguridad);
    }

    public class AgentePruebas : AgenteLlm
    {
        public AgentePruebas(AbstractLlm llm) : base(llm) { }
        public override string Nombre => "Pruebas";
        protected override string ArchivoPrompt => "pruebas.md";
        protected override List<Hallazgo> UsarHerramienta(string codigo) =>
            DetectorRegex.Analizar(codigo, Nombre, DetectorRegex.ReglasPruebas);
    }
}
