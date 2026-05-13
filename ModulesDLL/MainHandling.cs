using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TroflineMod
{
    public class MainHandling : MonoBehaviour
    {
        public static bool showMenu = true;
        public static bool showHotkeyMenu = false; // Schalter für das zweite Fenster
        public static bool noDamage = false;
        public static bool infStamina = false;

        public static bool showHitboxes = false;
        public static bool showVolumes = false;
        public static bool showEnemyHitboxes = false;

        // Hotkeys (Behalte deine Standard-Binds bei)
        public static Binding[] saveBinds = new Binding[4] {
            new Binding(KeyCode.F1, true), new Binding(KeyCode.F2, true),
            new Binding(KeyCode.F3, true), new Binding(KeyCode.F4, true)
        };
        public static Binding[] loadBinds = new Binding[4] {
            new Binding(KeyCode.F1), new Binding(KeyCode.F2),
            new Binding(KeyCode.F3), new Binding(KeyCode.F4)
        };

        private int listeningIndex = -1;
        private bool listeningSave = true;

        private Rect windowRect = new Rect(20, 20, 300, 400);
        private Rect hotkeyRect = new Rect(330, 20, 400, 300); // Position für das Hotkey-Fenster

        private Harmony harmony;
        private const string harmonyId = "com.trofline.black.ultrakill";

        void Start()
        {
            harmony = new Harmony(harmonyId);
            harmony.UnpatchAll(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Insert)) showMenu = !showMenu;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) SceneHelper.RestartScene();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Delete)) EjectTrainer();

            // Wenn wir gerade eine Taste lernen, keine Hotkeys auslösen
            if (listeningIndex != -1)
            {
                HandleListening();
            }
            else
            {
                CheckHotkeys();
            }
        }

        void OnGUI()
        {
            if (!showMenu) return;

            windowRect = GUI.Window(0, windowRect, DrawMenu, "SpeedrunHelper");

            if (showHotkeyMenu)
            {
                hotkeyRect = GUI.Window(1, hotkeyRect, DrawHotkeyWindow, "HOTKEY MANAGER");
            }
        }

        // --- HAUPTMENÜ (Clean & Ordentlich) ---
        void DrawMenu(int windowID)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.red;

            GUILayout.Space(10);
            GUILayout.Label("--- SpeedrunHelper ---", titleStyle);
            GUILayout.Space(15);

            noDamage = GUILayout.Toggle(noDamage, " [1] No Damage");
            infStamina = GUILayout.Toggle(infStamina, " [2] Infinite Stamina");

            GUILayout.Space(10);
            showHitboxes = GUILayout.Toggle(showHitboxes, " [3] World Hitboxes (Cyan)");
            showVolumes = GUILayout.Toggle(showVolumes, " [4] Volumes/Triggers (Green)");
            showEnemyHitboxes = GUILayout.Toggle(showEnemyHitboxes, " [5] Enemy Hitboxes (Red)");

            GUILayout.Space(20);
            GUILayout.Label("--- Position Saver ---", GUILayout.ExpandWidth(true));
            GUILayout.Label("Active Slots: 1 - 4");
            GUILayout.Space(10);
           
            if (GUILayout.Button("Reload Level [F5]")) SceneHelper.RestartScene();

            GUILayout.Space(10);

            if (GUILayout.Button(showHotkeyMenu ? "Close Hotkey Manager" : "Open Hotkey Manager")) showHotkeyMenu = !showHotkeyMenu;
            
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            if (GUILayout.Button("Hide Menu [INSERT]")) showMenu = false;
            
            GUILayout.Space(5);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Eject Trainer [DELETE]")) EjectTrainer();
            GUI.backgroundColor = Color.white;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // --- HOTKEY MANAGER FENSTER ---
        void DrawHotkeyWindow(int windowID)
        {
            GUILayout.Label("Click a button to rebind. Modifiers (Ctrl/Shift/Alt) are detected while pressing the main key.");
            GUILayout.Space(10);

            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Slot {i + 1}:", GUILayout.Width(50));

                // Speicher-Button
                string saveText = (listeningIndex == i && listeningSave) ? "Listening..." : "Save: " + saveBinds[i].ToString();
                if (GUILayout.Button(saveText)) { listeningIndex = i; listeningSave = true; }

                // Teleport-Button
                string loadText = (listeningIndex == i && !listeningSave) ? "Listening..." : "Load: " + loadBinds[i].ToString();
                if (GUILayout.Button(loadText)) { listeningIndex = i; listeningSave = false; }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Close Manager")) showHotkeyMenu = false;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        void CheckHotkeys()
        {
            for (int i = 0; i < 4; i++)
            {
                if (HotkeyLogic.IsPressed(saveBinds[i])) PositionSafer.Save(i);
                if (HotkeyLogic.IsPressed(loadBinds[i])) PositionSafer.Teleport(i);
            }
        }

        void HandleListening()
        {
            // Wir scannen alle Tasten
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                // WICHTIG: Wir ignorieren die Modifikatoren (Strg, Shift, Alt) als "Haupttaste"
                if (Input.GetKeyDown(k) && !HotkeyLogic.IsModifier(k))
                {
                    // Wir schauen beim Drücken der Haupttaste, ob die Modifikatoren GEHALTEN werden
                    bool c = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    bool s = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    bool a = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

                    if (listeningSave) saveBinds[listeningIndex] = new Binding(k, c, s, a);
                    else loadBinds[listeningIndex] = new Binding(k, c, s, a);

                    listeningIndex = -1; // Aufnahme beendet
                }
            }
        }

        private void EjectTrainer()
        {
            Time.timeScale = 1f;
            try
            {
                LevelStats stats = FindObjectOfType<LevelStats>();
                if (stats != null && stats.time != null) stats.time.color = Color.white;
            }
            catch { }
            try { if (harmony != null) harmony.UnpatchAll(harmonyId); } catch { }
            try { SceneHelper.RestartScene(); } catch { }
            Destroy(gameObject);
        }
    }
}