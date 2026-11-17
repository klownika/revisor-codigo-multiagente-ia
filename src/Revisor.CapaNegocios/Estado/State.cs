using Revisor.CapaDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.CapaNegocios.Estado
{
    /// PATRÓN DE DISEÑO: State (Estado) — Clase Contexto.
    /// Garantiza que el estado en memoria y el texto guardado en MongoDB siempre estén sincronizados.
    public class ContextoRevision
    {
        private AbstractEstado estado;
        public Revision Revision { get; }

        public ContextoRevision(Revision revision)
        {
            Revision = revision;
            estado = AbstractEstado.Desde(revision.Estado, this);
        }


        /// Actualiza tanto el objeto de estado interno como la propiedad string 'Estado' de la entidad
        /// para que quede lista para guardarse en MongoDB.
        public void cambiarEstado(AbstractEstado nuevoEstado)
        {
            estado = nuevoEstado;
            Revision.Estado = nuevoEstado.Nombre;
        }

        // Métodos puente: delegan la responsabilidad al estado en el que se encuentre la revisión.
        public string getEstado() => estado.Nombre;

        public void Iniciar() => estado.Iniciar();
        public void Terminar(bool hayResultados) => estado.Terminar(hayResultados);
        public void Aprobar(string revisor, string? comentario) => estado.Aprobar(revisor, comentario);
        public void Rechazar(string revisor, string? comentario) => estado.Rechazar(revisor, comentario);
    }


    /// PATRÓN DE DISEÑO: State (Estado) — Clase Base Abstracta.
    /// Define el contrato de todas las transiciones posibles del ciclo de vida de una revisión.
    /// Bloquea por defecto cualquier acción ilegal lanzando una excepción, evitando
    /// que la aplicación quede en estados corruptos (ej. aprobar una revisión que aún no analiza la IA).
    public abstract class AbstractEstado
    {
        protected ContextoRevision miRevision;

        protected AbstractEstado(ContextoRevision r) { miRevision = r; }

        public abstract string Nombre { get; }

        public virtual void Iniciar() => NoPermitido("iniciar");
        public virtual void Terminar(bool hayResultados) => NoPermitido("terminar");
        public virtual void Aprobar(string revisor, string? comentario) => NoPermitido("aprobar");
        public virtual void Rechazar(string revisor, string? comentario) => NoPermitido("rechazar");

        protected void NoPermitido(string accion) =>
            throw new InvalidOperationException($"No se puede {accion} una revisión en estado {Nombre}.");

        public static AbstractEstado Desde(string nombre, ContextoRevision r) => nombre switch
        {
            "Pendiente" => new Pendiente(r),
            "EnEjecucion" => new EnEjecucion(r),
            "EsperandoAprobacion" => new EsperandoAprobacion(r),
            "Completada" => new Completada(r),
            "Rechazada" => new Rechazada(r),
            "Fallida" => new Fallida(r),
            _ => throw new ArgumentException($"Estado desconocido: {nombre}")
        };
    }



    /// ESTADO 1: La revisión acaba de ser creada y está en cola esperando ser procesada.
    public class Pendiente : AbstractEstado
    {
        public Pendiente(ContextoRevision r) : base(r) { }
        public override string Nombre => "Pendiente";
        public override void Iniciar() => miRevision.cambiarEstado(new EnEjecucion(miRevision));
    }

    /// ESTADO 2 (Procesando): Los subagentes de IA están analizando el código en este momento.
    public class EnEjecucion : AbstractEstado
    {
        public EnEjecucion(ContextoRevision r) : base(r) { }
        public override string Nombre => "EnEjecucion";
        public override void Terminar(bool hayResultados) =>
            miRevision.cambiarEstado(hayResultados ? new EsperandoAprobacion(miRevision) : new Fallida(miRevision));
    }


    /// ESTADO 3: La IA ya entregó su reporte y espera la decisión de un humano.
    public class EsperandoAprobacion : AbstractEstado
    {
        public EsperandoAprobacion(ContextoRevision r) : base(r) { }
        public override string Nombre => "EsperandoAprobacion";

        public override void Aprobar(string revisor, string? comentario)
        {
            ValidarRevisor(revisor);
            miRevision.Revision.Decision = new Decision { Aprobado = true, Revisor = revisor, Comentario = comentario };
            miRevision.cambiarEstado(new Completada(miRevision));
        }

        public override void Rechazar(string revisor, string? comentario)
        {
            ValidarRevisor(revisor);
            miRevision.Revision.Decision = new Decision { Aprobado = false, Revisor = revisor, Comentario = comentario };
            miRevision.cambiarEstado(new Rechazada(miRevision));
        }

        // Regla de negocio: un revisor no puede aprobar su propio código
        private void ValidarRevisor(string revisor)
        {
            if (string.IsNullOrWhiteSpace(revisor)) throw new ArgumentException("Debe indicar el revisor.");
            if (string.Equals(revisor, miRevision.Revision.Autor, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Un revisor no puede decidir sobre código que él mismo envió.");
        }
    }

    // ESTADOS FINALES: solo lectura (regla de negocio), no permiten nada.
    public class Completada : AbstractEstado
    {
        public Completada(ContextoRevision r) : base(r) { }
        public override string Nombre => "Completada";
    }

    public class Rechazada : AbstractEstado
    {
        public Rechazada(ContextoRevision r) : base(r) { }
        public override string Nombre => "Rechazada";
    }

    public class Fallida : AbstractEstado
    {
        public Fallida(ContextoRevision r) : base(r) { }
        public override string Nombre => "Fallida";
    }
}
