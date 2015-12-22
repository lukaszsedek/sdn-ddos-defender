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
using System.IO;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;


namespace Mahapps
{

    

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : MetroWindow
    {

        SettingsSingleton _settings;

        public Settings()
        {
            _settings = SettingsSingleton.Instance;
            InitializeComponent();
            settingsIPText.DataContext = _settings;
            settingsPortnumberText.DataContext = _settings;
            
            DataContext = _settings;
            
        }

        private void updateSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }


        }


    }

    


