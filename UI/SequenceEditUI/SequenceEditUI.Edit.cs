using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UIBase;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
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
                    var panel = InsertablePanelUtils.ElementTypeToPanel(type);
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
                    var panel = InsertablePanelUtils.SequenceRefKeyToPanel(name);
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
        var sequence = SequenceGlobalManager.SequenceLookup[sequenceName];
        PropertyPanelData.Filler = new ObjectMetaDataFiller(sequence.Data);
        var rootPanel = InsertablePanelUtils.SequenceToInsertablePanel(sequence);
        rootPanel.BaseView = BasePanel;
        rootPanel.Mask = Mask;
        BasePanel.RootElement = rootPanel;
        rootPanel.Join(BasePanel);
        rootPanel.SetLeft(BasePanel.Bounds.Width * .25f);
        rootPanel.SetTop(BasePanel.Bounds.Height * .25f);
        PropertyPanelConfig.Filler = new NoneFiller();
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