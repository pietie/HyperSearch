using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace HyperSearch.Classes
{
    public class KeyList
    {
        private List<System.Windows.Input.Key> Keys;

        public KeyList() { Keys = new List<System.Windows.Input.Key>(); }

        public KeyList(params Key[] keys)
        {
            Keys = new List<System.Windows.Input.Key>();
            if (keys != null) Keys.AddRange(keys);
        }

        public bool Is(System.Windows.Input.Key key)
        {
            if (this.Keys == null) return false;

            return Keys.Contains(key);
        }

        public void Add(Key key)
        {
            if (!this.Keys.Contains(key))
                this.Keys.Add(key);
        }

        public int[] ToKeyCodeList()
        {
            return this.Keys.Select(k => (int)k).ToArray();
        }

        public override string ToString()
        {
            if (this.Keys == null || this.Keys.Count == 0) return null;

            return string.Join(",", this.Keys.Select(k => KeyToFriendlyStr(k)).ToArray());
        }

        private string KeyToFriendlyStr(Key key)
        {
            var keyName = key.ToString();
            var digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            switch (key)
            {
                case Key.D0: return "0";
                case Key.D1: return "1";
                case Key.D2: return "2";
                case Key.D3: return "3";
                case Key.D4: return "4";
                case Key.D5: return "5";
                case Key.D6: return "6";
                case Key.D7: return "7";
                case Key.D8: return "8";
                case Key.D9: return "9";


                case Key.OemSemicolon:/*Oem1*/ return ";";
                case Key.OemQuestion: /*Oem2*/ return "?";
                case Key.OemTilde:     //oem3
                    return "~";
                case Key.OemOpenBrackets:  //oem4
                    return "[";
                case Key.OemPipe:  //oem5
                    return "|";
                case Key.OemCloseBrackets:    //oem6
                    return "]";
                case Key.OemQuotes:        //oem7
                    return "\"";
                case Key.OemBackslash: //oem102
                    return "\\";
                case Key.OemPlus: return "+";
                case Key.OemMinus: return "-";
                case Key.OemComma: return ",";
                case Key.OemPeriod: return ".";

                case Key.Back: return "Backspace";

                default:
                    // simply remove the word Oem unles it's one Oem[n] .. e.g Oem10
                    if (keyName.StartsWith("Oem") && !(keyName.Length > 3 && digits.Contains(keyName[3])))
                    {
                        return keyName.Remove(0, 3);
                    }


                    return keyName;
            }
        }


        public static implicit operator KeyList(Key key)
        {
            return new KeyList(key);
        }

    }
}
