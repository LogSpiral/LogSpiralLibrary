using SilkyUIFramework.BasicElements;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
namespace LogSpiralLibrary.CodeLibrary.UIFramework;

public class UINumericTextView : SUIEditText
{
    public string Format = "0.00";
    public double DefaultValue = 0;
    public double MinValue = 0;
    public double MaxValue = 1;

    public bool IsValueSafe => double.TryParse(Text, out _);

    public double Value
    {
        get => double.Parse(Text);
        set => Text = value.ToString(Format);
    }

    public UINumericTextView()
    {
        // 删除所有非数字字符，保留数字和小数点（俄语小数点是逗号，也是挺神奇的）

        // 结束输入时检查是否在范围内
        OnEnterKeyDown += () =>
        {
            Text = new string([.. Text.Where(c => char.IsDigit(c) || c is '.' or '-' or ',')]);
            Value = !double.TryParse(Text, out double digit)
                ? DefaultValue
                : Math.Clamp(digit, MinValue, MaxValue);
        };
    }
}
