using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PPC
{
    /// <summary>
    /// Interaction logic for EsportaFileWindow.xaml
    /// </summary>
    public partial class EsportaFileWindow : Window
    {
        private ClientClass client;
        private TreeHandler[] trees;
        private String file;

        public EsportaFileWindow(ClientClass client, TreeHandler[] parameters)
        {
           
            InitializeComponent();
            this.client = client;
            this.trees = parameters;
            this.file = "";

            foreach (TreeHandler tree in trees)
            {
                MenuComboBox.Items.Add(tree.nome);
            }
        }

        private void MenuComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            this.file = comboBox.SelectedItem.ToString();
        }

        private void EsportaButton_Click(object sender, RoutedEventArgs e)
        {
            if (file.Equals(""))
            {
                ParametersErrorWindow err = new ParametersErrorWindow(false, "Selezionare file albero");
                err.ShowDialog();
            }
            else
            {
                client.write(file);
                int length = int.Parse(client.read());
                byte[] fileXml = new byte[length];
                client.stream.Read(fileXml, 0, length);

                using (FileStream fileStr = new FileStream(file + ".exi", FileMode.Create, System.IO.FileAccess.Write))
                {
                    fileStr.Write(fileXml, 0, length);

                }

                ParametersErrorWindow err = new ParametersErrorWindow(true, file + ".exi creato!");
                err.ShowDialog();

                
            }

            this.Close();

        }

        private void EsportaFileWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.write("Return");
        }

        private void HelpEsportaButton_Click(object sender, RoutedEventArgs e)
        {
            String helptext = "Questa finestra serve per esportare un albero" + '\n' +
                               "sul file-system. L'utente può scegliere un albero" + '\n' +
                               "nel menù a tendina “Seleziona Albero” e al click " + '\n' +
                               "su esporta verrà creato un file “.exi”.";
            HelpWindow help = new HelpWindow(EsportaFileWin.Title, helptext);
            help.ShowDialog();
        }



    }
}
