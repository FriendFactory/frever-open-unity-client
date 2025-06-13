using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TipsManagment
{
    [Serializable]
    public class TutorialProgress
    {
        public List<TutorialProgressItem> progressItems = new List<TutorialProgressItem>();

        public void MarkTipAsIgnored(long id)
        {
            var item = GetItem(id);
            item.IgnoreCount++;
        }

        public void MarkTipAsDone(long id)
        {
            var item = GetItem(id);
            item.Done = true;
        }

        public void Save()
        {
            var path = Application.persistentDataPath;
            path = Path.Combine(path, "TutorialProgress.prg");

            byte[] bytes;

            BinaryFormatter formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, this);
                bytes = memoryStream.ToArray();
            }

            File.WriteAllBytes(path, bytes);
        }

        public void Load()
        {
            var path = Application.persistentDataPath;
            path = Path.Combine(path, "TutorialProgress.prg");

            if (!File.Exists(path))
            {
                return;
            }

            var bytes = File.ReadAllBytes(path);

            BinaryFormatter formatter = new BinaryFormatter();
            var progress = new TutorialProgress();
            using (var memoryStream = new MemoryStream(bytes))
            {
                var desObj = formatter.Deserialize(memoryStream);
                if (desObj != null)
                {
                    progress = desObj as TutorialProgress;
                }
            }
            progressItems = progress.progressItems;
        }

        public TutorialProgressItem GetItem(long id)
        {
            var item = progressItems.Find(x => x.Id == id);
            if (item == null)
            {
                item = new TutorialProgressItem() { Id = id };
                progressItems.Add(item);
            }
            return item;
        }
    }

    [Serializable]
    public class TutorialProgressItem
    {
        public long Id;
        public bool Done;
        public int IgnoreCount;
    }
}