using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
namespace Lienzo2D.Clases
{
    class Gramatica : Grammar
    {
        public Gramatica() : base(caseSensitive: true)
        {
            CommentTerminal COMENTARIO_UL = new CommentTerminal("comentario_UL", ">>", "\n", "\r\n");
            CommentTerminal COMENTARIO_ML = new CommentTerminal("comentario_ML", "<-", "->");
            NonGrammarTerminals.Add(COMENTARIO_UL);
            NonGrammarTerminals.Add(COMENTARIO_ML);
            MarkReservedWords("arreglo");
            #region NOTERMINALES
            var LIENZO = new NonTerminal("LIENZO");
            var VISIVILIDAD= new NonTerminal("VISIVILIDAD");
            var EXTENDER = new NonTerminal("EXTENDER");
            var LISTAIDS = new NonTerminal("LISTAIDS");
            var SENTENCIASFUERA= new NonTerminal("SENTENCIASFUERA");
            var SENTENCIASDENTRO= new NonTerminal("SENTENCIASDENTRO");
            var DECLARAR_ASIGNAR= new NonTerminal("DECLARAR_ASIGNAR");
            var ASIGNAR= new NonTerminal("ASIGNAR");
            var ASIGNACION_ARREGLOS= new NonTerminal("ASIGNACION_ARREGLOS");
            var ARREGLO_DATOS= new NonTerminal("ARREGLO_DATOS");
            var DECLARAR_ASIGNAR_ARREGLOS = new NonTerminal("DECLARAR_ASIGNAR_ARREGLOS");
            var LLAMAR= new NonTerminal("LLAMAR");
            var LISTA_EXP= new NonTerminal("LISTA_EXP");
            var CICLOS= new NonTerminal("CICLOS");
            var IF1= new NonTerminal("IF1");
            var PARA= new NonTerminal("PARA");
            var PARA_DATOS= new NonTerminal("PARA_DATOS");
            var MIENTRAS= new NonTerminal("MIENTRAS");
            var HACER_MIENTRAS= new NonTerminal("HACER_MIENTRAS");
            var DECLARAR = new NonTerminal("DECLARAR");
            var DECLARARARREGLOS= new NonTerminal("DECLARARARREGLOS");
            var LISTADIMENSIONES= new NonTerminal("LISTADIMENSIONES");
            var CONSERVAR= new NonTerminal("CONSERVAR");
            var TIPO= new NonTerminal("TIPO");
            var ARREGLO_FUN_PRO= new NonTerminal("ARREGLO_FUN_PRO");
            var FUNCION_PROCED= new NonTerminal("FUNCION_PROCED");
            var PARAMETROS= new NonTerminal("PARAMETROS");
            var PRINCIPAL= new NonTerminal("PRINCIPAL");
            var FPINTAR_P= new NonTerminal("FPINTAR_P");
            var FPINTAR_OR= new NonTerminal("FPINTAR_OR");
            var EXP= new NonTerminal("EXP");
            var LIENZOP = new NonTerminal("LIENZOP");
            var SENTENCIAFUERA = new NonTerminal("SENTENCIAFUERA");
            var SENTENCIADENTRO= new NonTerminal("SENTENCIADENTRO");
            var LISTADIMENSION = new NonTerminal("LISTADIMENSION");
            var PARAMETRO = new NonTerminal("PARAMETRO");
            var ARREGLO_DATOSP = new NonTerminal("ARREGLO_DATOSP");
            var RETORNAR = new NonTerminal("RETORNAR");
            NumberLiteral num = TerminalFactory.CreateCSharpNumber("num");
            IdentifierTerminal id = TerminalFactory.CreateCSharpIdentifier("id");
            var datostring = new StringLiteral("datostring", "\"", StringOptions.AllowsDoubledQuote);
            var datochar = new StringLiteral("datochar", "'", StringOptions.AllowsDoubledQuote);

            #endregion
            this.Root = LIENZOP;
            LIENZO.Rule = MakeStarRule(LIENZOP,LIENZO);
            LIENZO.Rule = VISIVILIDAD + ToTerm("Lienzo") + id + EXTENDER + ToTerm("¿") + SENTENCIASFUERA + ToTerm("?");
            VISIVILIDAD.Rule = MakeStarRule(VISIVILIDAD,ToTerm("publico") | ToTerm("privado"));
            EXTENDER.Rule = MakeStarRule(EXTENDER, ToTerm("extiende") + LISTAIDS);
            LISTAIDS.Rule = MakeListRule(LISTAIDS,ToTerm(","),id);
            SENTENCIASFUERA.Rule = MakeStarRule(SENTENCIASFUERA, SENTENCIAFUERA);
            SENTENCIAFUERA.Rule = PRINCIPAL
                                    | FUNCION_PROCED
                                    | DECLARAR+ToTerm("$")
                                    //| DECLARARARREGLOS+ToTerm("$") fuera
                                    
                                    ;
            SENTENCIASDENTRO.Rule = MakeStarRule(SENTENCIASDENTRO,SENTENCIADENTRO);
            SENTENCIADENTRO.Rule = DECLARAR_ASIGNAR_ARREGLOS + ToTerm("$")
                                    //|DECLARARARREGLOS +ToTerm("$") fuera
                                    | DECLARAR + ToTerm("$")
                                    | ASIGNACION_ARREGLOS +ToTerm("$")
                                    
                                    //|FUNCION_PROCED    fuera
                                    |LLAMAR +ToTerm("$")
                                    |CICLOS
                                    |DECLARAR_ASIGNAR +ToTerm("$")
                                    |ASIGNAR +ToTerm("$")
                                    |RETORNAR + ToTerm("$")
                                    |EXP + ToTerm("--") + ToTerm("$")
                                    | EXP + ToTerm("++") + ToTerm("$")
                                    | FPINTAR_P + ToTerm("$")
                                    |FPINTAR_OR +ToTerm("$")
                                    ;
            CICLOS.Rule = IF1
                            | PARA
                            | MIENTRAS
                            | HACER_MIENTRAS;
            FPINTAR_P.Rule = ToTerm("Pintar_P") + ToTerm("(") + EXP + ToTerm(",") + EXP + ToTerm(",") + EXP + ToTerm(",") + EXP + ToTerm(")");
            FPINTAR_OR.Rule = ToTerm("Pintar_OR") + ToTerm("(") + EXP + ToTerm(",") + EXP + ToTerm(",") + EXP + ToTerm(",") + EXP + ToTerm(",") + EXP  +ToTerm(",") + EXP+ToTerm(")");
            RETORNAR.Rule = ToTerm("retorna")+EXP;
            IF1.Rule = ToTerm("si") + ToTerm("(") + EXP + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?")
                        | ToTerm("si") + ToTerm("(") + EXP + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?") + ToTerm("sino") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?");
            PARA.Rule = ToTerm("para") + ToTerm("(") + PARA_DATOS + ToTerm(";") + EXP + ToTerm(";") + EXP + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?");
            PARA_DATOS.Rule = ASIGNAR
                            | DECLARAR_ASIGNAR;
            MIENTRAS.Rule = ToTerm("mientras") + ToTerm("(") + EXP + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?");
            HACER_MIENTRAS.Rule = ToTerm("hacer") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?") + ToTerm("mientras") + ToTerm("(") + EXP + ToTerm(")");
            ASIGNAR.Rule = id + ToTerm("=") + EXP;
            DECLARAR_ASIGNAR.Rule= CONSERVAR+VISIVILIDAD+ ToTerm("var")+TIPO+LISTAIDS+ToTerm("=")+ EXP;
            LLAMAR.Rule = id + ToTerm("(") + LISTA_EXP+ToTerm(")");
            PRINCIPAL.Rule = ToTerm("Principal") + ToTerm("(") + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO+ToTerm("?");
            DECLARAR.Rule = CONSERVAR + VISIVILIDAD + ToTerm("var") + TIPO + LISTAIDS
                            | CONSERVAR + VISIVILIDAD + ToTerm("var") + TIPO + ToTerm("arreglo") + LISTAIDS + LISTADIMENSIONES
                            ;//|   CONSERVAR + VISIVILIDAD + TIPO + ARREGLO_FUN_PRO + id + ToTerm("(") + PARAMETROS + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?"); ;
            TIPO.Rule = ToTerm("entero")
                        | ToTerm("doble")
                        | ToTerm("boolean")
                        | ToTerm("caracter")
                        | ToTerm("cadena")
                        ;
            CONSERVAR.Rule = MakeStarRule(CONSERVAR,ToTerm("conservar"));
            DECLARAR_ASIGNAR_ARREGLOS.Rule = CONSERVAR + VISIVILIDAD + ToTerm("var") + TIPO + ToTerm("arreglo") + LISTAIDS + LISTADIMENSIONES + ToTerm("=") + ToTerm("{") + ARREGLO_DATOSP + ToTerm("}")
                                    | CONSERVAR + VISIVILIDAD + ToTerm("var") + TIPO + ToTerm("arreglo") + LISTAIDS + LISTADIMENSIONES + ToTerm("=") + ToTerm("{") + LISTA_EXP + ToTerm("}")
                                    | CONSERVAR + VISIVILIDAD + ToTerm("var") + TIPO + ToTerm("arreglo") + LISTAIDS + LISTADIMENSIONES + ToTerm("=") + LLAMAR;
                                    ;
            //DECLARAR_ASIGNAR_ARREGLOS.Rule = CONSERVAR + VISIVILIDAD+ ToTerm("var") + TIPO + ToTerm("arreglo")  + ASIGNACION_ARREGLOS;
            LISTADIMENSIONES.Rule = MakeStarRule(LISTADIMENSIONES,LISTADIMENSION);         //OJO num por exp
            LISTADIMENSION.Rule = ToTerm("[") + EXP + ToTerm("]");
            FUNCION_PROCED.Rule = VISIVILIDAD + TIPO + ARREGLO_FUN_PRO + id + ToTerm("(") + PARAMETROS + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?")
                                | VISIVILIDAD + ARREGLO_FUN_PRO + id + ToTerm("(") + PARAMETROS + ToTerm(")") + ToTerm("¿") + SENTENCIASDENTRO + ToTerm("?");
            PARAMETROS.Rule = MakeStarRule(PARAMETROS, ToTerm(","), PARAMETRO);             
            PARAMETRO.Rule = TIPO + EXP;                                                  //ojo id por exp... duda
            ARREGLO_FUN_PRO.Rule = MakeStarRule(ARREGLO_FUN_PRO,ToTerm("[")+ToTerm("]"));
            ASIGNACION_ARREGLOS.Rule = LISTAIDS + LISTADIMENSIONES + ToTerm("=") + ToTerm("{") + ARREGLO_DATOSP + ToTerm("}")
                                        | LISTAIDS + LISTADIMENSIONES + ToTerm("=") + ToTerm("{") + LISTA_EXP + ToTerm("}");
            ARREGLO_DATOSP.Rule = MakeStarRule(ARREGLO_DATOSP, ToTerm(","),ARREGLO_DATOS);
            ARREGLO_DATOS.Rule = ToTerm("{") + LISTA_EXP + ToTerm("}");
            LISTA_EXP.Rule = MakeStarRule(LISTA_EXP,ToTerm(","),EXP);
            EXP.Rule = EXP + ToTerm("||") + EXP
                    | EXP + ToTerm("&&") + EXP
                    | EXP + ToTerm("!&&") + EXP
                    | EXP + ToTerm("!||") + EXP
                    | EXP + ToTerm("&|") + EXP
                    | EXP + ToTerm("==")+ EXP
                    | EXP + ToTerm("!=")+ EXP
                    | EXP + ToTerm("<")+ EXP
                    | EXP + ToTerm(">")+ EXP
                    | EXP + ToTerm("<=") +EXP
                    | EXP + ToTerm(">=")+ EXP
                    | EXP + ToTerm("+")+ EXP
                    | EXP + ToTerm("-") + EXP
                    | EXP + ToTerm("*")+ EXP
                    | EXP + ToTerm("/")+ EXP
                    | EXP + ToTerm("^") +EXP
                    | ToTerm("!")+EXP
                    | ToTerm("(")+ EXP + ToTerm(")")
                    | EXP + ToTerm("--")
                    | EXP + ToTerm("++")
                    | id
                    | num
                    | datostring
                    | datochar
                    | ToTerm("falso")
                    | ToTerm("verdadero")
                    | id + LISTADIMENSIONES;



          
            SENTENCIADENTRO.ErrorRule = SyntaxError + SENTENCIADENTRO;

            //---------------------> Eliminacion de caracteres, no terminales, sin utilidad

            this.MarkPunctuation("(", ")", ";", ":", "{", "}", "=",",","var","$","Principal");
            this.MarkPunctuation("?", "¿", "Lienzo", ":", "{", "}", "=", "Pintar_P", "Pintar_OR");
            this.MarkTransient(SENTENCIAFUERA,SENTENCIADENTRO);
            //---------------------> Definir Asociatividad
            RegisterOperators(1, Associativity.Left, "||", "!||", "&|");                 //OR
            RegisterOperators(2, Associativity.Left, "&&", "!&&");                 //AND
            RegisterOperators(3, Associativity.Left, "==", "!=");           //IGUAL, DIFERENTE
            RegisterOperators(4, Associativity.Left, ">", "<", ">=", "<="); //MAYORQUES, MENORQUES
            RegisterOperators(5, Associativity.Left, "+", "-");             //MAS, MENOS
            RegisterOperators(6, Associativity.Left, "*", "/");             //POR, DIVIDIR
            RegisterOperators(7, Associativity.Right, "!");                 //NOT
            RegisterOperators(8, Associativity.Left, "^","++","--");                 //NOT

        } // fin del contructor 

        }

}
