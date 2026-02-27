namespace DevAndrew.SaveLoad.Contracts
{
    public interface ISaveLoadService
    {
        bool TryLoad<T>(string fileName, out T data) where T : class;

        bool TrySave<T>(string fileName, T data) where T : class;
    }
}
