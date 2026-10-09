using System.IO;
using Game.Core;
using UnityEditor;

namespace Game.Editor
{
    // Everything the tool stops to ask. New and Open throw away whatever is on
    // the grid and Save writes over a file on disk, and none of the three can be
    // undone once it has happened.
    internal static class LevelEditorDialogs
    {
        private const string Title = "Level Editor";

        // Only asked when there is something to lose, so a designer starting
        // level after level is never made to click twice.
        internal static bool MayDiscard(LevelEditorSession session)
        {
            if (!session.HasUnsavedChanges)
            {
                return true;
            }

            return EditorUtility.DisplayDialog(Title,
                Named(session.Path) + " has changes that were never saved. Throw them away?",
                "Discard", "Cancel");
        }

        internal static bool MayOverwrite(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }

            return EditorUtility.DisplayDialog(Title,
                "Write over " + Path.GetFileName(path) + "?", "Overwrite", "Cancel");
        }

        internal static void Report(Result result)
        {
            if (!result.IsSuccess)
            {
                EditorUtility.DisplayDialog(Title, result.Error, "OK");
            }
        }

        private static string Named(string path)
        {
            return string.IsNullOrEmpty(path) ? "This level" : Path.GetFileName(path);
        }
    }
}
