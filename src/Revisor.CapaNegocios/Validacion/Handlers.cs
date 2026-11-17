using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Revisor.CapaNegocios.Validacion
{
    /// PATRÓN DE DISEÑO: Chain of Responsibility (Cadena de Responsabilidad).
    /// Clase base abstracta que define la estructura de cada filtro (eslabón) de validación.
    /// Permite pasar el código recibido por una serie de controles secuenciales antes de enviarlo a la IA.
  
    public abstract class AbstractHandler
    {
        // Referencia al siguiente filtro en la cadena. Si es null, significa que es el último filtro.
        protected AbstractHandler? siguiente;

        protected AbstractHandler(AbstractHandler? sgte)
        {
            siguiente = sgte;
        }

        
        public abstract string? Validar(string codigo);

        protected string? Pasar(string codigo) => siguiente?.Validar(codigo);
    }


    /// ESLABÓN 1: Valida que el usuario no envíe un texto vacío o con solo espacios en blanco.
    /// Evita gastar peticiones o cuota de API de Gemini analizando nada.
    public class HVacio : AbstractHandler
    {
        public HVacio(AbstractHandler? sgte) : base(sgte) { }

        public override string? Validar(string codigo)
        {
            // Si está vacío, corta la cadena inmediatamente y devuelve el error al controlador.
            if (string.IsNullOrWhiteSpace(codigo)) return "El código está vacío.";

            // Si tiene contenido, pasa el código al siguiente filtro (HTamanio).
            return Pasar(codigo);
        }
    }

   
    /// ESLABÓN 2: Valida que el código no supere el límite de caracteres permitido
    /// Cumple la regla de negocio de tamaño máximo y evita errores por exceso de tokens en la IA.
    public class HTamanio : AbstractHandler
    {
        private readonly int maximo;

        public HTamanio(int maximo, AbstractHandler? sgte) : base(sgte) { this.maximo = maximo; }

        public override string? Validar(string codigo)
        {
            if (codigo.Length > maximo) return $"El código supera el máximo de {maximo} caracteres.";

            // Si el tamaño es correcto, pasa el código al siguiente filtro (HPromptInjection).
            return Pasar(codigo);
        }
    }

    /// ESLABÓN 3: Filtro de seguridad que busca patrones maliciosos mediante Expresiones Regulares (Regex).
    /// Protege al sistema contra ataques de "Prompt Injection"
    public class HPromptInjection : AbstractHandler
    {
        // Expresión regular compilada que detecta frases típicas de manipulación en español e inglés.
        private static readonly Regex sospechoso = new(
            @"(ignora|ignore|olvida|forget)\s+(todas\s+las\s+|las\s+|all\s+|previous\s+|the\s+)?(instrucciones|instructions|reglas|rules)" +
            @"|system\s*prompt|you\s+are\s+now|ahora\s+eres",
            RegexOptions.IgnoreCase);

        public HPromptInjection(AbstractHandler? sgte) : base(sgte) { }

        public override string? Validar(string codigo)
        {
            // Si detecta palabras sospechosas, bloquea el envío a la IA por seguridad.
            if (sospechoso.IsMatch(codigo)) return "Se detectó un posible intento de prompt injection.";

           
            return Pasar(codigo);
        }
    }

    
    /// Orden: 1. HVacio -> 2. HTamanio -> 3. HPromptInjection -> Fin (null).
    ///  Facilita que el servicio principal cree toda la validación con una sola línea de código.
    public static class FabricaValidacion
    {
        public static AbstractHandler Crear(int maxCaracteres) =>
            new HVacio(new HTamanio(maxCaracteres, new HPromptInjection(null)));
    }
}
