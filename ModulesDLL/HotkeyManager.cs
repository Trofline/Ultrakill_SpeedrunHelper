using UnityEngine;
using System;

namespace TroflineMod
{
    [Serializable]
    public class Binding
    {
        public KeyCode MainKey = KeyCode.None;
        public bool Ctrl, Shift, Alt;

        public Binding(KeyCode key, bool c = false, bool s = false, bool a = false)
        {
            MainKey = key; Ctrl = c; Shift = s; Alt = a;
        }

        public override string ToString()
        {
            if (MainKey == KeyCode.None) return "NONE";
            string s = "";
            if (Ctrl) s += "Ctrl + ";
            if (Shift) s += "Shift + ";
            if (Alt) s += "Alt + ";
            return s + MainKey.ToString();
        }
    }

    public static class HotkeyLogic
    {
        public static bool IsPressed(Binding b)
        {
            if (b.MainKey == KeyCode.None) return false;
            bool c = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool s = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool a = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            return Input.GetKeyDown(b.MainKey) && c == b.Ctrl && s == b.Shift && a == b.Alt;
        }

        // Hilfsfunktion: Ist die Taste ein Modifikator?
        public static bool IsModifier(KeyCode k)
        {
            return k == KeyCode.LeftControl || k == KeyCode.RightControl ||
                   k == KeyCode.LeftShift || k == KeyCode.RightShift ||
                   k == KeyCode.LeftAlt || k == KeyCode.RightAlt;
        }
    }
}