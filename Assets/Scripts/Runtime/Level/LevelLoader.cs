using Game.Core;
using Newtonsoft.Json;

namespace Game.Runtime
{
    public static class LevelLoader
    {
        public static Result TryLoad(string json, out Level level)
        {
            level = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return Result.Failure("The level file is empty.");
            }

            LevelData data;

            try
            {
                data = JsonConvert.DeserializeObject<LevelData>(json);
            }
            catch (JsonException error)
            {
                return Result.Failure("The level file is not valid JSON: " + error.Message);
            }

            if (data == null)
            {
                return Result.Failure("The level file holds no level.");
            }

            return Level.TryCreate(data, out level);
        }
    }
}
