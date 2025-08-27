using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UIBase;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Option.Writers;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    private bool _pendingUpdateElementLib = true;
    private bool _pendingUpdateSequenceLib = true;
    internal Dictionary<string, Sequence> PendingSequences { get; } = [];
    internal Dictionary<string, InsertablePanel> PendingPanels { get; } = [];
    internal Dictionary<string, Sequence> OpenedSequences { get; } = [];
    internal Dictionary<string, InsertablePanel> OpenedPanels { get; } = [];
    public UIElementGroup SaveButton { get; set; }
    public UIElementGroup RevertButton { get; set; }
    public UIElementGroup SaveAsButton { get; set; }
    public UIElementGroup EditButtonContainer { get; set; }
    public UIElementGroup EditButtonMask { get; set; }
    private AnimationTimer _buttonContainerTimer = new();
    internal static bool AutoLoadingPanels { get; private set; }
    public PageView CurrentPage => (string.IsNullOrEmpty(_currentPageFullName) || !OpenedPages.TryGetValue(_currentPageFullName, out var page)) ? null : page;

    public void SetupElementLib()
    {
        if (!_pendingUpdateElementLib) return;

        ElementLibrary.Container.RemoveAllChildren();

        KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(KeyValuePair<string, Type> pair)
        {
            var name = pair.Key;
            var type = pair.Value;
            var strs = name.Split("/");
            var modName = strs[0];
            var dummy = ModContent.Find<MeleeAction>(name);
            InsertablePanelFactory factory = new()
            {
                PanelFactory = delegate
                {
                    AutoLoadingPanels = true;
                    var panel = InsertablePanelUtils.ElementTypeToPanel(type);
                    AutoLoadingPanels = false;
                    panel.BaseView = BasePanel;
                    panel.Mask = Mask;
                    return panel;
                }
            };
            var mask = SequenceEditUIHelper.NewDownlistMask();
            mask.OnUpdateStatus += delegate
            {
                SequenceEditUIHelper.HoverColor(mask, Color.Black * .2f, Color.White * .1f);
            };
            mask.Padding = new(0);
            factory.Join(mask);
            string categoryTitleName = dummy.Category;
            string key = $"Mods.{dummy.Mod.Name}.{CurrentCategory.ElementName}.Category.{dummy.Category}";
            if (Language.Exists(key))
                categoryTitleName = Language.GetTextValue(key);
            List<KeyValuePair<string, string>> path;
            if (!string.IsNullOrEmpty(dummy.Category))
                path =
                   [
                        new(modName,ModLoader.GetMod(modName).DisplayName),
                        new(dummy.Category,categoryTitleName)
                   ];
            else
                path =
                    [
                        new(modName,ModLoader.GetMod(modName).DisplayName)
                    ];

            return new(mask, path);
        }
        SUIFolder.BuildFoldersToTarget(ElementLibrary.Container, from pair in CurrentCategory.Maganger.ElementTypeLookup select SpawnContentFromPair(pair));
    }

    private void SetupSequenceLib()
    {
        if (!_pendingUpdateElementLib) return;

        SequenceLibrary.Container.RemoveAllChildren();

        // _pendingUpdateElementLib = false;
        KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(KeyValuePair<string, Sequence> pair)
        {
            var name = pair.Key;
            var type = pair.Value;
            var strs = name.Split("/");
            var modName = strs[0];
            InsertablePanelFactory factory = new()
            {
                PanelFactory = delegate
                {
                    AutoLoadingPanels = true;
                    var panel = InsertablePanelUtils.SequenceRefKeyToPanel(name);
                    AutoLoadingPanels = false;
                    panel.BaseView = BasePanel;
                    panel.Mask = Mask;
                    return panel;
                }
            };
            var mask = SequenceEditUIHelper.NewDownlistMask();
            mask.OnUpdateStatus += delegate
            {
                SequenceEditUIHelper.HoverColor(mask, Color.Black * .2f, Color.White * .1f);
            };
            mask.Padding = new(0);
            factory.Join(mask);
            List<KeyValuePair<string, string>> path = [new(modName, ModLoader.GetMod(modName).DisplayName)];
            var length = strs.Length;
            for (int i = 1; i < length - 1; i++)
            {
                var folderName = strs[i];
                path.Add(new(folderName, folderName));
            }
            return new(mask, path);
        }
        SUIFolder.BuildFoldersToTarget(SequenceLibrary.Container, from pair in CurrentCategory.Maganger.Sequences select SpawnContentFromPair(pair));
    }

    private void SetupRootElement()
    {
        var sequenceName = _currentPageFullName;
        if (!OpenedSequences.TryGetValue(sequenceName, out var sequence))
            OpenedSequences[sequenceName] = sequence = SequenceGlobalManager.SequenceLookup[sequenceName].Clone();
        PropertyPanelData.Filler = new ObjectMetaDataFiller(sequence.Data);


        AutoLoadingPanels = true;
        if (!OpenedPanels.TryGetValue(sequenceName, out var rootPanel))
            OpenedPanels[sequenceName] = rootPanel = InsertablePanelUtils.SequenceToInsertablePanel(sequence);
        AutoLoadingPanels = false;
        rootPanel.BaseView = BasePanel;
        rootPanel.Mask = Mask;
        BasePanel.RootElement = rootPanel;
        rootPanel.Join(BasePanel);
        rootPanel.SetLeft(BasePanel.Bounds.Width * .25f);
        rootPanel.SetTop(BasePanel.Bounds.Height * .25f);
        PropertyPanelConfig.Filler = new NoneFiller();
        CurrentEditTarget = null;
        var delegateWriter = new DelegateWriter();
        delegateWriter.OnWriteValue += (option, value, boradCast) =>
        {
            if (CurrentPage is not { } page) return;
            page.PendingModified = true;
            PendingSequences.TryAdd(_currentPageFullName, sequence);
            PendingPanels.TryAdd(_currentPageFullName, rootPanel);
        };

        var pendingWriter = new CombinedWriter(DefaultWriter.Instance, delegateWriter);
        PropertyPanelData.Writer = pendingWriter;
        PropertyPanelConfig.Writer = pendingWriter;

    }

    private void SwitchToEdit()
    {
        MenuPanel.Remove();
        MainContainer.Add(EditPanel, 1);
        BasePanel.RemoveAllChildren();
        // 防止预览过程中退出导致死锁
        InsertablePanel.ForceEnablePV();
        SetupElementLib();
        SetupSequenceLib();
        SetupRootElement();
    }
}