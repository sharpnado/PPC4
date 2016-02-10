using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace PPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
/*_________________Metodo che ripulisce tutti i campi della finestra__________________________________________________________*/
        private void ClearStartButton_Click(object sender, RoutedEventArgs e)
        {
            IPStartTextBox.Text = "";
            PortStartTextBox.Text = "";
            IpDBTextBox.Text = "";
            UsernameTextBoxStart.Text = "";
            PasswordBoxStart.Clear();
        }


/*_________________Metodo che si connette al server e al db specificati dai parametri___________________________________________*/ 
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            /*controlla se i parametri sono errati*/
            if (!CheckParameters())
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Parametri errati");
                error.ShowDialog();
            }

            /*Se i parametri sono sintatticamente corretti*/
            else
            {
                /*parsing parametri*/
                IPAddress ip = IPAddress.Parse(IPStartTextBox.Text);
                int porta = Int32.Parse(PortStartTextBox.Text);
            /*______-----1-------Connettersi al Server_________________________*/
                ClientClass clientGui = new ClientClass(ip, porta);
                /*verifica il successo della connessione con il server*/
                if (!clientGui.connect())
                {
                    this.Close();
                    return;
                }
            
            /*______-----2-------Inviare username e password al server_________*/
                String parameters = IpDBTextBox.Text + '$' + UsernameTextBoxStart.Text + '$' + PasswordBoxStart.Password;
                clientGui.write(parameters);
          
            /*______-----3--------Check login database andato a buon fine_______*/
                String checkAuth = clientGui.read();
                /*-3.1- Se l'autenticazione non è andata a buon fine il server ci ritorna "1", altrimenti "0" */
                if (checkAuth.Equals("1"))
                {
                    ParametersErrorWindow errorAuth = new ParametersErrorWindow(false, "Auth DB fallita");
                    errorAuth.ShowDialog();
                    clientGui.disconnect();
                }/*Fine if-autenticazione non andata a buon fine*/
                else
                /*-3.2-Autenticazione andata a buon fine*/
                {
            /*______-----4-------Salvare in una strutturadati i dati dei database ricevuti dal server*/
                    String treeParameters = clientGui.read();
                    TreeHandler[] treeHandler;
                    if (!treeParameters.Equals("Vuoto"))
                    {

                        /*-4.1- I parametri arrivano nel formato seguente "NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/

                        treeHandler = fillTreeHandl(treeParameters);
                    }
                    else
                    {
                        treeHandler = new TreeHandler[0];
                    }
           /*______-----5-------Una volta che il server ci ha inviato la lista degli alberi con gli attributi,viene inviata l'attribute definition */
                    Dictionary<String, String> attrDef = attrDefPars(clientGui.read());
                    
           /*______-----6-------Instanziare finestra TreeView passando al costruttore treeHandler[] e attrDef e client*/
                    TreeView treeView = new TreeView(treeHandler, attrDef, clientGui);
                    treeView.Show();
                    this.Close();
                }/*Fine if-autenticazione andata a buon-fine*/
            }/*Fine if-Se i parametri sono sintatticamente corretti*/
        }/*Fine funzione ConnectButton_Click*/



/*_________________Metodo che verifica la correttezza sintattica dei parametri__________________________________________________*/ 
        private bool CheckParameters()
        {
            IPAddress ip;
            IPAddress dbIP;
            int porta;
            if (IPAddress.TryParse(IPStartTextBox.Text, out ip) && int.TryParse(PortStartTextBox.Text, out porta) && UsernameTextBoxStart.Text != "" && (IpDBTextBox.Text != "" || IpDBTextBox.Text.Equals("local") || IPAddress.TryParse(IpDBTextBox.Text,out dbIP)))
            {
                return true;
            }
            else
                return false;
        }


/*_________________Metodo che, data un stringa contente gli alberi con i relativi attributi, ritorna una struttura dati_____________
 *_________________contente le informazioni ottenute dal parsing della stringa______________________________________________________
* ___formato della stringa: NomeAlbero#attr1Vertex?tipo:attr2Vertex?tipo#attrEdge?tipo:attrEdge2?tipo$NomeAlbero2#etc*/

        private TreeHandler[] fillTreeHandl(String treeParameters)
        {
            TreeHandler[] treeHandler;
            String[] trees = treeParameters.Split('$');
            treeHandler = new TreeHandler[trees.Length];

            for(int i = 0; i < trees.Length; i++)
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

        /*___________________Metodo che converte la stringa (NomeAttr:type$NomeAttr:Type$etc) contenente tutta l'attr definition in dizionario__________*/
        private Dictionary<String, String> attrDefPars(String attrs)
        {
            Dictionary<String, String> attributes = new Dictionary<String, String>();
            String[] multipleAttributes = attrs.Split('$');
            String[] singleAttributes = new String[2];
            foreach( String single in multipleAttributes)
            {
                singleAttributes = single.Split(':');
                attributes.Add(singleAttributes[0], singleAttributes[1]);
            }
            return attributes;
        }


        private void HelpStartButton_Click(object sender, RoutedEventArgs e)
        {
            String helptext = "I campi IP e Porta servono per specificare a quale Engine remoto connettersi. " + '\n' +
                              "Inserire un IP e Porta validi. Le form DB IP ,Username e Password servono " + '\n' +
                              "per l'autenticazione nel database. Il bottone “Clear” serve per cancellare tutti " + '\n' +
                              "i valori inseriti nelle form. Una volta inseriti tutti i campi in modo corretto, " + '\n' +
                              "potrete cliccare sul bottone “Connect” per collegarvi all'engine. ";

            HelpWindow help = new HelpWindow("Main Window", helptext);
            help.ShowDialog();
        }

  }

       
  }

