using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lienzo2D.Clases
{
    class Ambito
    {
        public string nombre;
        public Dictionary<String, Variable> diccionario;
        public Ambito(string _nombre) {
            nombre = _nombre;
            diccionario = new Dictionary<string, Variable>();
        }
    }
}
