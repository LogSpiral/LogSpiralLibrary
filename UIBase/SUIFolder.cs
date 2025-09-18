using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UIBase;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
public class SUIFolder : DownSlideListContainer
{
    public SUIFolder(string folderName)
    {
        List.Container.CrossAlignment = CrossAlignment.Start;
        List.Container.MainAlignment = MainAlignment.Start;
        var title = new UITextView();
        title.Text = folderName;
        Title.Add(title);
    }

    Dictionary<string, SUIFolder> SubFolders { get; } = [];

    public static void BuildFoldersToTarget(UIElementGroup targetPanel, IEnumerable<KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>>> contents)
    {
        Dictionary<string, SUIFolder> Folders = [];

        foreach (var content in contents)
        {
            var path = content.Value;
            if (path.Count == 0) continue;
            var pair = path[0];
            var folderName = pair.Key;
            var displayFolderName = pair.Value;
            if (!Folders.TryGetValue(folderName, out var folder))
            {
                folder = new SUIFolder(displayFolderName);
                targetPanel.Add(folder);
                Folders.Add(folderName, folder);

            }
            folder.AddElementByPathInternal(path, content.Key, 1, path.Count);
        }
    }

    public void AddElementByPath(IReadOnlyList<KeyValuePair<string, string>> path, UIView element)
    {
        AddElementByPathInternal(path, element, 0, path.Count);
    }


    void AddElementByPathInternal(IReadOnlyList<KeyValuePair<string, string>> path, UIView element, int depth, int maxDepth)
    {
        if (depth == maxDepth)
        {
            List.Container.Add(element);
            return;
        }
        var pair = path[depth];
        var folderName = pair.Key;
        var displayFolderName = pair.Value;
        if (!SubFolders.TryGetValue(folderName, out var folder))
        {
            folder = new SUIFolder(displayFolderName) { Width = new(-12, 1), Left = new(0, 0, 1) };
            folder.OnUpdateStatus += delegate
            {
                if (folder._expandTimer.IsUpdating)
                    ForcedUpdateHeight = true;
            };
            List.Container.Add(folder, SubFolders.Count);
            SubFolders.Add(folderName, folder);
        }

        folder.AddElementByPathInternal(path, element, depth + 1, maxDepth);
    }
}
