using System.IO;
using Game.Authoring;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        // A draft is plain C# and does not survive a script reload, so the window
        // parks it as text in fields Unity does serialise. Losing an hour of
        // level to an unrelated recompile is not a trade worth making.
        [SerializeField] private string _parked;
        [SerializeField] private string _parkedOnDisk;
        [SerializeField] private string _path;

        private LevelEditorSession _session;
        private LevelBrush _brush = LevelBrush.Item(CellGroundType.Wall);
        private Vector2 _scroll;

        [MenuItem("Tools/Snake/Level Editor")]
        private static void Open()
        {
            GetWindow<LevelEditorWindow>("Level Editor").minSize = new Vector2(460f, 520f);
        }

        private void OnEnable()
        {
            _session = new LevelEditorSession();
            _session.Unpark(_parked, _parkedOnDisk, _path);
        }

        private void OnDisable()
        {
            _parked = _session.Park();
            _parkedOnDisk = _session.ParkBaseline();
            _path = _session.Path;
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space();
            SettingsBar.Draw(_session.Draft);

            EditorGUILayout.Space();
            _brush = PaletteBar.Draw(_brush);

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GridPainter.Draw(_session.Draft, _brush);
            EditorGUILayout.EndScrollView();

            DrawStatus();

            if (!GUI.changed)
            {
                return;
            }

            _session.Refresh();
            Repaint();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New", EditorStyles.toolbarButton) && LevelEditorDialogs.MayDiscard(_session))
            {
                _session.NewLevel();
            }

            if (GUILayout.Button("Open", EditorStyles.toolbarButton) && LevelEditorDialogs.MayDiscard(_session))
            {
                OpenFile();
            }

            using (new EditorGUI.DisabledScope(!_session.Status.IsSuccess))
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    SaveFile();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(Name(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            if (_session.Status.IsSuccess)
            {
                EditorGUILayout.HelpBox("This level is playable.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(_session.Status.Error, MessageType.Error);
        }

        private void OpenFile()
        {
            string path = LevelFile.AskWhereToOpen();

            if (!string.IsNullOrEmpty(path))
            {
                LevelEditorDialogs.Report(_session.Open(path));
            }
        }

        private void SaveFile()
        {
            string path = _session.Path ?? LevelFile.AskWhereToSave("level");

            if (!string.IsNullOrEmpty(path) && LevelEditorDialogs.MayOverwrite(path))
            {
                LevelEditorDialogs.Report(_session.Save(path));
            }
        }

        // The star is the only sign a level is not on disk yet, so it is worth
        // the two characters next to the file name.
        private string Name()
        {
            string name = string.IsNullOrEmpty(_session.Path) ? "unsaved" : Path.GetFileName(_session.Path);

            return _session.HasUnsavedChanges ? name + " *" : name;
        }
    }
}
