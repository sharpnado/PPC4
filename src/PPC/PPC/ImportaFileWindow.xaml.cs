using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace PPC
{
    /// <summary>
    /// Interaction logic for ImportaFileWindow.xaml
    /// </summary>
    public partial class ImportaFileWindow : Window
    {
        private ClientClass client;

        public ImportaFileWindow(ClientClass client)
        {
            InitializeComponent();
            this.client = client;
            
        }

        private void ScegliFileButton_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new System.Windows.Forms.OpenFileDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            string folderpath = dialog.FileName;

            PathFileTextBox.Text = folderpath;

        }

        private void CaricaFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (PathFileTextBox.Text.Equals(""))
            {
                ParametersErrorWindow error = new ParametersErrorWindow(false, "Inserire path file!");
                error.ShowDialog();
            }
            else
            {
                byte[] arr = File.ReadAllBytes(PathFileTextBox.Text);
                client.write(arr.Length.ToString());
                client.stream.Write(arr, 0, arr.Length);
                /***Lettura delle risposta dal server***/
                String response = client.read();
                if (response.Equals("0"))
                {
                    ParametersErrorWindow error = new ParametersErrorWindow(true, "File importato");
                    error.ShowDialog();
                    this.Close();
                }
                else
                {
                    ParametersErrorWindow error = new ParametersErrorWindow(false, "Errore nell'importazione");
                    error.ShowDialog();
                    this.Close();
                }
            }
        }

        private void ImportaFileWin_Closing(object sender, CancelEventArgs e)
        {
            client.write("Return");
        }

        private void HelpImportaButton_Click(object sender, RoutedEventArgs e)
        {
            String helptext = "Questa finestra serve per importare il file " + '\n' +
                              "nel database. L'utente può scegliere il file " + '\n' +
                              "nel file-system attraverso la finestra che " + '\n' +
                              "si apre, al click sul bottone scegli.";
            HelpWindow help = new HelpWindow(ImportaFileWin.Title, helptext);
            help.ShowDialog();
        }

    }
}
