using System;
using System.IO;
using Game.Authoring;
using Game.Core;
using Newtonsoft.Json;
using UnityEditor;

namespace Game.Editor
{
    // Levels on disk. JSON and file dialogs live here so the draft never has to
    // know it came from a file, and so the tool reads a level through the same
    // gate the game does before it lets anyone edit it.
    internal static class LevelFile
    {
        internal const string Folder = "Assets/Levels";

        private static readonly JsonSerializerSettings Writing = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        internal static Result TryOpen(string path, out LevelDraft draft)
        {
            draft = null;

            LevelData data;

            Result readResult = TryRead(path, out data);
            if (!readResult.IsSuccess)
            {
                return readResult;
            }

            // Opened through Level.TryCreate first: a file the game would refuse
            // is also a file the grid cannot lay out, and a rejection with a
            // reason beats an exception out of ItemGrid.
            Level level;
            Result levelResult = Level.TryCreate(data, out level);
            if (!levelResult.IsSuccess)
            {
                return Result.Failure(Path.GetFileName(path) + ": " + levelResult.Error);
            }

            draft = LevelDraft.From(data);

            return Result.Success();
        }

        internal static Result Save(LevelDraft draft, string path)
        {
            Result validation = draft.Validate();
            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                File.WriteAllText(path, ToJson(draft));
            }
            catch (IOException error)
            {
                return Result.Failure("The level could not be written: " + error.Message);
            }

            AssetDatabase.Refresh();

            return Result.Success();
        }

        internal static string ToJson(LevelDraft draft)
        {
            return JsonConvert.SerializeObject(Tidy(draft.ToData()), Writing);
        }

        // Reopening a draft the window parked before a script reload. It is the
        // tool's own text rather than a designer's file, so a draft that no
        // longer validates still comes back instead of being thrown away.
        internal static LevelDraft FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                LevelData data = JsonConvert.DeserializeObject<LevelData>(json);

                return data == null ? null : LevelDraft.From(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static string AskWhereToOpen()
        {
            return EditorUtility.OpenFilePanel("Open Level", Folder, "json");
        }

        internal static string AskWhereToSave(string suggested)
        {
            return EditorUtility.SaveFilePanel("Save Level", Folder, suggested, "json");
        }

        private static Result TryRead(string path, out LevelData data)
        {
            data = null;

            try
            {
                data = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path));
            }
            catch (JsonException error)
            {
                return Result.Failure(Path.GetFileName(path) + " is not valid JSON: " + error.Message);
            }
            catch (IOException error)
            {
                return Result.Failure("The level could not be read: " + error.Message);
            }

            return data == null
                ? Result.Failure(Path.GetFileName(path) + " holds no level.")
                : Result.Success();
        }

        // An empty array would write "Items": [] into files that used to say
        // nothing at all, which turns opening a level into a diff.
        private static LevelData Tidy(LevelData data)
        {
            if (data.Items != null && data.Items.Length == 0)
            {
                data.Items = null;
            }

            return data;
        }
    }
}
