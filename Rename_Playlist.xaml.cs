﻿using System;
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
using System.Windows.Shapes;

namespace myTunes
{
    /// <summary>
    /// Interaction logic for Rename_Playlist.xaml
    /// </summary>
    public partial class Rename_Playlist : Window
    {
        public Rename_Playlist()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(playlistTextBox.Text.Trim()))
            {
                DialogResult = true;
            }
            else
            {
                playlistWarning warning = new playlistWarning();
                warning.warningMessage.Content = "Please enter a valid playlist name";
                warning.ShowDialog();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
