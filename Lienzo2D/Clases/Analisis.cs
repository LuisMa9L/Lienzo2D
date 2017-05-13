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
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region "Atributos Generales, constructor "                     
        private LinkedList<Ambito> listaLista;    //-----> pila de ambitos
        private Ambito listaActual;               //-----> ambito actual
        public List<Error> lista_errores;
        public List<Simbolo> lista_simbolos;
        public Dictionary<String, Metodo> lista_metodos;
        public int ultimaLinea, ultimaColumna;
        public Dictionary<String, Variable> lista_variables_globales;
        public Analisis()
        {
            lista_errores = new List<Error>();                          //-----> lexicos, sintacticos, semanticos
            lista_simbolos = new List<Simbolo>();
            lista_metodos = new Dictionary<string, Metodo>();
            lista_variables_globales = new Dictionary<string, Variable>();
            ultimaLinea = 0;
            ultimaColumna = 0;
        }
        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region Analisis General, graficar, errores sintacticos 

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
            
            ejecutar(raiz);
            if (lista_errores.Count!=0)
            {
                GenerarTablaHLML(lista_errores);
                return;
            }
            GenerarTablaDeSimbolosHtml(lista_simbolos);
            Clases.Graficar g = new Clases.Graficar();
            //g.graficar(arbol);
            //g.abrirArbol(g.desktop + "\\Files\\Arbol\\arbol.png");
        }

        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region "Manejo de Variables y Ambitos"        

        private void guardarVariable(Variable v, int linea, int columan,string rol)
        {
            string ambito="";
            foreach (var item in listaLista)
            {
                if (ambito!="")
                {
                    ambito = "_"+ambito;
                }
                ambito = item.nombre+ambito;
                if (item.diccionario.ContainsKey(v.nombre))
                {
                    lista_errores.Add(new Error(linea, columan, "Semantico", "El "+ v.atributo+" "+ v.nombre + " ya existe"));
                    return;
                }
                if (lista_variables_globales.ContainsKey(v.nombre))
                {
                    lista_errores.Add(new Error(linea, columan, "Semantico", "El " + v.atributo + " " + v.nombre + " ya existe como global"));
                    return;
                }
            }
            listaActual.diccionario.Add(v.nombre, v);
            //Console.WriteLine("variable guardada >>> " + v.nombre + ">>> tipo >>> " + v.tipo);
            //ambito.Replace("$_", "nadnadadsfa");
            AgregarSimbolo(v.nombre, darTipo(v.tipo), rol, ambito);         //-----> Lo agrega a la tabla de simbolos 
                                                                                   //Console.WriteLine(ambito+" Ramon!!!");
        }
        private Variable getVariable(String nombre)
        {
            Variable valSalida;
            foreach (var item in listaLista)
            {
                if (item.diccionario.ContainsKey(nombre))
                {
                    item.diccionario.TryGetValue(nombre, out valSalida);
                    return valSalida;
                }

            }
            if (lista_variables_globales.ContainsKey(nombre))
            {
                lista_variables_globales.TryGetValue(nombre, out valSalida);
                return valSalida;
            }
            return null;
        }
        private Metodo getVMetodo(String nombre)
        {
            Metodo MetodoSalida;
                if (lista_metodos.ContainsKey(nombre))
                {
                lista_metodos.TryGetValue(nombre, out MetodoSalida);
                    return MetodoSalida;
                }
            return null;
        }
        private void aumentarAmbito(string nombre)
        {
            Ambito nuevo_a = new Ambito(nombre);
            listaLista.AddFirst(nuevo_a);
            listaActual = nuevo_a;
        }
        private void disminuirAmbito()
        {
            listaLista.RemoveFirst();
            listaActual = listaLista.First();
        }
        private void iniciarAmbitoGlobal(string nombre_clase)
        {
            listaLista = new LinkedList<Ambito>();
            listaActual = new Ambito(nombre_clase);
            listaLista.AddLast(listaActual);
        }
        public void AgregarSimbolo(String id, String tipo, String rol, String ambito) {
            lista_simbolos.Add(new Simbolo(id, tipo, rol, ambito));
        }


        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region Ejecutar metodos

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
                    iniciarAmbitoGlobal(nodo.ChildNodes[1].Token.Text);
                    foreach (var item in nodo.ChildNodes[3].ChildNodes)//-----> Ejecutar Sentencias de la clase -----> Primero las Variables Globales
                    {
                        if (item.Term.Name=="DECLARAR")
                        {
                            ejecutarVariablesGlobalesDeclarar(item);
                        }
                    }
                    foreach (var item in nodo.ChildNodes[3].ChildNodes)//-----> Ejecutar Sentencias de la clase ----> Segundo Las Funciones y Procedimientos
                    {
                        if (item.Term.Name == "FUNCION_PROCED")
                        {
                            ejecutarGuardarFuncion_Porced(item);
                        }
                    }
                    foreach (var item in nodo.ChildNodes[3].ChildNodes)//-----> Ejecutar Sentencias de la clase ----> Tercero el metodo    <<Principal>>
                    {
                        if (item.Term.Name == "PRINCIPAL")
                        {
                            ejecutarPrincipal(item);
                        }
                    }
                    break;
                case "CUERPO":
                    break;
                default: break;
            }
        }

        public void ejecutarVariablesGlobalesDeclarar(ParseTreeNode nodo) {   //-----> Declaarar Varialbles Globales
            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {
                String nombre = item.Token.Text;
                Variable nueva_variable = new Variable(nombre, irPoTipo(nodo.ChildNodes[2])); nueva_variable.atributo = "Atributo";
                if (lista_variables_globales.ContainsKey(nombre))
                {
                    lista_errores.Add(new Error(item.Token.Location.Line, item.Token.Location.Column, "Semantico", "La varialbe global " + nombre + " ya existe."));
                    return;
                }
                lista_variables_globales.Add(nombre, nueva_variable);


                string ambito = "";
                
                    if (ambito != "")
                    {
                        ambito = "_" + ambito;
                    }
                    //ambito = item.nombre + ambito;
               
                //AgregarSimbolo(v.nombre, darTipo(v.tipo), rol, ambito);         //-----> Lo agrega a la tabla de simbolos
            }
        }

        private void ejecutarSentenciasFuera(ParseTreeNode nodo)
        {
            switch (nodo.Term.Name)
            {
                case "DECLARAR":
                    ejecutarDECLARAR(nodo);
                    break;
                case "FUNCION_PROCED":
                    ejecutarGuardarFuncion_Porced(nodo);
                    break;
                case "CUERPO":
                    break;
                default: break;
            }
        }

        public void ejecutarSentenciasDentro(ParseTreeNode nodo)
        {
            switch (nodo.Term.Name)
            {
                case "DECLARAR":
                    ejecutarDECLARAR(nodo);
                    break;
                case "DECLARAR_ASIGNAR":
                    ejecutarDECLARAR_ASIGNAR(nodo);
                    break;
                case "ASIGNAR":
                    ejecutarASIGNAR(nodo);
                    break;
                case "LLAMAR":
                    ejecutarLlamar(nodo);
                    break;
                default: break;
            }
        }

        private void ejecutarPrincipal(ParseTreeNode nodo) {
            foreach (var item in nodo.ChildNodes[0].ChildNodes)
            {
            ejecutarSentenciasDentro(item);
            }
        }

        private void ejecutarGuardarFuncion_Porced(ParseTreeNode nodo) {//-----> se agregan los metodo encontrados a diccionario de metodos, sin ejecutar contenido
            Metodo nuevo_metodo = new Metodo(nodo.ChildNodes[2].Token.Text, nodo);
            lista_metodos.Add(nodo.ChildNodes[2].Token.Text, nuevo_metodo);
        }

        private void ejecutarFuncion_Procedimiento(ParseTreeNode nodo)
        {
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //Agregar funcion como variable

            Variable nueva_variable = new Variable(nodo.ChildNodes[2].Token.Text, 6);nueva_variable.atributo = "Metodo";
            guardarVariable(nueva_variable, nodo.ChildNodes[2].Token.Location.Line, nodo.ChildNodes[2].Token.Location.Line,"Metodo");

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

            aumentarAmbito(nodo.ChildNodes[2].Token.Text);

            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {

            }

            foreach (var item in nodo.ChildNodes[4].ChildNodes)
            {
                ejecutarSentenciasDentro(item);
            }
            switch (nodo.Term.Name) 
            {
                case "SENTENCIASDENTRO":
                    break;
                case "FUNCION_PROCED":
                    break;
                case "CUERPO":
                    break;
                default: break;
            }
            
            disminuirAmbito();
        }

        private void ejecutarFuncion_ProcedimientoPrimero(ParseTreeNode nodo)
        {
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //Agregar funcion como variable

            Variable nueva_variable = new Variable(nodo.ChildNodes[2].Token.Text, 6); nueva_variable.atributo = "Metodo";
            guardarVariable(nueva_variable, nodo.ChildNodes[2].Token.Location.Line, nodo.ChildNodes[2].Token.Location.Line, "Metodo");

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

            aumentarAmbito(nodo.ChildNodes[2].Token.Text);

            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {

            }

            foreach (var item in nodo.ChildNodes[4].ChildNodes)
            {
                ejecutarSentenciasDentro(item);
            }
            switch (nodo.Term.Name)
            {
                case "SENTENCIASDENTRO":
                    break;
                case "FUNCION_PROCED":
                    break;
                case "CUERPO":
                    break;
                default: break;
            }
            disminuirAmbito();
        }

        public void ejecutarLlamar(ParseTreeNode nodo) {
            ultimaColumna = nodo.ChildNodes[0].Token.Location.Column;
            ultimaLinea = nodo.ChildNodes[0].Token.Location.Line;
            if (nodo.ChildNodes[1].ChildNodes.Count==0)//------> LLamada a metodo sin atributos 
            {
                Metodo MetodoLlamado = getVMetodo(nodo.ChildNodes[0].Token.Text);
                if (MetodoLlamado!=null)
                {

                        LinkedList<Ambito>  listaListaaux = new LinkedList<Ambito>(); //-----> aux -----> ambito nuevo
                        Ambito listaActualaux = new Ambito("Nombre xxx por cambiar");
                        listaListaaux.AddLast(listaActualaux);

                    LinkedList<Ambito> listaListaaux2;                                  //-----> aux2 -----> ambito base
                    Ambito listaActualaux2;
                    listaListaaux2 = listaLista;
                    listaActualaux2 = listaActual;

                    listaLista = listaListaaux;
                    listaActual = listaActualaux;


                    ejecutarFuncion_Procedimiento(MetodoLlamado.Nodo);

                    listaLista = listaListaaux2;
                    listaActual = listaActualaux2;




                }
                else
                {
                    lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "El metodo que se ha llamdo no existe"));
                }
            }
            else//------> LLamada a metodo con atributos
            {

            }
        }

        public void ejecutarDECLARAR(ParseTreeNode nodo)//-----> Exclusivo Para Variables
        {
            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {
                String nombre = item.Token.Text;
                Variable nueva_variable = new Variable(nombre, irPoTipo(nodo.ChildNodes[2]));nueva_variable.atributo = "Atributo";
                guardarVariable(nueva_variable, item.Token.Location.Line, item.Token.Location.Column,"Atributo");
            }
        }

        public void ejecutarAgregarParametro(ParseTreeNode nodo)//-----> Exclusivo
        {
            String nombre = nodo.Token.Text;

            
        }

        public void ejecutarDECLARAR_ASIGNAR(ParseTreeNode nodo)//-----> Exclusivo Para  DECLARAR Y ASIGNAR VARIABLES
        {
            foreach (var item in nodo.ChildNodes[3].ChildNodes)
            {
                String nombre = item.Token.Text;
                Object Resultado = evaluarEXPRESION(nodo.ChildNodes[4]);
                Console.WriteLine(Resultado.ToString());
                Variable nueva_variable = new Variable(nombre,Resultado, irPoTipo(nodo.ChildNodes[2])); nueva_variable.atributo = "Atributo";
                guardarVariable(nueva_variable, item.Token.Location.Line, item.Token.Location.Column, "Atributo");
                //getVariable(nombre).valor = Resultado;
            }
        }

        public void ejecutarASIGNAR(ParseTreeNode nodo)//-----> Exclusivo Para 
        {
            String nombre = nodo.ChildNodes[0].Token.Text;
            getVariable(nombre).valor = evaluarEXPRESION(nodo.ChildNodes[1]);
            Console.WriteLine(evaluarEXPRESION(nodo.ChildNodes[1]));
        }

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region "Evaluar Expresion"

        private Object evaluarEXPRESION(ParseTreeNode nodo)
        {
            //---------------------> Si tiene 3 hijos
            #region "3 hijos"
            if (nodo.ChildNodes.Count == 3)
            {
                String operador = nodo.ChildNodes[1].Term.Name;
                switch (operador)
                {
                    case "||": return evaluarOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "!||": return evaluarNOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "&&": return evaluarAND(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "!&&": return evaluarNAND(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "&|": return evaluarXOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "==": return evaluarIGUAL(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "!=": return evaluarDIFERENTE(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case ">=": return evaluarMAYORIGUAL(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case ">": return evaluarMAYOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "<=": return evaluarMENORIGUAL(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "<": return evaluarMENOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "+": return evaluarMAS(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "-": return evaluarMENOS(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "*": return evaluarPOR(nodo.ChildNodes[0], nodo.ChildNodes[2]);
                    case "/": return evaluarDIVIDIR(nodo.ChildNodes[0], nodo.ChildNodes[2]); 
                     default: break;
                }
            }
            #endregion

            //---------------------> Si tiene 2 hijos
            #region "2 hijos"
            if (nodo.ChildNodes.Count == 2)
            {
                String termino2 = nodo.ChildNodes[0].Term.Name;
                switch (termino2)
                {
                    case "!": return evaluarNOT(nodo.ChildNodes[1]);
                    case "id": return evaluarID(nodo.ChildNodes[0]);
                    default: break;
                }

                //if (nodo.ChildNodes[0].Term.Name.Equals("!"))
                //    return evaluarNOT(nodo.ChildNodes[1]);
            }
            


            #endregion

            //---------------------> Si tiene 1 hijo
            #region "1 hijo"
            if (nodo.ChildNodes.Count == 1)
            {

                String termino = nodo.ChildNodes[0].Term.Name;
                try
                {
                ultimaLinea = nodo.ChildNodes[0].Token.Location.Line;
                ultimaColumna = nodo.ChildNodes[0].Token.Location.Column;

                }
                catch (Exception)
                {

                    
                }

                switch (termino)
                {

                    case "EXP": return evaluarEXPRESION(nodo.ChildNodes[0]);
                    case "id":  return evaluarID(nodo.ChildNodes[0]);

                    case "num": return evaluarNUMERO(nodo.ChildNodes[0]);
                    case "datostring": return nodo.ChildNodes[0].Token.Text.Replace("\"", "");
                    case "datochar": return nodo.ChildNodes[0].Token.Text.Replace("'", "");
                    case "falso": return false;
                    case "verdadero": return true;
                    default: break;
                }
            }
            #endregion

            //---------------------> Retorno por defecto
            return 1;
        }
        private Object evaluarOR(ParseTreeNode izq, ParseTreeNode der)
        {
            try
            {
                Boolean val1, val2;
                val1 = Convert.ToBoolean(evaluarEXPRESION(izq));
                val2 = Convert.ToBoolean(evaluarEXPRESION(der));
                return val1 || val2;
            }
            catch (Exception)
            {
                lista_errores.Add(new Error(ultimaLinea,ultimaColumna,"Semantico", "Para Utilizar || se esperaba un valor boolean"));
                return false;
            }
        }
        private Object evaluarNOR(ParseTreeNode izq, ParseTreeNode der)
        {
            try
            {
                Boolean val1, val2;
                val1 = Convert.ToBoolean(evaluarEXPRESION(izq));
                val2 = Convert.ToBoolean(evaluarEXPRESION(der));
                return !(val1 || val2);
            }
            catch (Exception)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Para Utilizar !|| se esperaba un valor boolean"));
                return false;
            }
        }
        private Object evaluarAND(ParseTreeNode izq, ParseTreeNode der)
        {
            try
            {
                Boolean val1, val2;
                val1 = Convert.ToBoolean(evaluarEXPRESION(izq));
                val2 = Convert.ToBoolean(evaluarEXPRESION(der));
                return val1 && val2;
            }
            catch (Exception)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Para Utilizar && se esperaba un valor boolean"));
                return false;
            }
        }
        private Object evaluarNAND(ParseTreeNode izq, ParseTreeNode der)
        {
            try
            {
                Boolean val1, val2;
                val1 = Convert.ToBoolean(evaluarEXPRESION(izq));
                val2 = Convert.ToBoolean(evaluarEXPRESION(der));
                return !(val1 && val2);
            }
            catch (Exception)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Para Utilizar !&& se esperaba un valor boolean"));
                return false;
            }
        }
        private Object evaluarXOR(ParseTreeNode izq, ParseTreeNode der)
        {
            try
            {
                Boolean val1, val2;
                val1 = Convert.ToBoolean(evaluarEXPRESION(izq));
                val2 = Convert.ToBoolean(evaluarEXPRESION(der));
                return (!val1 && val2) || (val1 && !val2);
            }
            catch (Exception)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Para Utilizar && se esperaba un valor boolean"));
                return false;
            }
        }
        private Object evaluarIGUAL(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            return val1.Equals(val2);
        }
        private Object evaluarDIFERENTE(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            return !val1.Equals(val2);
        }
        private Object evaluarMAYORIGUAL(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 1)
            {
                if (tipo2 == 1)
                    return Convert.ToInt32(val1) >= Convert.ToInt32(val2);
                else if (tipo2 == 2)
                    return Convert.ToDouble(val1) >= Convert.ToDouble(val2); ;
            }
            else if (tipo1 == 2)
            {
                if (tipo2 == 1 || tipo2 == 2)
                    return Convert.ToDouble(val1) >= Convert.ToDouble(val2);
            }
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", ">= operadores no comparables: " + tipo1 + " >= " + tipo2));
            return false;
        }
        private Object evaluarMAYOR(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 1)
            {
                if (tipo2 == 1)
                    return Convert.ToInt32(val1) > Convert.ToInt32(val2);
                else if (tipo2 == 2)
                    return Convert.ToDouble(val1) > Convert.ToDouble(val2); ;
            }
            else if (tipo1 == 2)
            {
                if (tipo2 == 1 || tipo2 == 2)
                    return Convert.ToDouble(val1) > Convert.ToDouble(val2);
            }
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "> operadores no comparables: " + tipo1 + " > " + tipo2));
            return false;
        }
        private Object evaluarMENORIGUAL(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 1)
            {
                if (tipo2 == 1)
                    return Convert.ToInt32(val1) <= Convert.ToInt32(val2);
                else if (tipo2 == 2)
                    return Convert.ToDouble(val1) <= Convert.ToDouble(val2); ;
            }
            else if (tipo1 == 2)
            {
                if (tipo2 == 1 || tipo2 == 2)
                    return Convert.ToDouble(val1) <= Convert.ToDouble(val2);
            }
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "<= operadores no comparables: " + tipo1 + " <= " + tipo2));
            return false;
        }
        private Object evaluarMENOR(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 1)
            {
                if (tipo2 == 1)
                    return Convert.ToInt32(val1) < Convert.ToInt32(val2);
                else if (tipo2 == 2)
                    return Convert.ToDouble(val1) < Convert.ToDouble(val2); ;
            }
            else if (tipo1 == 2)
            {
                if (tipo2 == 1 || tipo2 == 2)
                    return Convert.ToDouble(val1) < Convert.ToDouble(val2);
            }
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "< operadores no comparables: " + tipo1 + " < " + tipo2));
            return false;
        }
        private Object evaluarMAS(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 2 || tipo2 == 2)
                return Convert.ToDouble(val1) + Convert.ToDouble(val2);
            else if (tipo1 == 1 && tipo2 == 1)
                return Convert.ToInt32(val1) + Convert.ToInt32(val2);
            else if (tipo1 == 3 || tipo2 == 3)
                return val1.ToString() + val2.ToString();
            else if (tipo1 == 4 && tipo2 == 4)
                return val1.ToString() + val2.ToString();
            
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "+ solo recibe valores numericos como parametros"));
            return 1;
        }
        private Object evaluarMENOS(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 2 || tipo2 == 2)
                return Convert.ToDouble(val1) - Convert.ToDouble(val2);
            else if (tipo1 == 1 && tipo2 == 1)
                return Convert.ToInt32(val1) - Convert.ToInt32(val2);
            
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "- solo recibe valores numericos como parametros"));
            return 1;
        }
        private Object evaluarPOR(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 2 || tipo2 == 2)
                return Convert.ToDouble(val1) * Convert.ToDouble(val2);
            else if (tipo1 == 1 && tipo2 == 1)
                return Convert.ToInt32(val1) * Convert.ToInt32(val2);
            
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "* solo recibe valores numericos como parametros"));
            return 1;
        }
        private Object evaluarDIVIDIR(ParseTreeNode izq, ParseTreeNode der)
        {
            Object val1 = evaluarEXPRESION(izq);
            Object val2 = evaluarEXPRESION(der);
            int tipo1 = tipoObjecto(val1);
            int tipo2 = tipoObjecto(val2);

            if (tipo1 == 2 || tipo2 == 2)
                return Convert.ToDouble(val1) / Convert.ToDouble(val2);
            else if (tipo1 == 1 && tipo2 == 1)
                return Convert.ToInt32(val1) / Convert.ToInt32(val2);

            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "/ solo recibe valores numericos como parametros"));
            return 1;
        }
        private Object evaluarNOT(ParseTreeNode der)
        {
            Boolean val;
            if (Boolean.TryParse(evaluarEXPRESION(der).ToString(), out val))
                return !val;
            lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "! solo recibe booleano como parametro"));
            return false;
        }
        private Object evaluarID(ParseTreeNode nodo)
        {
            Variable vari = getVariable(nodo.Token.Text);
            if (vari == null)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Variable " + nodo.Token.Text + " no existe"));
                return 1;
            }
            if (vari.valor == null)
            {
                lista_errores.Add(new Error(ultimaLinea, ultimaColumna, "Semantico", "Variable " + nodo.Token.Text + " no tiene valor"));
                return 1;
            }
            return vari.valor;
        }
        private Object evaluarNUMERO(ParseTreeNode nodo)
        {
            if (nodo.Token.Text.Contains("."))
                return Convert.ToDouble(nodo.Token.Text);
            else
                return Convert.ToInt32(nodo.Token.Text);
        }

        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****

        #region extras, crear html, tomar tipo de variable

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

        public string darTipo(int _tipo) {
            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //-------> entero == 1
            //-------> doble == 2
            //-------> boolena == 3
            //-------> cadena == 4
            //-------> caracter ==5
            string nuevo;
            switch (_tipo)
            {
                case 1:
                    nuevo = "Entero";
                    break;
                case 2:
                    nuevo = "Doble";
                    break;
                case 3:
                    nuevo = "Boolean";
                    break;
                case 4:
                    nuevo = "Cadena";
                    break;
                case 5:
                    nuevo = "Caracter";
                    break;
                case 6:
                    nuevo = "Metodo";
                    break;
                default:
                    nuevo = "Desconocido";
                    break;
            }
            return nuevo;
        }

        private int tipoObjecto(Object var)
        {
            int reto = 0;
            if (var.GetType() == Type.GetType("System.Int32")) { reto = 1; }
            else if (var.GetType() == Type.GetType("System.Double")) { reto = 2; }
            else if (var.GetType() == Type.GetType("System.String")) { reto = 3; }
            else if (var.GetType() == Type.GetType("System.Char")) { reto = 4; }
            else if (var.GetType() == Type.GetType("System.Boolean")) { reto = 5; }
            return reto;
        }

        public void GenerarTablaHLML(List<Error> lista)
        {
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

        public void GenerarTablaDeSimbolosHtml(List<Simbolo> lista)
        {
            string datos = "<html> \n";
            datos += "<body> \n";
            datos += "<center><h2> Tabla de Simbolos </h2></center>";
            datos += "<center>";
            datos += "<table border = 4>";
            datos += "<tr>";
            datos += "<td><center><b>" + "Id" + "</b></center></td>";
            datos += "<td><center><b>" + "Tipo" + "</b></center></td>";
            datos += "<td><center><b>" + "Rol" + "</b></center></td>";
            datos += "<td><center><b>" + "Ambito" + "</b></center></td>";
            datos += "</tr>";
            foreach (var item in lista)
            {
                datos += "<tr>";
                datos += "<td><center><b>" + item.Id + "</b></center></td>";
                datos += "<td><center><b>" + item.Tipo + "</b></center></td>";
                datos += "<td><center><b>" + item.Rol + "</b></center></td>";
                datos += "<td><center><b>" + item.Ambito + "</b></center></td>";
                datos += "</tr>";
            }
            datos += "</table>";
            datos += "</center>";
            datos += "</html>";

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
            //Crear html, se abre desde herramientas
            String desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            System.IO.File.WriteAllText(desktop + "\\Files\\ht2.html", datos);

            //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****
        }

        #endregion

        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
        //V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****V*****v*****v*****v*****v*****
    }
}
