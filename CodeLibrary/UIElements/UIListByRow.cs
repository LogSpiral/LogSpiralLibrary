using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.UIElements
{
    public class UIListByRow : UIElement, IEnumerable<UIElement>, IEnumerable
    {
        public delegate bool ElementSearchMethod(UIElement element);

        private class UIInnerList : UIElement
        {
            public override bool ContainsPoint(Vector2 point) => true;

            public override void DrawChildren(SpriteBatch spriteBatch)
            {
                Vector2 position = base.Parent.GetDimensions().Position();
                Vector2 dimensions = new(base.Parent.GetDimensions().Width, base.Parent.GetDimensions().Height);
                foreach (UIElement element in Elements)
                {
                    Vector2 position2 = element.GetDimensions().Position();
                    Vector2 dimensions2 = new(element.GetDimensions().Width, element.GetDimensions().Height);
                    if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
                        element.Draw(spriteBatch);
                }
            }

            public override Rectangle GetViewCullingArea() => base.Parent.GetDimensions().ToRectangle();
        }

        //TML: Made public instead of protected.
        public List<UIElement> _items = [];
        protected UIScrollbarByRow _scrollbar;
        //TML: Made internal instead of private.
        internal UIElement _innerList = new UIInnerList();
        private float _innerListWidth;
        public float ListPadding = 5f;
        public Action<List<UIElement>> ManualSortMethod;

        public int Count => _items.Count;

        public UIListByRow()
        {
            _innerList.OverflowHidden = false;
            _innerList.Width.Set(0f, 1f);
            _innerList.Height.Set(0f, 1f);
            OverflowHidden = true;
            Append(_innerList);
        }

        public float GetTotalWidth() => _innerListWidth;

        public void Goto(ElementSearchMethod searchMethod)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (searchMethod(_items[i]))
                {
                    _scrollbar.ViewPosition = _items[i].Top.Pixels;
                    break;
                }
            }
        }

        public virtual void Add(UIElement item)
        {
            _items.Add(item);
            _innerList.Append(item);
            UpdateOrder();
            _innerList.Recalculate();
        }
        public virtual void Insert(int index, UIElement item)
        {
            _items.Insert(index, item);
            _innerList.Append(item);
            UpdateOrder();
            _innerList.Recalculate();
        }
        public virtual bool Remove(UIElement item)
        {
            _innerList.RemoveChild(item);
            // If order is stable doesn't make sense to reorder, left because it's in vanilla
            UpdateOrder();
            return _items.Remove(item);
        }

        public virtual void Clear()
        {
            _innerList.RemoveAllChildren();
            _items.Clear();
        }

        public override void Recalculate()
        {
            base.Recalculate();
            UpdateScrollbar();
        }

        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            if (_scrollbar != null)
                _scrollbar.ViewPosition -= evt.ScrollWheelValue;
        }

        public override void RecalculateChildren()
        {
            base.RecalculateChildren();
            float num = 0f;
            for (int i = 0; i < _items.Count; i++)
            {
                float num2 = ((_items.Count == 1) ? 0f : ListPadding);
                _items[i].Left.Set(num, 0f);
                _items[i].Recalculate();
                num += _items[i].GetOuterDimensions().Width + num2;
            }

            _innerListWidth = num;
        }

        private void UpdateScrollbar()
        {
            if (_scrollbar != null)
            {
                float width = GetInnerDimensions().Width;
                _scrollbar.SetView(width, _innerListWidth);
            }
        }

        public void SetScrollbar(UIScrollbarByRow scrollbar)
        {
            _scrollbar = scrollbar;
            UpdateScrollbar();
        }

        public void UpdateOrder()
        {
            if (ManualSortMethod != null)
                ManualSortMethod(_items);
            else
                _items.Sort(SortMethod);

            UpdateScrollbar();
        }

        public int SortMethod(UIElement item1, UIElement item2) => item1.CompareTo(item2);

        public override List<SnapPoint> GetSnapPoints()
        {
            List<SnapPoint> list = [];
            if (GetSnapPoint(out var point))
                list.Add(point);

            foreach (UIElement item in _items)
            {
                list.AddRange(item.GetSnapPoints());
            }

            return list;
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (_scrollbar != null)
                _innerList.Left.Set(0f - _scrollbar.GetValue(), 0f);

            Recalculate();
        }

        public IEnumerator<UIElement> GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
    }
}
