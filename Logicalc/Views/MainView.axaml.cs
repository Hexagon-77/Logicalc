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
using XamlMath.Utils;

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

            int val = int.Parse(TbEquation.Text);
            int oldBase = int.Parse(TbBaseOld.Text);
            int newBase = int.Parse(TbBaseNew.Text);
            string result = "Conversie invalidă.";
            string steps = string.Empty;

            try
            {
                switch (conversionType)
                {
                    case BaseConversionType.Împărțiri:
                        result = SuccessiveDivision(val, newBase, out steps);
                        break;
                    case BaseConversionType.Intermediar:
                        result = IntermediaryConversion(val, oldBase, newBase, out steps);
                        break;
                    case BaseConversionType.Rapid:
                        result = RapidConversion(val.ToString(), oldBase, newBase, out steps);
                        break;
                    default:
                        break;
                }
            }
            catch { }

            if (solutionVisible)
            {
                FormulaFeedback.Text = $"→ Soluție\n{steps}\n→ Rezultat {result}";
            }
            else
            {
                FormulaFeedback.Text = result;
            }
        }

        private string SuccessiveDivision(int value, int newBase, out string steps)
        {
            string result = "";
            steps = "";
            while (value > 0)
            {
                int remainder = value % newBase;
                steps += $"{value} / {newBase} = {value / newBase}, rest = {remainder} \n";
                result = remainder.ToString("X") + result;
                value /= newBase;
            }
            return result;
        }

        private string IntermediaryConversion(int value, int oldBase, int newBase, out string steps)
        {
            steps = "";
            int decimalValue = ConvertToDecimal(value.ToString(), oldBase, out string decimalSteps);
            steps += $"Din baza {oldBase} în baza 10:\n{decimalSteps}\n";
            string result = SuccessiveDivision(decimalValue, newBase, out string conversionSteps);
            steps += $"Din baza 10 în baza {newBase}:\n{conversionSteps}";
            return result;
        }

        private string RapidConversion(string value, int oldBase, int newBase, out string steps)
        {
            steps = "";
            steps += $"Conversie rapidă din baza {oldBase} în {newBase}:\n";

            int stepValue = Convert.ToInt32(value, oldBase);
            int groupSize = (int)Math.Log2(newBase) - (int)Math.Log2(oldBase) + 1;
            steps += $"Grupăm {groupSize} cifre\n";

            if (groupSize == 1)
            {
                steps += "Bazele sunt identice.\n";
                return value;
            }
            else
            {
                int groupCount = 1;

                while (stepValue > 0)
                {
                    steps += $"Grup {groupCount}: {stepValue % newBase}\n";

                    stepValue /= newBase;
                    groupCount++;
                }
            }

            string result = Convert.ToString(Convert.ToInt32(value, oldBase), newBase).ToUpper();
            return result;
        }

        private int ConvertToDecimal(string value, int baseFrom, out string steps)
        {
            steps = "";
            int result = 0;
            foreach (char c in value)
            {
                int digit = c >= '0' && c <= '9' ? c - '0' : c - 'A' + 10;
                steps += $"{result} * {baseFrom} + {digit} = {result * baseFrom + digit}\n";
                result = result * baseFrom + digit;
            }
            return result;
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
        Intermediar,
        Rapid
    }
}
