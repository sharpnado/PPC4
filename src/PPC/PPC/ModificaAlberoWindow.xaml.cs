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
    /// Interaction logic for ModificaAlberoWindow.xaml
    /// </summary>
    public partial class ModificaAlberoWindow : Window
    {
        private ClientClass client;
        private TreeHandler[] treeHandler;
        private Dictionary<String,String> newNodeAttrList;
        private Dictionary<String,String> newEdgeAttrList;
        private TreeHandler selectedTree = null;
        private String albero;
        private String nomeStartEnd;
        private String nodeString;
        private String edgeString;
        private bool isSelected;
        private int nodIniz, nodFin;
        private String type;
        private int num;


        public ModificaAlberoWindow(ClientClass client)
        {
            InitializeComponent();
            this.client = client;
            
            this.albero = "";
            this.nomeStartEnd = "";
            this.nodeString = "";
            this.edgeString = "";
            this.NomAttrLabel.Content = "";
            this.isSelected = false;
            this.type = "";

            

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
                SelAlbModcomboBox.Items.Add(tree.nome);

            }
        }



        private void SelAlbModcomboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isSelected = true;
            albero = SelAlbModcomboBox.SelectedItem.ToString();
            NomAttrLabel.Content = "";
            type = "";
            ArchiListBox.Items.Clear();
            NodiListBox.Items.Clear();
            this.newNodeAttrList = new Dictionary<String, String>();
            this.newEdgeAttrList = new Dictionary<String, String>();


            foreach (TreeHandler selTree in treeHandler)
            {

                if (selTree.nome.Equals(albero))
                {
                    selectedTree = selTree;
                    break;
                }

            }

            foreach (KeyValuePair<String, String> vertexAttr in selectedTree.vertexAttrList)
            {
                NodiListBox.Items.Add(vertexAttr.Key);
            }


            foreach (KeyValuePair<String, String> edgeAttr in selectedTree.edgeAttrList)
            {
                ArchiListBox.Items.Add(edgeAttr.Key);

            }
        }

        private void NodiListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodiListBox.Items.Count != 0)
            {
                this.type = getType(NodiListBox.SelectedItem.ToString(), "nodo");
                NomAttrLabel.Content = NodiListBox.SelectedItem.ToString() + "[" + this.type + "]" + "\n" + " (Nodo)";
            }
            NValTextBox.Text = "";
        }

        private void ArchiListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArchiListBox.Items.Count != 0)
            {
                this.type = getType(ArchiListBox.SelectedItem.ToString(), "arco");
                NomAttrLabel.Content = ArchiListBox.SelectedItem.ToString() + "[" + this.type + "]" + "\n"+ " (Arco)";
            }
            NValTextBox.Text = "";
        }

        private void ConfeNewValButton_Click(object sender, RoutedEventArgs e)
        {
            if (NomAttrLabel.Content.ToString().Equals(""))
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Selezionare un attributo");
                error.ShowDialog();
            }
            else if (type.Equals("numeric") && (!int.TryParse(NValTextBox.Text, out num)))
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Attributo di tipo 'numeric'");
                error.ShowDialog();
            }
            else
            {

                /*Dopo che ho inserito il nuovo valore aggiungo la coppia Nome:val al dizionario*/
                if (NomAttrLabel.Content.ToString().Contains("Arco"))
                {
                    newEdgeAttrList.Add(NomAttrLabel.Content.ToString().Split('[')[0], NValTextBox.Text);
                }
                else
                {
                    newNodeAttrList.Add(NomAttrLabel.Content.ToString().Split('[')[0], NValTextBox.Text);
                }
            }
            
        }

        private void ConfermaModButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSelected || !int.TryParse(NodInitListBox.Text, out nodIniz) || !int.TryParse(NodFinListBox.Text, out nodFin) || newEdgeAttrList.Count == 0 && newNodeAttrList.Count == 0)
            {
                /* int edit(String graphName,int startNode,int endNode, Dictionary<String, String> newNodeAttrList, Dictionary<String, String> newEdgeAttrList, DatabaseInterface dbConnection);*/
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Controllare i parametri");
                error.ShowDialog();
           
            }
            else
            {
                /*nomeAlbero$NodoInizio$NodoFine*/
                this.nomeStartEnd += albero + "$" + NodInitListBox.Text + "$" + NodFinListBox.Text;
                client.write(nomeStartEnd);
                /*nomeAttrVertex1:newVal1$nomeAttrVertex2:newVal2$etc*/
                this.nodeString = parseAttrDict(newNodeAttrList);
                client.write(nodeString);
                /*nomeAttrEdge1:newVal1$nomeAttrEdge2:newVal2$etc*/
                this.edgeString = parseAttrDict(newEdgeAttrList);
                client.write(edgeString);
                if (client.read().Equals("1"))
                {
                    ParametersErrorWindow result = new ParametersErrorWindow(false, "Errore nella modifica");
                    result.ShowDialog();
                    this.Close();
                }
                else
                {
                    ParametersErrorWindow result = new ParametersErrorWindow(true, "Modifica avvenuta con successo");
                    result.ShowDialog();
                    this.Close();
                }
            }

            
        }

        private String parseAttrDict(Dictionary<String, String> dict)
        {
            String result = "";
            int count = 0;
            foreach(KeyValuePair<String, String> elements in dict)
            {
                result += elements.Key + ":" + elements.Value;
                if (count != dict.Count - 1)
                {
                    result += "$";
                }
            }

            return result;
        }

        private String getType(String attribute, String arcoOrNodo)
        {
           String type = "";
           if(arcoOrNodo.Equals("nodo"))
           {
            type = selectedTree.vertexAttrList[attribute];
           }
           else
           {
            type = selectedTree.edgeAttrList[attribute];
           }
           return type;
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
            String helpText = "Questa finestra serve a modificare i valori degli attributi " + '\n' +
                              "per i nodi e gli archi compresi in un determinato path. " + '\n' +
                              "(Nodo Iniziale – Nodo Finale) che specifichiamo nella finestra. " + '\n' +
                              "Dal menù a tendina “Seleziona Albero” possiamo selezionare uno " + '\n' +
                              "tra tutti gli attributi presenti nel Database.Nelle form " + '\n' +
                              "“Nodo Iniziale” - “Nodo Finale” l'utente dovrà inserire " + '\n' +
                              "degli interi che corrispondono agli ID dei nodi.  Se l'utente " + '\n' +
                              "inserisce il numero 1 nella form “Nodo Iniziale” e 3 nella form " + '\n' +
                              "“Nodo Finale”  verranno modificati gli attributi dal nodo 1 al nodo 3. " + '\n' +
                              "Gli id dei nodi sono assegnati con una ricerca in profondità " + '\n' +
                              "(depth-first search).Appena selezionato l'albero vengono popolate " + '\n' +
                              "a run-time le sezione Archi e Nodi con gli attributi corrispondenti " + '\n' +
                              "agli archi e a i nodi. Da queste sezioni possiamo selezionare " + '\n' +
                              "un attributo per assegnargli un nuovo valore in base al dominio " + '\n' +
                              "specificato dal testo sovrastante. ";

            HelpWindow help = new HelpWindow(ModAlbMainLabel.Text, helpText);
            help.ShowDialog();
        }


    }
} 
            