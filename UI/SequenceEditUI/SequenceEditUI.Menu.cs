using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UIBase;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    // [ExtendsFromMod(nameof(SilkyUIFramework))]
    private static class MenuHelper
    {
        public static PageView AppendPage(SequenceEditUI instance, string name, Sequence sequence, bool immediateSwitch)
        {
            void SwitchToPage(PageView page)
            {
                instance._currentPageFullName = name;
                instance.SwitchToEdit();
                page.BackgroundColor = Color.Black * .135f;
                var index = instance.PagePanel.GetInnerChildIndex(page);
                instance.PagePanel.Children[index - 1].BackgroundColor = default;
                instance.PagePanel.Children[index + 1].BackgroundColor = default;
                Recents.Remove(name);
                Recents.Insert(0, name);
                SaveRecentListAsFile();
            }
            var fullName = name;
            if (!instance.OpenedPages.TryGetValue(name, out var page))
            {
                page = new PageView
                {
                    TitleText = sequence.Data?.DisplayName ?? name,
                    NameIndex = name
                };
                var vr = new VerticalRule() { Height = new Dimension(-16, 0.8f), Top = new Anchor(-8f, 0, 1), BackgroundColor = Color.Black * .25f };
                instance.PagePanel.AddBefore(page, instance.CreateNewButton);
                instance.PagePanel.AddBefore(vr, instance.CreateNewButton);

                page.CloseButton.LeftMouseClick += delegate
                {
                    page.Remove();
                    vr.Remove();
                    instance.OpenedPages.Remove(name);
                    instance.OpenedPanels.Remove(name);
                    instance.OpenedSequences.Remove(name);
                    instance.PendingPanels.Remove(name);
                    instance.PendingSequences.Remove(name);
                    if (instance._currentPageFullName == name)
                        instance.SwitchToMenu();
                };
                page.LeftMouseClick += (source, evt) =>
                {
                    if (evt.Source != source) return;
                    SequenceEditUIHelper.RecoverPreviousActivePageColor(instance);
                    SwitchToPage(page);
                };
                page.OnUpdateStatus += delegate
                {
                    if (instance._currentPageFullName != name)
                        SequenceEditUIHelper.HoverColor(page, default, Color.White * .1f);
                };
                instance.OpenedPages[name] = page;
                if (immediateSwitch)
                    SwitchToPage(page);
            }
            else
                SwitchToPage(page);
            return page;
        }
        public static UIView SpawnViewFromPair(SequenceEditUI instance, KeyValuePair<string, Sequence> pair)
        {
            var sequence = pair.Value;
            var name = pair.Key;
            var mask = SequenceEditUIHelper.NewDownlistMask();

            var textView = new UITextView
            {
                Text = sequence.Data?.DisplayName ?? name,
                IgnoreMouseInteraction = true,
                Top = new Anchor(0, 0, .5f),
                TextAlign = new Vector2(0.5f)
            };
            textView.Join(mask);
            mask.OnUpdateStatus += delegate
            {
                //if (Favorites.Contains(name))
                //    SequenceEditUIHelper.HoverColor(mask, Color.DarkOrange * .2f, Color.LightYellow * .1f);
                //else
                SequenceEditUIHelper.HoverColor(mask, Color.Black * .2f, Color.White * .1f);

                if (Favorites.Contains(name))
                {
                    mask.BorderColor = SUIColor.Highlight * .25f;
                    mask.Border = 1;
                    textView.TextColor = Color.Yellow;
                }
                else
                {
                    mask.BorderColor = default;
                    mask.Border = 0;
                    textView.TextColor = Color.White;
                }
            };

            mask.LeftMouseClick += delegate
            {
                AppendPage(instance, name, sequence, false);
            };
            mask.RightMouseClick += delegate
            {
                AppendPage(instance, name, sequence, true);
            };
            return mask;
        }
        public static KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(SequenceEditUI instance, KeyValuePair<string, Sequence> pair)
        {
            var name = pair.Key;
            var pathArray = name.Split("/");
            var modName = pathArray[0];
            var mask = SpawnViewFromPair(instance, pair);
            mask.MiddleMouseClick += delegate
            {
                if (!Favorites.Remove(name))
                    Favorites.Add(name);
                SaveFavoriteListAsFile();
                instance.SetupMenuFavorites();
                SoundEngine.PlaySound(SoundID.Research);
                SoundEngine.PlaySound(SoundID.ResearchComplete);
            };
            List<KeyValuePair<string, string>> path = [new(modName, ModLoader.GetMod(modName).DisplayName)];
            int c = pathArray.Length;
            if (c > 2)
                for (int n = 2; n < c - 1; n++)
                    path.Add(new KeyValuePair<string, string>(pathArray[n], pathArray[n]));
            return new KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>>(mask, path);
        }
    }


    private bool _pendingUpdateMenu = true;

    private string? _currentPageFullName;
    public SequenceMenuPanel MenuPanel { get; } = new();
    private Dictionary<string, PageView> OpenedPages { get; } = [];

    private void SwitchToMenu()
    {
        SequenceEditUIHelper.RecoverPreviousActivePageColor(this);
        _currentPageFullName = null;
        EditPanel.Remove();
        MainContainer.Add(MenuPanel, 1);
        SetupMenu();
        // 防止预览过程中退出导致死锁
        InsertablePanel.ForceEnablePV();
    }

    private void SetupMenuRecents()
    {
        MenuPanel.RecentList.Container.RemoveAllChildren();
        foreach (var recents in Recents)
        {
            if (!CurrentCategory.Maganger.Sequences.TryGetValue(recents, out var sequence))
                continue;

            var view = MenuHelper.SpawnViewFromPair(this, new KeyValuePair<string, Sequence>(recents, sequence));
            view.Join(MenuPanel.RecentList.Container);
        }
    }

    private void SetupMenuFavorites()
    {
        MenuPanel.FavoriteList.Container.RemoveAllChildren();
        foreach (var favorite in Favorites)
        {
            if (!CurrentCategory.Maganger.Sequences.TryGetValue(favorite, out var sequence))
                continue;

            var view = MenuHelper.SpawnViewFromPair(this, new KeyValuePair<string, Sequence>(favorite, sequence));
            view.Join(MenuPanel.FavoriteList.Container);
        }
    }

    private void SetUpMenuFinished()
    {
        MenuPanel.FinishedList.Container.RemoveAllChildren();
        SUIFolder.BuildFoldersToTarget(
            MenuPanel.FinishedList.Container,
            from pair
            in CurrentCategory.Maganger.Sequences
            where pair.Value!.Data!.Finished
            select MenuHelper.SpawnContentFromPair(this, pair));
    }

    private void SetupMenuLibrary()
    {
        MenuPanel.LibraryList.Container.RemoveAllChildren();
        SUIFolder.BuildFoldersToTarget(
            MenuPanel.LibraryList.Container,
            from pair
            in CurrentCategory.Maganger.Sequences
            where !pair.Value!.Data!.Finished
            select MenuHelper.SpawnContentFromPair(this, pair));
    }
    private void SetupMenu()
    {
        if (!_pendingUpdateMenu) return;


        // 应当要挂起更新目录才重新生成，但是调试阶段姑且每次都生成吧
        // _pendingUpdateMenu = false;

        LoadRecentListFromFile();
        LoadFavoriteListFromFile();
        SetupMenuRecents();
        SetupMenuFavorites();
        SetUpMenuFinished();
        SetupMenuLibrary();
    }



}