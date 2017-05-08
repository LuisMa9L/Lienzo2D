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
        public void RealizarAnalisis(string entrada) {
            List<Error> lista_errores = new List<Error>();
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

            Clases.Graficar g = new Clases.Graficar();
            g.graficar(arbol);
            //g.abrirArbol(g.desktop + "\\Files\\Arbol\\arbol.png");
        }
        public void GenerarTablaHLML(List<Error> lista) {
            string datos = "<html> \n";
            datos += "<body> \n";
            datos += "<center><h2> Reporte de Errores </h2></center>";
            datos += "<center>";
            datos += "<table border = 4>";
            datos += "<tr>";
            datos +="<td><center><b>" + "Linea" + "</b></center></td>";
            datos +="<td><center><b>" + "Columna" + "</b></center></td>";
            datos +="<td><center><b>" + "Tipo" + "</b></center></td>";
            datos +="<td><center><b>" + "Descripcion" + "</b></center></td>";
            datos +="</tr>";
            foreach (var item in lista)
            {
                datos += "<tr>";
                datos += "<td><center><b>" + item.linea + "</b></center></td>";
                datos += "<td><center><b>" +item.columna +"</b></center></td>";
                datos += "<td><center><b>" + item.tipo+"</b></center></td>";
                datos += "<td><center><b>" + item.descripcion+"</b></center></td>";
                datos += "</tr>";
            }
            datos +="</table>";
            datos +="</center>";
            datos +="</html>";

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //Crear y ver html
            String desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            System.IO.File.WriteAllText(desktop + "\\Files\\ht.html", datos);
            Process.Start(desktop + "\\Files\\ht.html");

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
        }


    }
}
