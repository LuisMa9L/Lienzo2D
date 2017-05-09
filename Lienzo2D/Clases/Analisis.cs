using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lienzo2D.Clases
{
    class Analisis
    {
        #region "Atributos"
        private LinkedList<Dictionary<String, Variable>> listaLista;
        private Dictionary<String, Variable> listaActual;
        List<Error> lista_errores;
        #endregion
        public Analisis() {
            lista_errores = new List<Error>();
        }
        public void RealizarAnalisis(string entrada) {
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //Parser de la cadena de entrada
            Clases.Gramatica gramatica = new Clases.Gramatica();
            Parser parser = new Parser(gramatica);
            ParseTree arbol = parser.Parse(entrada);
            ParseTreeNode raiz = arbol.Root;

            if (raiz == null || arbol.ParserMessages.Count > 0 || arbol.HasErrors())
            {
                //---------------------> Hay Errores
                foreach (var item in arbol.ParserMessages)
                {
                    string tipo, descripcion;
                    //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
                    //Se recolectan todos los erroes
                    if (item.Message.Contains("Invalid character"))
                    {
                        tipo = "Lexico";
                        descripcion = "Caracter Invalido";
                    }
                    else
                    {
                        tipo = "Sintactico";
                        descripcion = item.Message; ;
                    }
                    lista_errores.Add(new Error(item.Location.Line, item.Location.Column, tipo, descripcion));
                }
                GenerarTablaHLML(lista_errores);
                //errores.Replace("expected", "se esperaba");
                return;
            }
            //---------------------> Todo Bien
            iniciarAmbitoGlobal();
            ejecutar(raiz);
            if (lista_errores.Count!=0)
            {
                GenerarTablaHLML(lista_errores);
            }
            Clases.Graficar g = new Clases.Graficar();
            //g.graficar(arbol);
            //g.abrirArbol(g.desktop + "\\Files\\Arbol\\arbol.png");
        }
        public void GenerarTablaHLML(List<Error> lista) {
            string datos = "<html> \n";
            datos += "<body> \n";
            datos += "<center><h2> Reporte de Errores </h2></center>";
            datos += "<center>";
            datos += "<table border = 4>";
            datos += "<tr>";
            datos += "<td><center><b>" + "Linea" + "</b></center></td>";
            datos += "<td><center><b>" + "Columna" + "</b></center></td>";
            datos += "<td><center><b>" + "Tipo" + "</b></center></td>";
            datos += "<td><center><b>" + "Descripcion" + "</b></center></td>";
            datos += "</tr>";
            foreach (var item in lista)
            {
                datos += "<tr>";
                datos += "<td><center><b>" + item.linea + "</b></center></td>";
                datos += "<td><center><b>" + item.columna + "</b></center></td>";
                datos += "<td><center><b>" + item.tipo + "</b></center></td>";
                datos += "<td><center><b>" + item.descripcion + "</b></center></td>";
                datos += "</tr>";
            }
            datos += "</table>";
            datos += "</center>";
            datos += "</html>";

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //Crear y ver html
            String desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            System.IO.File.WriteAllText(desktop + "\\Files\\ht.html", datos);
            Process.Start(desktop + "\\Files\\ht.html");

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
        }

        #region "Manejo de Variables y Ambitos"        

        private void guardarVariable(Variable v, int linea, int columan)
        {
            if (!this.listaActual.ContainsKey(v.nombre))
            {
                listaActual.Add(v.nombre, v);
                Console.WriteLine("variable guardada >>> "+v.nombre+">>> tipo >>> "+v.tipo);
                return;
            }
            lista_errores.Add(new Error(linea, columan, "Semantico", "La variable " + v.nombre + " ya existe en el mismo ambito"));
        }
        private Variable getVariable(String nombre)
        {
            foreach (Dictionary<String, Variable> item in listaLista)
            {
                if (item.ContainsKey(nombre))
                {
                    Variable reto;
                    item.TryGetValue(nombre, out reto);
                    return reto;
                }
            }
            return null;
        }
        private void aumentarAmbito()
        {
            Dictionary<String, Variable> nuevo = new Dictionary<String, Variable>();
            listaLista.AddFirst(nuevo);
            listaActual = nuevo;
        }
        private void disminuirAmbito()
        {
            listaLista.RemoveFirst();
            listaActual = listaLista.First();
        }
        private void iniciarAmbitoGlobal()
        {
            listaLista = new LinkedList<Dictionary<string, Variable>>();
            listaActual = new Dictionary<string, Variable>();
            listaLista.AddLast(listaActual);
        }

        #endregion

        #region Ejecutar
        private void ejecutar(ParseTreeNode nodo)
        {
            switch (nodo.Term.Name)
            {
                case "LIENZOP":
                    foreach (var item in nodo.ChildNodes)
                    {
                        ejecutar(item);
                    }
                    break;
                case "LIENZO":
                    foreach (var item in nodo.ChildNodes[3].ChildNodes)
                    {
                        ejecutarSentenciasFuera(item);
                    }
                    Console.WriteLine(nodo.ChildNodes[1].Token.Text);
                    break;
                case "CUERPO":
                    ejecutar(nodo.ChildNodes[2]);
                    break;
                default: break;
            }
        }
        #endregion
        #region EjecutarSFuera
        private void ejecutarSentenciasFuera(ParseTreeNode nodo)
        {
            switch (nodo.Term.Name)
            {
                case "DECLARAR":
                    ejecutarDECLARAR(nodo);
                    break;
                case "LIENZO":
                    foreach (var item in nodo.ChildNodes)
                    {
                        ejecutar(item);
                    }
                    Console.WriteLine(nodo.ChildNodes[2].Token.Text);
                    ejecutar(nodo.ChildNodes[0]);
                    break;
                case "CUERPO":
                    ejecutar(nodo.ChildNodes[2]);
                    break;
                default: break;
            }
        }
        #endregion
        public void ejecutarDECLARAR(ParseTreeNode nodo)
        {
            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {
                String nombre = item.Token.Text;
                Variable nueva_variable = new Variable(nombre,irPoTipo(nodo.ChildNodes[2]));
                guardarVariable(nueva_variable, item.Token.Location.Line, item.Token.Location.Column);
            }
        }
        public int irPoTipo(ParseTreeNode nodo){

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //-------> entero == 1
            //-------> doble == 2
            //-------> boolena == 3
            //-------> cadena == 4
            //-------> caracter ==5

            int respuesta =0;
            if (nodo.ChildNodes[0].Token.Text=="entero")
            {
                return 1;
            }
            else if (nodo.ChildNodes[0].Token.Text =="doble")
            {
                return 2;
            }


            return respuesta;
            }

    }
}
