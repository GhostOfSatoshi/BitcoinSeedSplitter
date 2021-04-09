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
using System.Windows.Threading;

namespace BIP39Splitter
{
    /// <summary>
    /// Interaction logic for ucSplit.xaml
    /// </summary>
    public partial class ucSplit : UserControl
    {
        DispatcherTimer timerDoCounter;
        private DateTime dRunUntil;

        public ucSplit()
        {
            InitializeComponent();
        }

        private void SetDefValues()
        {
            txtShareCount.Text = "10";
            txtThreshold.Text = "6";

            txtSeedToSplit.TextChanged += txtSeedToSplit_TextChanged;

#if(DEBUG)
            txtSeedToSplit.Text = "they traffic utility squirrel rent morning shoot stool music female gown pupil hurt roast quantum gym impose unit consider thrive either farm easily betray";
            txtPassword.Text = "Pass123";
            txtSeconds.Text = "3";
            //txtSeedToSplit.Text = "submit mail gauge page fork matter pact absorb adjust unusual dance bring";
#endif
        }
        private void txtSeedToSplit_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            try
            {
                clsSeed seed = new clsSeed(txtSeedToSplit.Text);
                txtSeedToSplit.ToolTip = $"Count of words: {seed.iWordCount}";
                txtSeedToSplit.Background = new SolidColorBrush(Colors.LightGreen);
                txtSeedBIP39HEX.Text = seed.BIP39SeedHex;
                txtSeedHex.Text = seed.SeedBitsHex;
                btnDoSplit.IsEnabled = true;
                btnCopySharesToClipboard.IsEnabled = false;
                txtSeedToSplit.IsReadOnly = true;
                btnLockSeed.Visibility = Visibility.Visible;
                btnLockSeedOpen.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                txtSeedToSplit.Background = new SolidColorBrush(Colors.LightSalmon);
                txtSeedToSplit.ToolTip = $"Invalid seed";
                lstSplitShares.Items.Clear();
                txtSeedBIP39HEX.Text = "";
                txtSeedHex.Text = "";
                btnDoSplit.IsEnabled = false;
                btnCopySharesToClipboard.IsEnabled = false;
                txtSeedToSplit.IsReadOnly = false;
                btnLockSeed.Visibility = Visibility.Collapsed;
                btnLockSeedOpen.Visibility = Visibility.Visible;
            }
        }
        private async void btnDoSplit_Click(object sender, RoutedEventArgs e)
        {
            btnDoSplit.Content = "Calculating...";
            gridSplit.IsEnabled = false;
            lstSplitShares.Items.Clear();
            txtTestMerged.Text = "";

            clsSeed seed = new clsSeed(txtSeedToSplit.Text);
            byte[] baSeedHex = clsHelpers.StringToByteArray(seed.SeedBitsHex);

            List<string> liBestShares = new List<string>();
            int iBestMaxLen = int.MaxValue;
            Single siBestAvg = Single.MaxValue;
            string sPassword = txtPassword.Text;
            byte bShareCount = byte.Parse(txtShareCount.Text);
            byte bThreshold = byte.Parse(txtThreshold.Text);
            int iTried = 0;

            timerDoCounter = new DispatcherTimer();
            timerDoCounter.Interval = new TimeSpan(0, 0, 1);
            timerDoCounter.Tick += timerDoCounter_Tick;
            timerDoCounter.Start();

            dRunUntil = DateTime.Now.AddSeconds(Convert.ToInt32(txtSeconds.Text));
            await Task.Run(() =>
            {
                while (DateTime.Now < dRunUntil)
                {
                    iTried++;
                    clsBIP39Splitter splitter = new clsBIP39Splitter();
                    List<string> liShares = splitter.SeedToSplitSecrets(seed.SeedBytes, bShareCount, bThreshold, sPassword);
                    int iCurrMax = 0, iCurrTotal = 0;

                    foreach (string s1 in liShares)
                    {
                        iCurrTotal += s1.Length;
                        if (iCurrMax < s1.Length)
                            iCurrMax = s1.Length;
                    }
                    Single siCurrAvg = iCurrTotal / liShares.Count;

                    if ((siCurrAvg < siBestAvg) && (iCurrMax <= iBestMaxLen))
                    {
                        siBestAvg = siCurrAvg;
                        iBestMaxLen = iCurrMax;
                        liBestShares = liShares;
                    }
                }
            });

            for (int i1 = 0; i1 < liBestShares.Count; i1++)
            {
                string sShare = liBestShares[i1];
                ucShare uc1 = new ucShare(i1 + 1, sShare);
                lstSplitShares.Items.Add(uc1);
            }
            btnDoSplit.Content = "Do Split";
            btnDoSplit.ToolTip = $"Tried:{iTried} Avg:{siBestAvg} MaxLen={iBestMaxLen}";
            gridSplit.IsEnabled = true;
            btnCopySharesToClipboard.IsEnabled = true;
        }

