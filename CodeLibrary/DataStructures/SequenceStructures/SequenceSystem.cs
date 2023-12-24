using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public class SequenceSystem : ModSystem
    {

        SequenceUI sequenceUI;
        UserInterface userInterfaceSequence;
        public override void Load()
        {
            sequenceUI = new SequenceUI();
            userInterfaceSequence = new UserInterface();
            sequenceUI.Activate();
            userInterfaceSequence.SetState(sequenceUI);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (SequenceUI.Visible)
                userInterfaceSequence?.Update(gameTime);
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
                   "LogSpiralLibrary:SequenceUI",
                   //这里是匿名方法
                   delegate
                   {
                       //当Visible开启时（当UI开启时）
                       if (SequenceUI.Visible)
                           //绘制UI（运行exampleUI的Draw方法）
                           sequenceUI.Draw(Main.spriteBatch);
                       return true;
                   },
                   //这里是绘制层的类型
                   InterfaceScaleType.UI)
               );
            }
            base.ModifyInterfaceLayers(layers);
        }
    }
    public class SequenceUI : UIState 
    {
        public static bool Visible = false;
        public override void OnActivate()
        {
            base.OnActivate();
        }
    }
}
