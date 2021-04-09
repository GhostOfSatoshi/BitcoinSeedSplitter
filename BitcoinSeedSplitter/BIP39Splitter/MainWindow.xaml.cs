using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BIP39Splitter
{
    public partial class MainWindow : Window
    {
        private ucSplit currSplit;
        private ucMerge currMerge;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void rbSplit_Checked(object sender, RoutedEventArgs e)
        {
            if (currMerge != null)
                gridMain.Children.Remove(currMerge);
            if (currSplit==null)
                currSplit = new ucSplit();

            Grid.SetRow(currSplit, 3);
            gridMain.Children.Add(currSplit);
        }
        private void rbMerge_Checked(object sender, RoutedEventArgs e)
        {
            if (currSplit != null)
                gridMain.Children.Remove(currSplit);
            if (currMerge==null)
                currMerge = new ucMerge();

            Grid.SetRow(currMerge, 3);
            gridMain.Children.Add(currMerge);
        }


    }
}