        private void timerDoCounter_Tick(object sender, EventArgs e)
        {
            int iTotalSec = Convert.ToInt32(dRunUntil.Subtract(DateTime.Now).TotalSeconds);

            if (iTotalSec < 0)
            {
                btnDoSplit.Content = "Do Split";
                timerDoCounter.Stop();
            }
            else
            {
                btnDoSplit.Content = $"{iTotalSec}";
            }
        }

        private void lstSplitShares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> liSelShares = new List<string>();

            foreach (ucShare uc1 in lstSplitShares.SelectedItems)
            {
                liSelShares.Add(uc1.txtShare.Text);
            }

            if (liSelShares.Count < 2)
                return;

            clsBIP39Splitter merger = new clsBIP39Splitter();

            byte[] baSecret;
            string sRet = merger.JoinSecrets(liSelShares, out baSecret,txtPassword.Text);
            if (sRet!="")
            {
                txtTestMerged.Text = sRet;
                txtTestMerged.Background = new SolidColorBrush(Colors.Yellow);
                return;
            }

            try
            {
                clsSeed seed = new clsSeed(baSecret);
                txtTestMerged.Text = seed.SeedString;

                if (txtSeedToSplit.Text == txtTestMerged.Text)
                {
                    txtTestMerged.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    txtTestMerged.Background = new SolidColorBrush(Colors.LightSalmon);
                }
            }
            catch (Exception ex)
            {
                txtTestMerged.Text = $"Wrong password used ({ex.Message})";
                txtTestMerged.Background = new SolidColorBrush(Colors.LightSalmon);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetDefValues();
        }

        private void btnCopySharesToClipboard_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for(int i1=0;i1<lstSplitShares.Items.Count;i1++)
            {
                ucShare share = (ucShare)lstSplitShares.Items[i1];
                sb.Append($"{i1+1}. {share.txtShare.Text}{Environment.NewLine}");
            }

            string sClip = sb.ToString();
            Clipboard.Clear();
            Clipboard.SetText(sClip);
        }

        private void btnLockSeed_Click(object sender, RoutedEventArgs e)
        {
            txtSeedToSplit.IsReadOnly = false;
            btnLockSeed.Visibility = Visibility.Collapsed;
            btnLockSeedOpen.Visibility = Visibility.Visible;
        }

        private void txtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemMinus) || (e.Key==Key.Back))
                return;
            if ((e.Key >= Key.D0) && (e.Key <= Key.D9))
                return;
            if ((e.Key >= Key.A) && (e.Key <= Key.Z))
                return;
            if ((e.Key == Key.NumPad0) || (e.Key == Key.NumPad1) || (e.Key == Key.NumPad2) || (e.Key == Key.NumPad3) || (e.Key == Key.NumPad4) || (e.Key == Key.NumPad5) || (e.Key == Key.NumPad6) || (e.Key == Key.NumPad7) || (e.Key == Key.NumPad8) || (e.Key == Key.NumPad9))
                return;

            e.Handled = true;
        }

        private void txtSeconds_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
                return;
            if ((e.Key >= Key.D0) && (e.Key <= Key.D9))
                return;
            if ((e.Key == Key.NumPad0) || (e.Key == Key.NumPad1) || (e.Key == Key.NumPad2) || (e.Key == Key.NumPad3) || (e.Key == Key.NumPad4) || (e.Key == Key.NumPad5) || (e.Key == Key.NumPad6) || (e.Key == Key.NumPad7) || (e.Key == Key.NumPad8) || (e.Key == Key.NumPad9))
                return;
            e.Handled = true;
        }
    }
}
