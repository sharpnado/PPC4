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
    /// Logica di interazione per ParametersErrorWindow.xaml
    /// </summary>
    public partial class ParametersErrorWindow : Window
    {
        public ParametersErrorWindow(bool result, String label)
        {
            
            InitializeComponent();
            MainErrLabel.FontSize = 24;
            if (result)
            {
                MainErrLabel.Text = "Successo:";
            }
            else 
            {
                MainErrLabel.Text = "Errore:";
            }
            ErrorLabel.Content = label;
        }

        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
