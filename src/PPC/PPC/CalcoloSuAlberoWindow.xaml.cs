using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
    /// Interaction logic for CalcoloSuAlberoWindow.xaml
    /// </summary>
    public partial class CalcoloSuAlberoWindow : Window
    {
        private TreeHandler[] treeHandler;
        private ClientClass client;
        private bool isSelected;
        private String albero;
        private TreeHandler selectedTree;
        private List<String> attrNodi;
        private List<String> attrArchi;
        private int nodIniz;
        private int nodFin;
        private String nomeStartEnd;
        private String nodeString;
        private String edgeString;
  
        public CalcoloSuAlberoWindow(ClientClass client)
        {
            
            InitializeComponent();
            this.client = client;
            
            this.isSelected = false;
            this.albero = "";
            NomAttrLabel.Content = "";
            
            this.nodeString = "";
            this.edgeString = "";

            String treeParameters = client.read();
            
            if (!treeParameters.Equals("Vuoto"))
            {

                /*-4.1- I parametri arrivano nel formato seguente "NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/

                this.treeHandler = fillTreeHandl(treeParameters);
            }
            else
            {
                this.treeHandler = new TreeHandler[0];
            }

      
            foreach (TreeHandler tree in treeHandler)
            {

                SelAlbCalcomboBox.Items.Add(tree.nome);
            }

            
           
        }

        private void SelAlbCalcomboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.isSelected = true;
            albero = SelAlbCalcomboBox.SelectedItem.ToString();
            NomAttrLabel.Content = "";
            AttrArchiListbox.Items.Clear();
            AttrNodiListBox.Items.Clear();
            this.attrArchi = new List<string>();
            this.attrNodi = new List<string>();


            foreach (TreeHandler selTree in treeHandler)
            {

                if (selTree.nome.Equals(albero))
                {
                    selectedTree = selTree;
                    break;
                }

            }

            foreach(KeyValuePair<String, String> vertexAttr in selectedTree.vertexAttrList)
            {
                if(vertexAttr.Value.Equals("numeric"))
                {
                    AttrNodiListBox.Items.Add(vertexAttr.Key);
                }
            }


            foreach (KeyValuePair<String, String> edgeAttr in selectedTree.edgeAttrList)
            {
                if (edgeAttr.Value.Equals("numeric"))
                {
                    AttrArchiListbox.Items.Add(edgeAttr.Key);
                }
            }
        }

      

        

        private void AttrNodiListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AttrNodiListBox.Items.Count != 0)
            {
                NomAttrLabel.Content = AttrNodiListBox.SelectedItem.ToString() + "(Nodo)";
            }
        }

        private void AttrArchiListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AttrArchiListbox.Items.Count != 0)
            {
                NomAttrLabel.Content = AttrArchiListbox.SelectedItem.ToString() + "(Arco)";
            }
        }


        private void AggiungiButton_Click(object sender, RoutedEventArgs e)
        {
            if (NomAttrLabel.Content.ToString().Equals(""))
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Selezionare un attributo");
                error.ShowDialog();
            }
            else
            {

               
                if (NomAttrLabel.Content.ToString().Contains("Arco"))
                {
                    attrArchi.Add(NomAttrLabel.Content.ToString().Split('(')[0]);
                }
                else
                {
                    attrNodi.Add(NomAttrLabel.Content.ToString().Split('(')[0]);
                }
            }

        }

        private void CalcoloButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSelected || !int.TryParse(NodInizioListBox.Text, out nodIniz) || !int.TryParse(NodFineListBox.Text, out nodFin) || attrArchi.Count == 0 && attrNodi.Count == 0)
            {
                
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Controllare i parametri");
                error.ShowDialog();

            }
            else
            {
                /*nomeAlbero$NodoInizio$NodoFine*/
                this.nomeStartEnd += albero + "$" + NodInizioListBox.Text + "$" + NodFineListBox.Text;
                client.write(nomeStartEnd);
                /*nomeAttrVertex1$nomeAttrVertex2$etc*/
                this.nodeString = parseAttrDict(attrNodi);
                client.write(nodeString);
                /*nomeAttrEdge1$nomeAttrEdge2$etc*/
                this.edgeString = parseAttrDict(attrArchi);
                client.write(edgeString);
                    
                ParametersErrorWindow result = new ParametersErrorWindow(true,"Il risultato è: " + client.read());
                result.ShowDialog();
                this.Close();
            }
        }

        private String parseAttrDict(List<String> list)
        {
            String result = "";
            int count = 0;
            foreach (String elements in list)
            {
                result += elements;
                if (count != list.Count - 1)
                {
                    result += "$";
                }
            }
            return result;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.write("Return");
        }

        private TreeHandler[] fillTreeHandl(String treeParameters)
        {/*NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/
            TreeHandler[] treeHandler;
            String[] trees = treeParameters.Split('$');
            treeHandler = new TreeHandler[trees.Length];

            for (int i = 0; i < trees.Length; i++)
            {
                Dictionary<String, String> vertex = new Dictionary<String, String>();
                Dictionary<String, String> edge = new Dictionary<String, String>();

                String[] trees_aux = trees[i].Split('#');
                String[] vertex_aux = trees_aux[1].Split(':');
                String[] edge_aux = trees_aux[2].Split(':');
                String[] nomeAttrTipo = new String[2];
                foreach (String aux in vertex_aux)
                {
                    nomeAttrTipo = aux.Split('?');
                    vertex.Add(nomeAttrTipo[0], nomeAttrTipo[1]);
                }
                foreach (String aux in edge_aux)
                {
                    nomeAttrTipo = aux.Split('?');
                    edge.Add(nomeAttrTipo[0], nomeAttrTipo[1]);
                }

                treeHandler[i] = new TreeHandler(trees_aux[0], vertex, edge);
            }
            return treeHandler;
        }

        private void HelpModButton_Click(object sender, RoutedEventArgs e)
        {
            String helptext = "Questa finestra serve ad effettuare il calcolo sugli attributi " + '\n' +
                              "per i nodi e gli archi compresi in un determinato path." + '\n' +
                              "Il calcolo che viene effettuato è una somma degli attributi selezionati. " + '\n' +
                              "Dal menù a tendina “Seleziona Albero” specifichiamo l'albero su cui vogliamo " + '\n' +
                              "effettuare il calcolo. Nelle  form “Nodo Iniziale” e “Nodo Finale” l'utente " + '\n' +
                              "deve specificare un intero che che corrisponde agli ID dei nodi. " + '\n' +
                              "Se l'utente inserisce il numero 1 nella form “Nodo Iniziale” e 3 nella form “Nodo Finale” " + '\n' +
                              "verrà effettuata la  somma gli attributi dal nodo 1 al nodo 3." + '\n' +
                              "Gli id dei nodi sono assegnati con una ricerca in profondità (depth-first search)." + '\n' +
                              "Vengono popolate a run-time le sezioni Archi e Nodi. " + '\n' +
                              "In queste sezioni sono contenuti gli attributi, l'utente può selezionarli" + '\n' +
                              "e aggiungerli al calcolo attraverso il bottone “Aggiungi al calcolo”. ";


            HelpWindow help = new HelpWindow("Calcolo su Albero", helptext);
            help.ShowDialog();
        }
    }
}
