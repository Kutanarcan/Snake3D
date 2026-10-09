using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public static class LevelSequenceLoader
    {
        public static Result TryLoad(TextAsset[] files, out LevelSequence sequence)
        {
            sequence = null;

            if (files == null || files.Length == 0)
            {
                return Result.Failure("GameInstaller has no level files assigned.");
            }

            Level[] levels = new Level[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Result fileResult = TryLoadOne(files[i], i, levels);
                if (!fileResult.IsSuccess)
                {
                    return fileResult;
                }
            }

            sequence = new LevelSequence(levels);

            return Result.Success();
        }

        private static Result TryLoadOne(TextAsset file, int index, Level[] levels)
        {
            if (file == null)
            {
                return Result.Failure("Level file " + (index + 1) + " of the sequence is missing.");
            }

            Level level;
            Result loadResult = LevelLoader.TryLoad(file.text, out level);
            if (!loadResult.IsSuccess)
            {
                return Result.Failure(file.name + ": " + loadResult.Error);
            }

            levels[index] = level;

            return Result.Success();
        }
    }
}
