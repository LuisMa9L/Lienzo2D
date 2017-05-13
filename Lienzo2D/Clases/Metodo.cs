using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lienzo2D.Clases
{
    class Metodo
    {
        public String Nombre;
        public ParseTreeNode Nodo;
        public Metodo(String nom,ParseTreeNode Nod) {
            Nombre = nom;
            Nodo = Nod;
        }
    }
}
