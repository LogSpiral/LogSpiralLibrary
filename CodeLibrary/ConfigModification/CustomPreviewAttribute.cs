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

namespace LogSpiralLibrary.CodeLibrary.ConfigModification
{
    public interface ICustomConfigPreview
    {
        bool usePreview { get; }
        void Draw(SpriteBatch spriteBatch, ConfigElement element);
    }
    public abstract class SimplePreview<T> : ICustomConfigPreview
    {
        public virtual bool usePreview => true;

        public void Draw(SpriteBatch spriteBatch, ConfigElement element)
        {
            Vector2 topLeft = element.GetDimensions().ToRectangle().TopRight() + new Vector2(60, 0);
            Rectangle targetRectanle = new Rectangle((int)topLeft.X, (int)topLeft.Y, Math.Min(480, (int)(Main.screenWidth - topLeft.X) - 20), 240);

            ComplexPanelInfo panel = new ComplexPanelInfo
            {
                destination = targetRectanle,
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
            ConfigPreviewSystem.GetModConfigFromElement(element, out ModConfig modConfig);
            if (element.GetObject() is T instance)
                Draw(spriteBatch, targetRectanle, instance, modConfig);
        }
        public abstract void Draw(SpriteBatch spriteBatch, Rectangle drawRange, T data, ModConfig pendingConfig);
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
        static List<CustomConfigPreviewDelegate> delegates = new List<CustomConfigPreviewDelegate>();
        static List<Func<bool>> useRenderDelegate = new List<Func<bool>>();
        static HashSet<string> registeredDelegateName = new HashSet<string>();
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
            MonoModHooks.Add(previewDrawingMethod, PreviewDrawing);
            On_UIElement.GetClippingRectangle += UIElement_GetClippingRectangle;
            IL_Main.DoDraw += AddPreviewRenderOn;
            base.Load();
        }
        private void AddPreviewRenderOn(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            for (int n = 0; n < 5; n++)
                if (!cursor.TryGotoNext(i => i.MatchLdstr("Sepia")))
                    return;
            cursor.Index += 14;
            cursor.EmitDelegate(() =>
            {
                return !PVRenderUsing;
            });
            cursor.EmitAnd();
            for (int n = 0; n < 2; n++)
                if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(FilterManager).GetMethod(nameof(FilterManager.EndCapture), BindingFlags.Public | BindingFlags.Instance))))
                    return;
            cursor.Index -= 6;
            cursor.EmitDelegate<Func<bool, bool>>(flag =>
            {
                return flag && (!PVRenderUsing || Main.hideUI);
            });

            for (int n = 0; n < 2; n++)
                if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(Main).GetMethod(nameof(Main.DrawInterface), BindingFlags.NonPublic | BindingFlags.Instance))))
                    return;

            cursor.Index += 3;
            cursor.EmitDelegate(() =>
            {
                if (Lighting.NotRetro && PVRenderUsing)
                    Filters.Scene.EndCapture(null, Main.screenTarget, Main.screenTargetSwap, Color.Black);
            });
        }
        private static void PreviewDrawing(Action<ConfigElement, SpriteBatch> orig, ConfigElement self, SpriteBatch spriteBatch)
        {
            orig.Invoke(self, spriteBatch);
            var pvAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomPreviewAttribute>(self.MemberInfo, self.Item, self.List);
            if (pvAttribute != null && self.IsMouseHovering)
            {
                var drawer = (ICustomConfigPreview)Activator.CreateInstance(pvAttribute.pvType);
                if (drawer.usePreview)
                    drawer.Draw(spriteBatch, self);
            }
        }
        private Rectangle UIElement_GetClippingRectangle(On_UIElement.orig_GetClippingRectangle orig, UIElement self, SpriteBatch spriteBatch)
        {
            var origin = orig.Invoke(self, spriteBatch);
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
