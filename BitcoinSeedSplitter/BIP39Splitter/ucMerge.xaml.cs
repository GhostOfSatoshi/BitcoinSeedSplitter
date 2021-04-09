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
    /// Interaction logic for ucMerge.xaml
    /// </summary>
    public partial class ucMerge : UserControl
    {
        private clsBIP39Splitter seedChecker = new clsBIP39Splitter();
        private List<string> liCurrShares = new List<string>();
        private DateTime dDontExpectKeyPressBefore = DateTime.Now;
        public ucMerge()
        {
            InitializeComponent();
        }
        private void txtNewWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (dDontExpectKeyPressBefore > DateTime.Now)
            {
                e.Handled = true;
            }
        }
        private void txtNewWord_TextChanged(object sender, TextChangedEventArgs e)
        {                
            spWordOptions.Children.Clear();
            if (txtNewWord.Text.Length>80)
            {   //Full share
                string[] sa = txtNewWord.Text.Split(' ');
                txtNewWord.Text = "";
                if (sa.Length > 15)
                {
                    try
                    {
                        Int16[] iaWords = clsBIP39Words.GetWordIDsFromWords(sa); //Fails if any incorrect words

                        for (int i1=0;i1<sa.Length;i1++)
                        {
                            if (i1 == iaWords.Length - 1)
                                AddToCurrChildren(sa[i1]);
                            else
                                AddToCurrChildren(sa[i1], false);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }
            if ((txtNewWord.Text.Length > 0) && (txtNewWord.Text.Length<5))
            {  //Typing
                List<short> liWords = clsBIP39Words.GetAllStartsWith(txtNewWord.Text);
                if (liWords.Count == 1)
                {
                    txtNewWord.Text = "";
                    dDontExpectKeyPressBefore = DateTime.Now.AddMilliseconds(500);
                    AddToCurrChildren(clsBIP39Words.liWordsENG[liWords[0]]);
                }
                else if ((liWords.Count > 0) && (liWords.Count <= 10))
                {
                    foreach (short i1 in liWords)
                    {
                        TextBlock block = new TextBlock() { Background = new SolidColorBrush(Colors.LightBlue), Text = clsBIP39Words.liWordsENG[i1], FontSize = 20, Margin = new Thickness(10, 0, 10, 0)};
                        block.MouseDown += newWord_MouseDown;
                        spWordOptions.Children.Add(block);
                    }
                }
            }
        }

        private void AddToCurrChildren(string sWord, bool DoCheckIfFull = true)
        {
            TextBlock block = new TextBlock() { Text = sWord, Margin = new Thickness(0, 0, 10, 0), FontSize = 30, Background=new SolidColorBrush(Colors.LightGreen) };
            block.MouseDown += ShareWord_MouseDown;
            spCurrShare.Children.Add(block);
            if (DoCheckIfFull==true)
                CheckAndAddShare();
        }
        private void CalculateFromValidShares()
        {
            if (liCurrShares.Count < 3)
                return;

            clsBIP39Splitter merger = new clsBIP39Splitter();
            byte[] baSeed;
            string sRet=merger.JoinSecrets(liCurrShares, out baSeed, txtPassword.Text);

            if (sRet!="")
            {
                txtMerged.Text = sRet;
                txtMerged.Background = new SolidColorBrush(Colors.LightSalmon);
                return;
            }
            try
            {
                clsSeed seed = new clsSeed(baSeed);
                txtMerged.Text = seed.SeedString;
                txtMerged.Background = new SolidColorBrush(Colors.LightGreen);
            }
            catch (Exception ex)
            {
                txtMerged.Text = $"Wrong password used ({ex.Message})";
                txtMerged.Background = new SolidColorBrush(Colors.LightSalmon);
            }
        }
        private void newWord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock block = (TextBlock)sender;
            spWordOptions.Children.Clear();
            txtNewWord.Text = "";
            AddToCurrChildren(block.Text);
        }
        private void CheckAndAddShare()
        {
            string sShare = "";
            foreach(TextBlock txt in spCurrShare.Children)
            {
                sShare += txt.Text + " ";
            }

            sShare = sShare.Trim();

            if (seedChecker.CheckShare(sShare)==true)
            {
                if (liCurrShares.Contains(sShare) == false)
                {
                    spCurrShare.Children.Clear();
                    liCurrShares.Add(sShare);
                    lstShares.Items.Add(sShare);
                    CalculateFromValidShares();
                }
            }

        }
        private void ShareWord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock block = (TextBlock)sender;
            spCurrShare.Children.Remove(block);
        }

        private void txtMerged_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void txtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemMinus) || (e.Key == Key.Back))
                return;
            if ((e.Key >= Key.D0) && (e.Key <= Key.D9))
                return;
            if ((e.Key >= Key.A) && (e.Key <= Key.Z))
                return;
            if ((e.Key == Key.NumPad0) || (e.Key == Key.NumPad1) || (e.Key == Key.NumPad2) || (e.Key == Key.NumPad3) || (e.Key == Key.NumPad4) || (e.Key == Key.NumPad5) || (e.Key == Key.NumPad6) || (e.Key == Key.NumPad7) || (e.Key == Key.NumPad8) || (e.Key == Key.NumPad9))
                return;

            e.Handled = true;
        }

        private void txtPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateFromValidShares();
        }
    }
}

//2 of 3 test
//beach lounge cattle useless zone enter quit leg exchange coil impact loop globe rose found wild father minimum mother grid nose ignore spray vacant goose short example meat
//beach dust car upper crouch sister nice toy mimic team drop raw enact analyst unhappy word delay call yard first apart atom scrub hunt neck protect siege curious
