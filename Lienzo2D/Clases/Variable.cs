using System;

namespace Lienzo2D.Clases
{
    internal class Variable
    {
        public String nombre;
        public Object valor;
        public int tipo;
        public String atributo;

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
        //-------> entero == 1
        //-------> double == 2
        //-------> boolena == 3
        //------->  string == 4

        public Variable(String nom,int tip)
        {
            nombre = nom;
            tipo = tip;
        }
        public Variable(String nom, Object val, int tip)
        {
            nombre = nom;
            valor = val;
            tipo = tip;
        }
    }
}