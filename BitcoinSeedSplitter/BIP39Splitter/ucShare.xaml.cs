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
    /// <summary>
    /// Interaction logic for ucShare.xaml
    /// </summary>
    public partial class ucShare : UserControl
    {
        private bool IsSelected = false;
        public ucShare(int iSharedID, string sShare)
        {
            InitializeComponent();
            btnID.Content = iSharedID.ToString();
            txtShare.Text = sShare;

            string[] sa = sShare.Split(' ');

            txtShare.ToolTip = $"Length:{txtShare.Text.Length} Words:{sa.Length}";
        }

        private void btnID_Click(object sender, RoutedEventArgs e)
        {
            IsSelected = !IsSelected;
            if (IsSelected == true)
            {
                txtShare.FontSize = 36;
                txtShare.FontWeight = FontWeights.Bold;
            }
            else
            {
                txtShare.FontSize = 24;
                txtShare.FontWeight = FontWeights.Normal;
            }

        }
    }
}
