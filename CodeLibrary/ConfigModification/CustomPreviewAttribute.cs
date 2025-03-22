using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using MonoMod.Cil;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.UI;
using log4net.Filter;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace LogSpiralLibrary.CodeLibrary.ConfigModification
{
    public class PreviewAssistConfigElement : ConfigElement { }
    public struct OptionMetaData(PropertyFieldWrapper varibleInfo, object item, IList list, int index, object config)
    {
        public PropertyFieldWrapper varibleInfo = varibleInfo;
        public object item = item;
        public IList list = list;
        public int index = index;
        public object config = config;
        public readonly object Value
        {
            get => list != null ? list[index] : varibleInfo.GetValue(item);
            set

            {
                if (list != null)
                    list[index] = value;
                else
                    varibleInfo.SetValue(item, value);
            }
        }
        public readonly T GetAttribute<T>() where T : Attribute => ConfigManager.GetCustomAttributeFromMemberThenMemberType<T>(varibleInfo, item, list);
    }
    public interface ICustomConfigPreview
    {
        bool UsePreview { get; }
        void Draw(SpriteBatch spriteBatch, CalculatedStyle dimension, OptionMetaData metaData);
    }
    public abstract class SimplePreview<T> : ICustomConfigPreview
    {
        public virtual bool UsePreview => true;

        public void Draw(SpriteBatch spriteBatch, CalculatedStyle dimension, OptionMetaData metaData)
        {


            ComplexPanelInfo panel = new()
            {
                destination = dimension.ToRectangle(),
                StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value,
                glowEffectColor = Color.MediumPurple with { A = 102 },
                glowShakingStrength = .1f,
                glowHueOffsetRange = 0.1f,
                backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value,
                backgroundFrame = new Rectangle(4, 4, 28, 28),
                backgroundUnitSize = new Vector2(28, 28) * 2f,
                backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f
            };
            panel.DrawComplexPanel(spriteBatch);
            //SDFGraphics.HasBorderRoundedBox(dimension.Position(), default, new Vector2(dimension.Width, dimension.Height), new Vector4(12f), UICommon.DefaultUIBlue * .5f, 4, Color.Black, SDFGraphics.GetMatrix(true));
            if (metaData.Value is T instance)
                Draw(spriteBatch, dimension, instance, metaData);
        }
        public abstract void Draw(SpriteBatch spriteBatch, CalculatedStyle dimension, T data, OptionMetaData metaData);
    }
    public class CustomPreviewAttribute : Attribute
    {
        public Type pvType;
        protected CustomPreviewAttribute(Type type)
        {
            pvType = type;
        }
    }
    public class CustomPreviewAttribute<T> : CustomPreviewAttribute where T : ICustomConfigPreview
    {
        public CustomPreviewAttribute() : base(typeof(T)) { }
    }
    public class HorizonOverflowEnableAttribute : Attribute { }
    public class RenderDrawingPreviewNeededAttribute : Attribute { }
    public class ConfigPreviewSystem : ModSystem
    {
        public delegate void CustomConfigPreviewDelegate(UIElement element, out ModConfig modConfig);
        static List<CustomConfigPreviewDelegate> delegates = [];
        static List<Func<bool>> useRenderDelegate = [];
        static HashSet<string> registeredDelegateName = [];
        public static void ConfigSettingRegister(CustomConfigPreviewDelegate func, Func<bool> useRender, string funcName)
        {
            if (registeredDelegateName.Contains(funcName))
            {
                throw new Exception("已经添加过了这个委托");
            }
            delegates.Add(func);
            useRenderDelegate.Add(useRender);
            registeredDelegateName.Add(funcName);
        }
        public static void GetModConfigFromElement(UIElement element, out ModConfig modConfig)
        {
            modConfig = null;
            UIElement pare = element;
            while (pare != null && pare is not UIModConfig)
                pare = pare.Parent;
            if (pare is UIModConfig uIModConfig)
                modConfig = uIModConfig.pendingConfig;
            if (modConfig == null)
                foreach (var func in delegates)
                {
                    func?.Invoke(element, out modConfig);
                    if (modConfig != null)
                        break;
                }
        }
        static bool ExtraRenderUsingCondition()
        {
            //return false;
            foreach (var func in useRenderDelegate)
                if (func?.Invoke() == true)
                    return true;
            return false;
        }
        public static bool PVRenderUsing =>
            LogSpiralLibraryMiscConfig.Instance.WTHConfig ||
            ((Main.gameMenu ? Main.MenuUI : Main.InGameUI).CurrentState == Interface.modConfig && Interface.modConfig?.modConfig?.GetType()?.GetCustomAttribute<RenderDrawingPreviewNeededAttribute>() != null) ||
            ExtraRenderUsingCondition();

        public override void Load()
        {

            Filters.Scene.OnPostDraw += () => { };

            var previewDrawingMethod = typeof(ConfigElement).GetMethod(nameof(ConfigElement.DrawSelf), BindingFlags.NonPublic | BindingFlags.Instance);
            MonoModHooks.Add(previewDrawingMethod, PreviewDrawing_Hook);
            On_UIElement.GetClippingRectangle += UIElement_GetClippingRectangle;
            //if (ModLoader.TryGetMod("ImproveGame", out var qot))
            //    qot.Call("AddRenderOnCondition", () => PVRenderUsing);
            //else
            Main.QueueMainThreadAction(() => IL_Main.DoDraw += AddPreviewRenderOn);
           
            base.Load();
        }
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("ImproveGame", out var qot))
            {
                var assembly = qot.GetType().Assembly;
                ImproveGame_ModernConfigCrossModHelper.OnGlobalConfigPreview(qot, (UIElement element, ModConfig currentConfig, PropertyFieldWrapper varibleInfo, object item, IList list, int index) =>
                {
                    OptionMetaData metaData = new(varibleInfo, item, list, index, currentConfig);
                    var pvAttribute = metaData.GetAttribute<CustomPreviewAttribute>();
                    if (pvAttribute != null)
                        ConfigPreviewSystem.PreviewDrawing(pvAttribute, element.GetDimensions(), metaData);
                });
                var tooltipPanelType = assembly.GetType("ImproveGame.UI.ModernConfig.TooltipPanel");
                var tooltipPanelInstanceFld = tooltipPanelType.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                var currentOptionFld = tooltipPanelType.GetField("currentOption", BindingFlags.Public | BindingFlags.Instance);
                var mainUIType = assembly.GetType("ImproveGame.UI.ModernConfig.ModernConfigUI");
                var mainUIInstanceProp = mainUIType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var enabledFld = mainUIType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                var modernConfigOptionType = assembly.GetType("ImproveGame.UI.ModernConfig.OptionElements.ModernConfigOption");
                var configProp = modernConfigOptionType.GetProperty("Config", BindingFlags.Public | BindingFlags.Instance);
                useRenderDelegate.Add(() =>
                {
                    var mainUIInstance = mainUIInstanceProp?.GetValue(null);
                    if (mainUIInstance == null)
                        return false;

                    var enableFlag = enabledFld?.GetValue(mainUIInstance);
                    if (enableFlag is not true)
                        return false;

                    var panelInstance = tooltipPanelInstanceFld?.GetValue(null);
                    if (panelInstance == null)
                        return false;

                    var option = currentOptionFld?.GetValue(panelInstance);
                    if (option == null)
                        return false;

                    var config = configProp?.GetValue(option);
                    if (config == null || config.GetType().GetCustomAttribute<RenderDrawingPreviewNeededAttribute>() == null)
                        return false;

                    return true;
                });

            }
            base.PostSetupContent();
        }
        private void AddPreviewRenderOn(ILContext il)
        {
            //这部分代码负责在主页面开启screenTarget捕获
            ILCursor cursor = new(il);

            //"Sepia"是饥荒世界的滤镜，这里世界生成的时候也会开启，这里用for查找到最后一个
            for (int n = 0; n < 5; n++)
                if (!cursor.TryGotoNext(i => i.MatchLdstr("Sepia")))
                    return;
            //神人螺线直接Index+=14了，这里是导航到or指令前面
            //具体说来是drawToScreen || netMode == 2 || flag
            //这里只要有一个成立就不会开启screenTarget
            //其中flag表示  不启用饥荒滤镜
            cursor.Index += 14;
            cursor.EmitDelegate(() =>
            {
                return !PVRenderUsing;
            });
            cursor.EmitAnd();
            //↑这里我加入了一个 *不启用设置预览的Render绘制*然后取与
            //也就是说如果既不要饥荒滤镜也不要螺线光污染就不开screenTarget捕获，很合理
            

            //下面这部分代码负责在游戏内时等UI绘制完毕再结束屏幕捕获

            //这里是先找寻到游戏内结束捕获的函数
            //找寻两次是因为第一次是主页面内结束捕获
            for (int n = 0; n < 2; n++)
                if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(FilterManager).GetMethod(nameof(FilterManager.EndCapture), BindingFlags.Public | BindingFlags.Instance))))
                    return;
            //调用函数前会压一堆值到栈里面，所以得往前找一段距离
            cursor.Index -= 6;
            
            //会判定在屏幕捕获状态时才结束捕获，这里我加了个条件来取消结束捕获
            cursor.EmitDelegate<Func<bool, bool>>(flag =>
            {
                return flag && (!PVRenderUsing || Main.hideUI);
            });

            //两次是因为有一次写在try里面有一次写在catch里面
            for (int n = 0; n < 2; n++)
                if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(Main).GetMethod(nameof(Main.DrawInterface), BindingFlags.NonPublic | BindingFlags.Instance))))
                    return;

            //保证DrawInterface已经执行完了，而后EndCapture
            cursor.Index += 3;
            cursor.EmitDelegate(() =>
            {
                if (Lighting.NotRetro && PVRenderUsing)
                    Filters.Scene.EndCapture(null, Main.screenTarget, Main.screenTargetSwap, Color.Black);
            });
        }
        private static void PreviewDrawing_Hook(Action<ConfigElement, SpriteBatch> orig, ConfigElement self, SpriteBatch spriteBatch)
        {
            orig.Invoke(self, spriteBatch);
            GetModConfigFromElement(self, out var modConfig);
            PreviewDrawing(self, modConfig);

        }
        public static void PreviewDrawing(ConfigElement self, ModConfig pendingConfig, bool forcedDrawing = false)
        {
            var pvAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomPreviewAttribute>(self.MemberInfo, self.Item, self.List);
            if (pvAttribute == null || !(forcedDrawing || self.IsMouseHovering)) return;

            var dimension = self.GetDimensions();
            Vector2 topLeft = new(60 + dimension.X + dimension.Width, Main.mouseY);
            dimension = new(topLeft.X, topLeft.Y, Math.Min(480, Main.screenWidth - topLeft.X - 20), 240);
            PreviewDrawing(pvAttribute, dimension, new(self.MemberInfo, self.Item, self.List, self.Index, pendingConfig));

        }
        public static void PreviewDrawing(CustomPreviewAttribute previewAttribute, CalculatedStyle dimension, OptionMetaData metaData)
        {
            var spriteBatch = Main.spriteBatch;
            var rect = Main.instance.GraphicsDevice.ScissorRectangle;
            Main.spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            var drawer = (ICustomConfigPreview)Activator.CreateInstance(previewAttribute.pvType);
            if (drawer.UsePreview)
                drawer.Draw(spriteBatch, dimension, metaData);
            Main.spriteBatch.End();
            Main.instance.GraphicsDevice.ScissorRectangle = rect;
            Main.instance.GraphicsDevice.RasterizerState = UIElement.OverflowHiddenRasterizerState;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, UIElement.OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
        }
        private Rectangle UIElement_GetClippingRectangle(On_UIElement.orig_GetClippingRectangle orig, UIElement self, SpriteBatch spriteBatch)
        {
            var origin = orig.Invoke(self, spriteBatch);
            return origin;
            //UIElement element = self;//mainConfigList
            //for (int n = 0; n < 3; n++)
            //    if (element.Parent != null)
            //        element = element.Parent;//依次是 uIPanel uIElement this(UIModConfig)
            //    else
            //        return origin;//如果没有就润
            //if (element is not UIModConfig uIModConfig)
            //    return origin;//找错人了，润
            if (self is not UIList)
                return origin;
            GetModConfigFromElement(self, out var modConfig);
            if (modConfig == null)
                return origin;
            var type = modConfig.GetType();
            var attribute = type.GetCustomAttribute<HorizonOverflowEnableAttribute>();
            if (attribute != null)
            {
                var rect = Main.instance.GraphicsDevice.ScissorRectangle;

                return rect with { Y = origin.Y, Height = origin.Height };
            }

            return origin;
        }
        public override void Unload()
        {
            On_UIElement.GetClippingRectangle -= UIElement_GetClippingRectangle;
            IL_Main.DoDraw -= AddPreviewRenderOn;
            base.Unload();
        }
    }
}
