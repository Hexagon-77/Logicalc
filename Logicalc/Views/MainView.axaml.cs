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
using System.Numerics;

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

            CbType2.ItemsSource = Enum.GetValues(typeof(CalculationType));
            CbType2.SelectedIndex = 0;
        }

        private void Calc_Click(object sender, RoutedEventArgs e)
        {
            BaseConversionType conversionType = ((BaseConversionType?)CbType.SelectedItem) ?? BaseConversionType.Împărțiri;
            bool solutionVisible = CkShowSolve.IsChecked ?? false;

            string inputVal = TbEquation.Text;
            int oldBase = int.Parse(TbBaseOld.Text);
            int newBase = int.Parse(TbBaseNew.Text);
            string result = "Conversie invalidă.";
            string steps = string.Empty;

            try
            {
                switch (conversionType)
                {
                    case BaseConversionType.Împărțiri:
                        result = SuccessiveDivision(inputVal, oldBase, newBase, out steps);
                        break;
                    case BaseConversionType.Intermediar:
                        result = IntermediaryConversion(inputVal, oldBase, newBase, out steps);
                        break;
                    case BaseConversionType.Rapid:
                        result = RapidConversion(inputVal, oldBase, newBase, out steps);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) 
            {
                steps = $"Eroare: {ex.Message}";
            }

            if (solutionVisible)
            {
                FormulaFeedback.Text = $"→ Soluție\n{steps}\n→ Rezultat {result}";
            }
            else
            {
                FormulaFeedback.Text = result;
            }
        }

        private void Calc2_Click(object sender, RoutedEventArgs e)
        {
            CalculationType calculationType = ((CalculationType?)CbType2.SelectedItem) ?? CalculationType.Adunare;
            bool solutionVisible = CkShowSolve.IsChecked ?? false;

            string inputVal = TbNumber1.Text;
            string inputVal2 = TbNumber2.Text;
            string result = "Eroare.";
            string steps = string.Empty;
            
        }

        string StringInBase(BigInteger value, int toBase)
        {
            if (toBase < 2 || toBase > 36) throw new ArgumentException("toBase");
            if (value < 0) throw new ArgumentException("value");

            if (value == 0) return "0";

            string AlphaCodes = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string retVal = "";

            while (value > 0)
            {
                retVal = AlphaCodes[(int)(value % toBase)] + retVal;
                value /= toBase;
            }

            return retVal;
        }

        private string SuccessiveDivision(string value, int newBase, out string steps)
        {
            string result = "";
            steps = "";
            BigInteger number = BigInteger.Parse(value);
            
            while (number > 0)
            {
                BigInteger remainder = number % newBase;
                steps += $"{number}(10) / {newBase}(10) = {number / newBase}(10), rest = {remainder}(10) \n";
                result = StringInBase(remainder, newBase) + result;
                number /= newBase;
            }

            return result;
        }

        private string SuccessiveDivision(string value, int oldBase, int newBase, out string steps)
        {
            string result = "";
            steps = "";
            BigInteger number = ConvertToDecimal(value, oldBase, out string _);
            
            while (number > 0)
            {
                BigInteger remainder = number % newBase;
                steps += $"{StringInBase(number, oldBase)}({oldBase}) / {newBase}(10) = {StringInBase(number / newBase, oldBase)}({oldBase}), rest = {StringInBase(remainder, newBase)}({newBase}) \n";
                result = StringInBase(remainder, newBase) + result;
                number /= newBase;
            }

            return result;
        }

        private string IntermediaryConversion(string value, int oldBase, int newBase, out string steps)
        {
            steps = "";
            BigInteger decimalValue = ConvertToDecimal(value, oldBase, out string decimalSteps);
            steps += $"Din baza {oldBase} în baza 10:\n{decimalSteps}\n";
            string result = SuccessiveDivision(decimalValue.ToString(), newBase, out string conversionSteps);
            steps += $"Din baza 10 în baza {newBase}:\n{conversionSteps}";
            return result;
        }

        private string RapidConversion(string value, int oldBase, int newBase, out string steps)
        {
            steps = "";
            steps += $"Conversie rapidă din baza {oldBase} în {newBase}:\n";

            if (oldBase == newBase)
            {
                steps += "Bazele sunt identice.\n";
                return value;
            }

            // Convert to decimal first
            BigInteger decimalValue = ConvertToDecimal(value, oldBase, out string _);
            
            // Then convert to target base
            string result = "";
            BigInteger currentValue = decimalValue;
            
            while (currentValue > 0)
            {
                BigInteger remainder = currentValue % newBase;
                result = GetDigitChar((int)remainder) + result;
                currentValue /= newBase;
                steps += $"Pas: {currentValue} rest {remainder}\n";
            }

            return result.Length > 0 ? result : "0";
        }

        private BigInteger ConvertToDecimal(string value, int baseFrom, out string steps)
        {
            steps = "";
            BigInteger result = 0;
            foreach (char c in value.ToUpper())
            {
                int digit = GetDigitValue(c);
                if (digit >= baseFrom)
                    throw new ArgumentException($"Cifra {c} nu este validă în baza {baseFrom}");
                    
                steps += $"{result} * {baseFrom} + {digit} = {result * baseFrom + digit}\n";
                result = result * baseFrom + digit;
            }
            return result;
        }

        private int GetDigitValue(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            throw new ArgumentException($"Caracter invalid: {c}");
        }

        private char GetDigitChar(int value)
        {
            if (value < 10)
                return (char)('0' + value);
            return (char)('A' + (value - 10));
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

    public enum CalculationType
    {
        Adunare,
        Scădere,
        Înmulțire,   // Cu o cifră
        Împărțire,   // Cu o cifră
    }
}
