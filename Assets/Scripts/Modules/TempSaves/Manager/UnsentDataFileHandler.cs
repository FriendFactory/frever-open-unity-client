using Common;

namespace Modules.TempSaves.Manager
{
    public abstract class UnsentDataFileHandler<T> where T : class
    {
        private readonly TempFileManager _tempFileManager;
        private readonly string _filePath;

        protected UnsentDataFileHandler(TempFileManager tempFileManager, string fileName)
        {
            _tempFileManager = tempFileManager;
            _filePath = $"{Constants.FileDefaultPaths.SAVE_FOLDER}/{fileName}";
        }

        public void Save(T unsentEvent)
        {
            _tempFileManager.SaveDataLocally(unsentEvent, _filePath);
        }

        public bool TryLoad(out T unsentEvent)
        {
            unsentEvent = _tempFileManager.GetData<T>(_filePath);

            if (_tempFileManager.FileExists(_filePath))
            {
                _tempFileManager.RemoveTempFile(_filePath);
            }

            return unsentEvent != null;
        }
    }
}