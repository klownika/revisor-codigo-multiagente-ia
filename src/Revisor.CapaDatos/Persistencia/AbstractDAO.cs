using System;
using System.Collections.Generic;
using System.Text;
using Revisor.CapaDatos.Entidades;

namespace Revisor.CapaDatos.Persistencia
{
    public abstract class AbstractDAO
    {
        public abstract Task Insertar(Revision revision);
        public abstract Task Actualizar(Revision revision);
        public abstract Task<Revision?> Obtener(string id);
        public abstract Task<List<Revision>> Listar(int limite = 50);
    }
}
