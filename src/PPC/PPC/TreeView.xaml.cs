using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PPC
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class TreeView : Window
    {
        private TreeHandler[] treeHandler;
        private Dictionary<String, String> attrDef;
        private ClientClass client;
       

        
       
        public TreeView(TreeHandler[] treeHandler, Dictionary<String, String> attrDef,  ClientClass client)
        {
            InitializeComponent();
            this.treeHandler = treeHandler;
            this.attrDef = attrDef;
            this.client = client;
        }

       
        private void buttImpFile_Click(object sender, RoutedEventArgs e)
        {

            if(checkOp("Importa"))
            {
                ImportaFileWindow importaFileWindow = new ImportaFileWindow(client);
                importaFileWindow.ShowDialog();
            }
            else
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Errore lato server..");
                err.ShowDialog();
            }
        }

        private void buttEspFile_Click(object sender, RoutedEventArgs e)
        {
            if (checkOp("Esporta"))
            {
                EsportaFileWindow esportaFileWindow = new EsportaFileWindow(client,treeHandler);
                esportaFileWindow.ShowDialog();
            }
            else
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Errore lato server..");
                err.ShowDialog();
            }
           
        }

        private void buttCreaAlb_Click(object sender, RoutedEventArgs e)
        {

            if (checkOp("Crea"))
            {
              CreaAlberoWindow creaAlberoWindow = new CreaAlberoWindow(treeHandler, attrDef, client);
              creaAlberoWindow.ShowDialog();
                
            }
            else
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Errore lato server..");
                err.ShowDialog();
            }
        }

        private void buttModAlb_Click(object sender, RoutedEventArgs e)
        {
            if (checkOp("Modifica"))
            {

               ModificaAlberoWindow modificaAlberoWindow = new ModificaAlberoWindow(client);
               modificaAlberoWindow.ShowDialog();
            }
            else
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Errore lato server..");
                err.ShowDialog();
            }
            
        }

        private void buttCalcAlb_Click(object sender, RoutedEventArgs e)
        {
            if (checkOp("Calcola"))
            {
                CalcoloSuAlberoWindow calcoloSuAlberoWindow = new CalcoloSuAlberoWindow(client);
                calcoloSuAlberoWindow.ShowDialog();
            }
            else
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Errore lato server..");
                err.ShowDialog();
            }
            
        }


        private bool checkOp(String operation)
        {
            client.write(operation);
            if (client.read().Equals(operation))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TreeWindow_Closing(object sender, CancelEventArgs e)
        {
            client.write("Disconnect");
            client.disconnect();
        }

        private void buttAiuto_Click_1(object sender, RoutedEventArgs e)
        {
            String helpText = "In questa finestra avete la possibilità di scegliere quale operazione eseguire " + '\n' +
                                 "cliccando sui bottoni posti sui rami dell'albero.";
            HelpWindow help = new HelpWindow(TreeWindow.Title, helpText);
            help.ShowDialog();
        }

}
}

