﻿using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
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

namespace LogSpiralLibrary.CodeLibrary.ConfigModification
{
    public interface ICustomConfigPreview
    {
        void Draw(SpriteBatch spriteBatch, ConfigElement element);
    }
    public abstract class SimplePreview<T> : ICustomConfigPreview
    {
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

            UIElement pare = element;
            while (pare != null && pare is not UIModConfig)
                pare = pare.Parent;
            ModConfig modConfig = null;
            if (pare is UIModConfig uIModConfig)
                modConfig = uIModConfig.pendingConfig;
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
    public class ConfigPreviewSystem : ModSystem
    {
        public override void Load()
        {
            var previewDrawingMethod = typeof(ConfigElement).GetMethod(nameof(ConfigElement.DrawSelf), BindingFlags.NonPublic | BindingFlags.Instance);
            MonoModHooks.Add(previewDrawingMethod, PreviewDrawing);
            On_UIElement.GetClippingRectangle += UIElement_GetClippingRectangle;

            base.Load();
        }
        private static void PreviewDrawing(Action<ConfigElement, SpriteBatch> orig, ConfigElement self, SpriteBatch spriteBatch)
        {
            orig.Invoke(self, spriteBatch);
            var pvAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomPreviewAttribute>(self.MemberInfo, self.Item, self.List);
            if (pvAttribute != null && self.IsMouseHovering)
            {
                //try
                //{
                //    self.Parent.Parent.Parent.OverflowHidden = false;
                //}
                //catch { }
                var drawer = (ICustomConfigPreview)Activator.CreateInstance(pvAttribute.pvType);
                drawer.Draw(spriteBatch, self);
            }
        }
        private Rectangle UIElement_GetClippingRectangle(On_UIElement.orig_GetClippingRectangle orig, UIElement self, SpriteBatch spriteBatch)
        {
            var origin = orig.Invoke(self, spriteBatch);
            UIElement element = self;//mainConfigList
            for (int n = 0; n < 3; n++)
                if (element.Parent != null)
                    element = element.Parent;//依次是 uIPanel uIElement this(UIModConfig)
                else
                    return origin;//如果没有就润
            if (element is not UIModConfig uIModConfig)
                return origin;//找错人了，润

            var type = uIModConfig.modConfig.GetType();
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

            base.Unload();
        }
    }
}