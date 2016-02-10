using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
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
    /// Interaction logic for CreaAlberoWindow.xaml
    /// </summary>
    public partial class CreaAlberoWindow : Window
    {
        private ClientClass client;
        private Dictionary<String, String> attrDef;
        private TreeHandler[] treeHandler;
        private Dictionary<String, String[]> vertexAttrList;
        private Dictionary<String, String[]> edgeAttrList;
        private String type;


        public CreaAlberoWindow(TreeHandler[] treeHandler, Dictionary<String, String> attrDef, ClientClass client)
        {
            InitializeComponent();
            
            this.client = client;
            this.attrDef = attrDef;
            this.treeHandler = treeHandler;
            this.vertexAttrList = new Dictionary<String, String[]>();
            this.edgeAttrList = new Dictionary<String, String[]>();
            this.type = "";

            setVisibile(false);
            setDefStringVisibility(false);
            setRandomNumberVisibility(false);
            foreach (KeyValuePair<String, String> attribute in attrDef)
            {
                AttributeListbox.Items.Add(attribute.Key);
            }
        }

        private void setVisibile(bool visible)
        {
            if (visible)
            {
                AttributeNameLabel.Visibility = Visibility.Visible;
                AttrRuleLabel.Visibility = Visibility.Visible;
                ArcoCheckBox.Visibility = Visibility.Visible;
                NodoCheckBox.Visibility = Visibility.Visible;
                
            }
            else
            {
                AttributeNameLabel.Visibility = Visibility.Collapsed;
                AttrRuleLabel.Visibility = Visibility.Collapsed;
                ArcoCheckBox.Visibility = Visibility.Collapsed;
                NodoCheckBox.Visibility = Visibility.Collapsed;
               
            }
        }

        private void setDefStringVisibility(bool visibile)
        {
            if (visibile)
            {
                DefStringTextBox.Visibility = Visibility.Visible;
                InserireStrLabel.Visibility = Visibility.Visible;
                ConfermaAttrButton.Visibility = Visibility.Visible;
            }
            else
            {
                DefStringTextBox.Visibility = Visibility.Collapsed;
                InserireStrLabel.Visibility = Visibility.Collapsed;
                ConfermaAttrButton.Visibility = Visibility.Collapsed;
            }
        }

        private void setRandomNumberVisibility(bool visibile)
        {
            if (visibile)
            {
                MinNumTextBox.Visibility = Visibility.Visible;
                MaxNumTextBox.Visibility = Visibility.Visible;
                RangeLabel.Visibility = Visibility.Visible;
                ConfermaAttrButton.Visibility = Visibility.Visible;
            }
            else
            {
                MinNumTextBox.Visibility = Visibility.Collapsed;
                MaxNumTextBox.Visibility = Visibility.Collapsed;
                RangeLabel.Visibility = Visibility.Collapsed;
                ConfermaAttrButton.Visibility = Visibility.Collapsed;
            }
        }

        private void AttributeListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            type = attrDef[AttributeListbox.SelectedItem.ToString()];
            AttributeNameLabel.Content = AttributeListbox.SelectedItem.ToString() + "(" + type + ")";
           
            MaxNumTextBox.Text = "";
            MinNumTextBox.Text = "";
            DefStringTextBox.Text = "";

            NodoCheckBox.IsChecked = false;
            ArcoCheckBox.IsChecked = false;
           

            setRandomNumberVisibility(false);
            setDefStringVisibility(false);
            setVisibile(true);

            if(type.Equals("string"))
            {
                setRandomNumberVisibility(false);
                setDefStringVisibility(true);
            }
            else
            {
                setDefStringVisibility(false);
                setRandomNumberVisibility(true);
            }
            
        }

       
        private void ConfermaAttrButton_Click(object sender, RoutedEventArgs e)
        {
            int tmp, temp;
            /*checkbox nodo selezionato*/
            if ((bool)NodoCheckBox.IsChecked)
            {
                /*Default String*/
                if (type.Equals("string"))
                {
                    if (!(DefStringTextBox.Text.Equals("")))
                    {
                        vertexAttrList.Add(AttributeNameLabel.Content.ToString().Split('(')[0], new String[] { "string", DefStringTextBox.Text });
                    }
                    else
                    {
                        ParametersErrorWindow err = new ParametersErrorWindow(false, "Inserire stringa di default ");
                        err.ShowDialog();
                    }

                }
                /*Fine Default String*/

                /*Numero random*/
                else if (type.Equals("numeric"))
                {
                    if (!(MinNumTextBox.Text.Equals("") || MaxNumTextBox.Text.Equals("")))
                    {
                        if (int.TryParse(MinNumTextBox.Text, out tmp) && int.TryParse(MaxNumTextBox.Text, out temp))
                        {
                            vertexAttrList.Add(AttributeNameLabel.Content.ToString().Split('(')[0], new String[] { "numeric", MinNumTextBox.Text + "-" + MaxNumTextBox.Text });
                        }
                        else
                        {
                            ParametersErrorWindow err = new ParametersErrorWindow(false, "Range non valido");
                        }
                    }
                    else
                    {
                        ParametersErrorWindow err = new ParametersErrorWindow(false, "Inserire range");
                        err.ShowDialog();
                    }
                }
                /*Fine Numero random*/
                else
                {
                    ParametersErrorWindow err = new ParametersErrorWindow(false, "Selezionare Attribute Value Generation Rule");
                    err.ShowDialog();
                }
            }/*Fine checkbox nodo selezionato*/

            /*checkbox arco selezionato*/
            if((bool) ArcoCheckBox.IsChecked)
            {
                /*Default String*/
                if (type.Equals("string"))
                {
                    if (!(DefStringTextBox.Text.Equals("")))
                    {
                        edgeAttrList.Add(AttributeNameLabel.Content.ToString().Split('(')[0], new String[] { type, DefStringTextBox.Text });
                    }
                    else
                    {
                        ParametersErrorWindow err = new ParametersErrorWindow(false, "Inserire stringa di default ");
                        err.ShowDialog();
                    }

                }
                /*Fine Default String*/

                /*Numero random*/
                else if (type.Equals("numeric"))
                {
                    if (!(MinNumTextBox.Text.Equals("") || MaxNumTextBox.Text.Equals("")))
                    {
                        if(int.TryParse(MinNumTextBox.Text, out tmp) && int.TryParse(MaxNumTextBox.Text, out temp) )
                        {
                            edgeAttrList.Add(AttributeNameLabel.Content.ToString().Split('(')[0], new String[] { type, MinNumTextBox.Text + "-" + MaxNumTextBox.Text });
                        }
                        else
                        {
                            ParametersErrorWindow err = new ParametersErrorWindow(false, "Range non valido");
                        }
                    }
                    else
                    {
                        ParametersErrorWindow err = new ParametersErrorWindow(false, "Inserire range ");
                        err.ShowDialog();
                    }
                }
                /*Fine Numero random*/
                else
                {
                    ParametersErrorWindow err = new ParametersErrorWindow(false, "Selezionare Attribute Value Generation Rule");
                    err.ShowDialog();
                }
            }

            if (!((bool)ArcoCheckBox.IsChecked || (bool)NodoCheckBox.IsChecked))
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Selezionare nodo o arco");
                error.ShowDialog();
            }

            setRandomNumberVisibility(false);
            setDefStringVisibility(false);
            setVisibile(true);
            MaxNumTextBox.Text = "";
            MinNumTextBox.Text = "";
            DefStringTextBox.Text = "";
            ArcoCheckBox.IsChecked = false;
            NodoCheckBox.IsChecked = false;
            
        }

        private void CreaAlberoButton_Click(object sender, RoutedEventArgs e)
        {
           
            if((!checkParameters(NomeCreaTextBox.Text,TipoTextBox.Text, SplitSizeCreaTextBox.Text, DepthCreaTextBox.Text)) || edgeAttrList.Count == 0 || vertexAttrList.Count == 0)
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Riempire tutti i campi!");
                error.ShowDialog();
            }
            else
            {
                //Nome$Tipo$Splitsize$Depth
                String toSend = NomeCreaTextBox.Text + "$" + TipoTextBox.Text + "$" + SplitSizeCreaTextBox.Text + "$" + DepthCreaTextBox.Text;
                client.write(toSend);

                //NomeAttrVertex1#tipoAttr:valore$NomeAttrVertex2#tipoAttr2:valore2$etc
                toSend = dictAttrParser(vertexAttrList);
                client.write(toSend);
                //NomeAttrEdge1#tipoAttr:valore$NomeAttrEdge2#tipoAttr2:valore2$etc
                toSend = dictAttrParser(edgeAttrList);
                client.write(toSend);

                int length = int.Parse(client.read());
                byte[] fileXml = new byte[length];
                client.stream.Read(fileXml, 0, length);

                using (FileStream fileStr = new FileStream(NomeCreaTextBox.Text + ".exi", FileMode.Create, System.IO.FileAccess.Write))
                {
                    fileStr.Write(fileXml, 0, length);

                }

                ParametersErrorWindow err = new ParametersErrorWindow(true, NomeCreaTextBox.Text + ".exi creato!");
                err.ShowDialog();
                this.Close();
                
                
            }
       }

        private bool checkParameters(String name, String tipoAlb, String splitSize, String depth)
        {
            bool nameTree = false;
            int nam, alb, spl, dep;

            foreach (TreeHandler trhndlr in treeHandler)
            {
                if (trhndlr.nome.Equals(name))
                {
                    nameTree = true;
                    break;
                }
            }

            if ( nameTree || int.TryParse(name, out nam) || name.Equals("") || int.TryParse(tipoAlb, out alb) || tipoAlb.Equals("") || !(int.TryParse(splitSize, out spl)) || splitSize.Equals("") || !(int.TryParse(depth, out dep)) || depth.Equals(""))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private String dictAttrParser(Dictionary<String, String[]> dict)
        {
            String toSend = "";
            int count = 0;
            //NomeAttrVertex1#tipoAttr:valore$NomeAttrVertex2#tipoAttr2:valore2$etc
            foreach (KeyValuePair<String, String[]> item in dict)
            {
                toSend += item.Key + "#" + item.Value[0] + ":" + item.Value[1];
                if(count != dict.Count-1 )
                {
                    toSend += "$"; 
                }
                count += 1;
            }

            return toSend;
        }

        private void CreaAlberoWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.write("Return");
        }



        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            String helptext = "Questa è la finestra serve per la creazione dell'albero. In questa finestra possiamo" + '\n' +
                              "definire tutte le caratteristiche che deve avere l'albero che vogliamo creare." + '\n' +
                              "La form “Nome” serve a specificare il nome dell'albero. Nella form SplitSize l'utente" + '\n' +
                              "deve inserire un intero che definisce il numero di figli che ogni nodo deve avere. " + '\n' +
                              "La form Depth serve a specificare quanti livelli dovrà avere l'albero. La parte sinistra" + '\n' +
                              "della finestra,dopo le form, sarà riempita  a run-time con tutti gli attributi già definiti" + '\n' +
                              "nel database(Attribute Definition).Al click su un singolo attributo verrà aggiornata" + '\n' +
                              "la corrispondente parte destra della finestra. L'utente,attraverso le checkbox, può specificare " + '\n' +
                              "se assegnare l'attributo ad un arco oppure ad un nodo .In base al dominio dell'attributo" + '\n' +
                              "specificato in alto, l'utente può inserire una stringa o un range numerico.";

            HelpWindow help = new HelpWindow(CreaAlberoWin.Title, helptext);
            help.ShowDialog();
        }

        
     }
}
