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
            bool solutionVisible = CkShowSolve2.IsChecked ?? false;

            string inputVal = TbNumber1.Text.ToUpper();
            string inputVal2 = TbNumber2.Text.ToUpper();
            int baseVal = int.Parse(TbBase1.Text);
            string result = "Eroare.";
            string steps = string.Empty;

            try
            {
                switch (calculationType)
                {
                    case CalculationType.Adunare:
                        result = AddInBase(inputVal, inputVal2, baseVal, out steps);
                        break;
                    case CalculationType.Scădere:
                        result = SubtractInBase(inputVal, inputVal2, baseVal, out steps);
                        break;
                    case CalculationType.Înmulțire:
                        if (inputVal2.Length > 1)
                            throw new ArgumentException("Al doilea număr trebuie să fie o singură cifră pentru înmulțire");
                        result = MultiplyInBase(inputVal, inputVal2[0], baseVal, out steps);
                        break;
                    case CalculationType.Împărțire:
                        if (inputVal2.Length > 1)
                            throw new ArgumentException("Al doilea număr trebuie să fie o singură cifră pentru împărțire");
                        if (inputVal2 == "0")
                            throw new DivideByZeroException("Împărțirea la zero nu este permisă");
                        result = DivideInBase(inputVal, inputVal2[0], baseVal, out steps);
                        break;
                }
            }
            catch (Exception ex)
            {
                steps = $"Eroare: {ex.Message}";
                result = "Eroare";
            }

            if (solutionVisible)
            {
                FormulaFeedback2.Text = $"→ Soluție\n{steps}\n→ Rezultat {result}";
            }
            else
            {
                FormulaFeedback2.Text = result;
            }
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
            if (c >= 'A' && c <= 'Z')
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

        private string AddInBase(string num1, string num2, int baseVal, out string steps)
        {
            steps = $"Adunare în baza {baseVal}:\n\n";
            steps += $"  {num1}\n+ {num2}\n";
            steps += "  " + new string('-', Math.Max(num1.Length, num2.Length) + 1) + "\n";

            // Pad the shorter number with zeros
            int maxLength = Math.Max(num1.Length, num2.Length);
            num1 = num1.PadLeft(maxLength, '0');
            num2 = num2.PadLeft(maxLength, '0');

            string result = "";
            int carry = 0;

            for (int i = maxLength - 1; i >= 0; i--)
            {
                int digit1 = GetDigitValue(num1[i]);
                int digit2 = GetDigitValue(num2[i]);
                
                int sum = digit1 + digit2 + carry;
                int remainder = sum % baseVal;

                if (sum / baseVal > 0)
                {
                    steps += $"  {digit1} + {digit2} + transport {carry} = {sum}, scriem {GetDigitChar(remainder)} și transportăm {sum / baseVal}\n";
                }
                else if (carry > 0)
                {
                    steps += $"  {digit1} + {digit2} + transport {carry} = {sum}, scriem {GetDigitChar(remainder)}\n";
                }
                else
                {
                    steps += $"  {digit1} + {digit2} = {sum}, scriem {GetDigitChar(remainder)}\n";
                }

                carry = sum / baseVal;
                result = GetDigitChar(remainder) + result;
            }

            if (carry > 0)
            {
                result = GetDigitChar(carry) + result;
                steps += $"  Transport final {carry}\n";
            }

            return result;
        }

        private string SubtractInBase(string num1, string num2, int baseVal, out string steps)
        {
            steps = $"Scădere în baza {baseVal}:\n\n";
            steps += $"  {num1}\n- {num2}\n";
            steps += "  " + new string('-', Math.Max(num1.Length, num2.Length) + 1) + "\n";

            int maxLength = Math.Max(num1.Length, num2.Length);
            num1 = num1.PadLeft(maxLength, '0');
            num2 = num2.PadLeft(maxLength, '0');

            string result = "";
            int borrow = 0;

            for (int i = maxLength - 1; i >= 0; i--)
            {
                int digit1 = GetDigitValue(num1[i]);
                int digit2 = GetDigitValue(num2[i]);

                digit1 = digit1 - borrow;
                if (digit1 < digit2)
                {
                    digit1 += baseVal;
                    borrow = 1;
                    steps += $"  {GetDigitChar(GetDigitValue(num1[i]))} - {digit2} (împrumut {baseVal}) = {digit1 - digit2}\n";
                }
                else
                {
                    borrow = 0;
                    steps += $"  {digit1} - {digit2} = {digit1 - digit2}\n";
                }

                result = GetDigitChar(digit1 - digit2) + result;
            }

            // Remove leading zeros
            result = result.TrimStart('0');
            if (result == "") result = "0";

            return result;
        }

        private string MultiplyInBase(string num1, char digit2, int baseVal, out string steps)
        {
            steps = $"Înmulțire în baza {baseVal}:\n\n";
            steps += $"  {num1}\n× {digit2}\n";
            steps += "  " + new string('-', num1.Length + 1) + "\n";

            int multiplier = GetDigitValue(digit2);
            string result = "";
            int carry = 0;

            for (int i = num1.Length - 1; i >= 0; i--)
            {
                int digit1 = GetDigitValue(num1[i]);
                int product = digit1 * multiplier + carry;
                int remainder = product % baseVal;

                if (product / baseVal > 0)
                {
                    steps += $"  {digit1} × {multiplier} + transport {carry} = {product}, scriem {GetDigitChar(remainder)} și transportăm {product / baseVal}\n";
                }
                else if (carry > 0)
                {
                    steps += $"  {digit1} × {multiplier} + transport {carry} = {product}, scriem {GetDigitChar(remainder)}\n";
                }
                else
                {
                    steps += $"  {digit1} × {multiplier} = {product}, scriem {GetDigitChar(remainder)}\n";
                }

                carry = product / baseVal;
                result = GetDigitChar(remainder) + result;
            }

            if (carry > 0)
            {
                result = GetDigitChar(carry) + result;
                steps += $"  Transport final {carry}\n";
            }

            return result;
        }

        private string DivideInBase(string num1, char digit2, int baseVal, out string steps)
        {
            steps = $"Împărțire în baza {baseVal}:\n\n";
            int divisor = GetDigitValue(digit2);
            steps += $"  {num1} ÷ {digit2}\n";
            steps += "  " + new string('-', num1.Length + 1) + "\n";

            string result = "";
            int remainder = 0;

            for (int i = 0; i < num1.Length; i++)
            {
                int currentDigit = GetDigitValue(num1[i]);
                int current = remainder * baseVal + currentDigit;
                int quotientDigit = current / divisor;
                remainder = current % divisor;

                steps += $"  ({remainder} × {baseVal} + {currentDigit}) ÷ {divisor} = {quotientDigit}, rest {remainder}\n";
                result += GetDigitChar(quotientDigit);
            }

            // Remove leading zeros
            result = result.TrimStart('0');
            if (result == "") result = "0";

            if (remainder > 0)
            {
                steps += $"\n  Rest final: {remainder}\n";
            }

            return result;
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
