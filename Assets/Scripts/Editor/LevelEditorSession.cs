using Game.Authoring;
using Game.Core;

namespace Game.Editor
{
    // The level the window has open: the draft, the file it came from, and
    // whether it would play. The three only ever change together, which leaves
    // the window with nothing to keep in step.
    internal sealed class LevelEditorSession
    {
        private const int NewBoardSide = 9;

        internal LevelEditorSession()
        {
            NewLevel();
        }

        // What the draft looked like when it was last opened or written. Text is
        // what a level file is, so comparing against it needs nothing the tool
        // does not already do to save.
        private string _onDisk;

        internal LevelDraft Draft { get; private set; }
        internal string Path { get; private set; }
        internal Result Status { get; private set; }
        internal bool HasUnsavedChanges { get; private set; }

        // Both answers are derived from the draft and both are wanted every
        // frame, so they are worked out once per edit rather than per repaint.
        internal void Refresh()
        {
            Status = Draft.Validate();
            HasUnsavedChanges = Park() != _onDisk;
        }

        internal void NewLevel()
        {
            Begin(new LevelDraft(new GridSize(NewBoardSide, NewBoardSide)), null);
        }

        internal Result Open(string path)
        {
            LevelDraft opened;

            Result result = LevelFile.TryOpen(path, out opened);
            if (result.IsSuccess)
            {
                Begin(opened, path);
            }

            return result;
        }

        internal Result Save(string path)
        {
            Result result = LevelFile.Save(Draft, path);
            if (result.IsSuccess)
            {
                Path = path;
                Settle();
            }

            return result;
        }

        internal string Park()
        {
            return LevelFile.ToJson(Draft);
        }

        internal string ParkBaseline()
        {
            return _onDisk;
        }

        // What came back from a script reload is the tool's own text, so a draft
        // that no longer validates is still restored: the designer gets their
        // work back along with the complaint about it. The baseline comes back
        // too, otherwise a reload would quietly call unsaved work saved.
        internal void Unpark(string json, string onDisk, string path)
        {
            LevelDraft draft = LevelFile.FromJson(json);

            if (draft == null)
            {
                return;
            }

            Draft = draft;
            Path = path;
            _onDisk = onDisk;

            Refresh();
        }

        private void Begin(LevelDraft draft, string path)
        {
            Draft = draft;
            Path = path;

            Settle();
        }

        private void Settle()
        {
            _onDisk = Park();

            Refresh();
        }
    }
}
