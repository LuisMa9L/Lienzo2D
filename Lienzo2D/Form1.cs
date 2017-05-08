using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;

namespace Lienzo2D
{
    public partial class Form1 : Form


    {
        
        public StringBuilder errores;
        public Form1()
        {
            InitializeComponent();
        }

        private void verArbolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        

        private void ejecutarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clases.Analisis analisis = new Clases.Analisis();
            analisis.RealizarAnalisis(CuadroEntrada.Text);
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            abrir.Title = "Abrir archivos de texto";
            abrir.Filter = "Archivo de Texto Plano  (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            abrir.ShowDialog();

            if (abrir.FileName.Length == 0)
                return;

            System.IO.StreamReader sr = new System.IO.StreamReader(abrir.FileName, System.Text.Encoding.UTF8);
            //System.IO.StreamReader sr = new System.IO.StreamReader(abrir.FileName, System.Text.Encoding.Default);
            String contenido = sr.ReadToEnd();
            sr.Close();
            CuadroEntrada.Text = contenido;
            
        }
    }
}
