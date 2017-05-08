using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lienzo2D.Clases
{
    class Error
    {
        public int linea, columna;
        public string tipo, descripcion;
        public Error(int linea, int columna, string tipo, string descripcion) {
            this.linea = linea;
            this.columna = columna;
            this.tipo = tipo;
            this.descripcion = descripcion;
        }
    }
}
