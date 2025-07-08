using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel;

public interface IPropertyPanelHandler
{
    void SetValue();
    void BuildOptionList();

    void HandleNotSupport();
    void HandleMiddleClick();
    void HandleRightClick();

    LocalizedText GetLabel();
    LocalizedText GetDescription();

    void DisplayDescription();

    bool Interactable();


    object CloneObject(object source);
}
