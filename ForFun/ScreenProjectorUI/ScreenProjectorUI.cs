using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace LogSpiralLibrary.ForFun.ScreenProjectorUI
{
    public class ScreenProjectorPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ScreenProjectorSystem.ShowScreenProjectorKeybind.JustPressed)
            {
                if (ScreenProjectorUI.Visible)
                    ScreenProjectorSystem.instance.screenProjectorUI.Close();
                else
                    ScreenProjectorSystem.instance.screenProjectorUI.Open();
            }
            base.ProcessTriggers(triggersSet);
        }
    }
    public class ScreenProjectorSystem : ModSystem
    {
        public static ScreenProjectorSystem instance;
        public ScreenProjectorUI screenProjectorUI;
        public UserInterface userInterface;
        public static ModKeybind ShowScreenProjectorKeybind { get; private set; }
        public override void UpdateUI(GameTime gameTime)
        {
            if (ScreenProjectorUI.Visible)
            {
                userInterface?.Update(gameTime);
            }
            base.UpdateUI(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            //寻找一个名字为Vanilla: Mouse Text的绘制层，也就是绘制鼠标字体的那一层，并且返回那一层的索引
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            //寻找到索引时
            if (MouseTextIndex != -1)
            {
                //往绘制层集合插入一个成员，第一个参数是插入的地方的索引，第二个参数是绘制层
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                   //这里是绘制层的名字
                   "LogSpiralLibrary:ScreenProjectorUI",
                   //这里是匿名方法
                   delegate
                   {
                       //当Visible开启时（当UI开启时）
                       if (ScreenProjectorUI.Visible)
                           //绘制UI（运行exampleUI的Draw方法）
                           screenProjectorUI.Draw(Main.spriteBatch);
                       return true;
                   },
                   //这里是绘制层的类型
                   InterfaceScaleType.UI)
               );
            }
            base.ModifyInterfaceLayers(layers);
        }
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                instance = this;
                screenProjectorUI = new ScreenProjectorUI();
                userInterface = new UserInterface();
                screenProjectorUI.Activate();
                userInterface.SetState(screenProjectorUI);
                ShowScreenProjectorKeybind = KeybindLoader.RegisterKeybind(Mod, "ShowScreenProjector", "U");
                //On_UIElement.DrawSelf += On_UIElement_DrawSelf;
            }
            base.Load();
        }
        
    }
    public class DraggableButton : UIElement
    {
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), Color.White);
            base.DrawSelf(spriteBatch);
        }
        public bool Dragging;
        public Vector2 Offset;
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            Dragging = true;
            Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            Main.NewText("好");
            base.LeftMouseDown(evt);

        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            Dragging = false;

            base.LeftMouseUp(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (Dragging)
            {
                Left.Set(Main.mouseX - Offset.X, 0f);
                Top.Set(Main.mouseY - Offset.Y, 0f);
                Recalculate();
            }
            base.Update(gameTime);
        }
    }
    public class ScreenProjectorUI : UIState
    {
        public static Matrix QuadrangleToMatrix(Vector2[] vecs)
        {
            if (vecs.Length != 4) throw new ArgumentException($"向量的数量不对，需要四个，当前是{vecs.Length}个");
            Vector2 pos = vecs[0] + vecs[3] - vecs[1] - vecs[2];
            Vector2 i = vecs[1] - vecs[3];
            Vector2 j = vecs[2] - vecs[3];
            Matrix m1 = Matrix.Identity with { M11 = i.X, M12 = i.Y, M21 = j.X, M22 = j.Y };
            m1 = Matrix.Invert(m1);
            pos = Vector2.Transform(pos, m1);
            Vector3 X = new Vector3(vecs[1].X * (pos.X + 1) - vecs[0].X, vecs[2].X * (pos.Y + 1) - vecs[0].X, vecs[0].X);
            Vector3 Y = new Vector3(vecs[1].Y * (pos.X + 1) - vecs[0].Y, vecs[2].Y * (pos.Y + 1) - vecs[0].Y, vecs[0].Y);
            Matrix result = Matrix.Identity with
            {
                M11 = X.X,
                M21 = X.Y,
                M41 = X.Z,
                M12 = Y.X,
                M22 = Y.Y,
                M42 = Y.Z,
                M14 = pos.X,
                M24 = pos.Y
            };
            return result;

        }
        public static Matrix Transform =>
            QuadrangleToMatrix([((float)LogSpiralLibraryMod.ModTime / 60f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2() * new Vector2(1, 0) + ((float)LogSpiralLibraryMod.ModTime / 30f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2() * new Vector2(0, 1) + (-(float)LogSpiralLibraryMod.ModTime / 30f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2()  + (-(float)LogSpiralLibraryMod.ModTime / 60f).ToRotationVector2() * 200]) * Matrix.Invert(QuadrangleToMatrix([default, new Vector2(Main.screenWidth, 0), new Vector2(0, Main.screenHeight), Main.ScreenSize.ToVector2()]));

        //QuadrangleToMatrix([Main.ScreenSize.ToVector2() * .25f + ((float)LogSpiralLibraryMod.ModTime / 60f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2() * new Vector2(.75f,.25f) + ((float)LogSpiralLibraryMod.ModTime / 30f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2() * new Vector2(.25f, .75f) + (-(float)LogSpiralLibraryMod.ModTime / 30f).ToRotationVector2() * 200, Main.ScreenSize.ToVector2() * new Vector2(.75f) + (-(float)LogSpiralLibraryMod.ModTime / 60f).ToRotationVector2() * 200]) * Matrix.Invert(QuadrangleToMatrix([default, new Vector2(Main.screenWidth, 0), new Vector2(0, Main.screenHeight), Main.ScreenSize.ToVector2()]));

        //QuadrangleToMatrix([default, new Vector2(Main.screenWidth / 2, 0), new Vector2(0, Main.screenHeight), Main.ScreenSize.ToVector2() * new Vector2(.5f,.5f + ((float)m))]) * Matrix.Invert(QuadrangleToMatrix([default, new Vector2(Main.screenWidth, 0), new Vector2(0, Main.screenHeight), Main.ScreenSize.ToVector2()]));
        //QuadrangleToMatrix(ScreenProjectorSystem.instance.screenProjectorUI.targetVectors) * Matrix.Invert(QuadrangleToMatrix(ScreenProjectorSystem.instance.screenProjectorUI.origVectors));
        public static bool Visible;
        public Vector2[] origVectors = new Vector2[4];
        public Vector2[] targetVectors = new Vector2[4];
        public DraggableButton[] buttons = new DraggableButton[4];
        public bool selectingOrig;
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vec = buttons[i].GetDimensions().Center();
                if (selectingOrig)
                    origVectors[i] = vec;
                else
                    targetVectors[i] = vec;
            }

            base.Update(gameTime);
        }

        public override void OnInitialize()
        {
            UITextPanel<string> switchState = new UITextPanel<string>("切换状态");
            switchState.OnLeftClick += (evt, e) =>
            {
                selectingOrig ^= true;
                CombatText.NewText(Main.LocalPlayer.Hitbox, Color.White, "已切换为" + (selectingOrig ? "选取标准点" : "选取变换点"));
            };
            switchState.Left.Set(50, 0);
            switchState.Top.Set(500, 0);
            Append(switchState);
            for (int i = 0; i < 4; i++)
            {
                //  0,0|0   1,0|1
                //  0,1|3   1,1|2
                var btn = buttons[i] = new DraggableButton();
                btn.SetSize(new Vector2(16));
                btn.Left.Set(i % 2 * Main.screenWidth - 8, 0);//1.5f - Math.Abs(1.5f - i)
                btn.Top.Set(i / 2 * Main.screenHeight - 8, 0);
                Append(btn);
            }
            if (origVectors == null)
                origVectors = new Vector2[4];
            if (targetVectors == null)
                targetVectors = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 vec = buttons[i].GetDimensions().Center();
                origVectors[i] = vec;
                targetVectors[i] = vec;
            }
            base.OnInitialize();
        }
        public void Open()
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Visible = true;
            Elements.Clear();
            OnInitialize();
            Recalculate();
        }
        public void Close()
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            Visible = false;

        }
    }
}
