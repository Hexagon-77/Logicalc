using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using Avalonia.Threading;
using AngouriMath;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngouriMath.Extensions;

namespace Logicalc.Views
{
    public partial class MainView : UserControl
    {
        string Name;

        public List<(string, int)> Solves;

        public MainView()
        {
            InitializeComponent();

            CbType.ItemsSource = Enum.GetValues(typeof(BaseConversionType));
            CbType.SelectedIndex = 0;
        }

        private void Calc_Click(object sender, RoutedEventArgs e)
        {
            BaseConversionType conversionType = ((BaseConversionType?)CbType.SelectedItem) ?? BaseConversionType.Împărțiri;
            bool solutionVisible = CkShowSolve.IsChecked ?? false;

            switch (conversionType)
            {
                case BaseConversionType.Împărțiri:
                    break;
                case BaseConversionType.Substituție:
                    break;
                case BaseConversionType.Intermediar:
                    break;
                case BaseConversionType.Rapid:
                    break;
                default:
                    break;
            }
        }

        private async void TbEquation_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Formula.Opacity = 0;
                FormulaBase.Opacity = 0;
                await Task.Delay(200);

                Formula.Text = TbEquation.Text;
                FormulaBase.Text = $"({TbBaseOld.Text})";

                Formula.Opacity = 1;
                FormulaBase.Opacity = 1;
            }
            catch { }
        }
    }

    public enum BaseConversionType
    {
        Împărțiri,
        Substituție,
        Intermediar,
        Rapid
    }
}
