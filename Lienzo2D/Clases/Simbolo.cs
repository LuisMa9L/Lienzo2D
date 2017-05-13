using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lienzo2D.Clases
{
    class Simbolo
    {
        public String Id, Tipo, Rol, Ambito;
        public Simbolo(String id, String tipo, String rol, String ambito) {
            this.Id=id;
            this.Tipo=tipo;
            this.Rol = rol;
            this.Ambito = ambito;
        }

        
    }
}
