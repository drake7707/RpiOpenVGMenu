using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VGMenu.Screens;

namespace VGMenu.Screens.Menu
{
    public abstract class BaseMenuControl : Control
    {
        public List<MenuItem> Items { get; set; }

        public ShapeController.COLOR MenuBackgroundColor { get; set; }
        public ShapeController.COLOR MenuTextColor { get; set; }
        public ShapeController.COLOR MenuSelectedColor { get; set; }

        public System.Drawing.PointF Location { get; set; }
        public System.Drawing.SizeF Size { get; private set; }

        public float MenuHeight { get; private set; }
        public float MenuSpacing { get; private set; }


        public int ScrollOffset { get; set; }

        public System.Drawing.PointF ScrollBarLocation { get; set; }
        public System.Drawing.SizeF ScrollBarSize { get; set; }

        public bool HideScrollbar { get; private set; }

        protected IShapeMenuItem[] shapeItems;
        private ShapeController.SHAPE_RECT shapeScroll;
        private ShapeController.SHAPE_RECT shapeScrollThumb;

        public int Columns { get; set; }

        public BaseMenuControl(PointF location, SizeF size, float menuHeight, float menuSpacing, bool hideScrollbar = false)
        {
            Items = new List<MenuItem>();

            selectedIndex = -1;
            Columns = 1;

            this.Location = location;
            this.Size = size;
            this.MenuHeight = menuHeight;
            this.MenuSpacing = menuSpacing;
            this.HideScrollbar = hideScrollbar;


        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;

                if (selectedIndex == -1)
                    ScrollOffset = 0;
                else
                {
                    if (selectedIndex >= ScrollOffset + NrOfItemsVisible)
                        ScrollOffset = (selectedIndex - NrOfItemsVisible);
                    else if (selectedIndex < ScrollOffset)
                        ScrollOffset = selectedIndex;
                }

                UpdateShapes();
            }
        }


        public void Initialize()
        {
            CreateShapes();
        }

        public int NrOfItemsVisible
        {
            get
            {
                if (Size.Height < MenuHeight + MenuSpacing) // not enough space for even 1 row, keep drawing 1 row partially visible
                    return Columns;
                else
                    return Columns * (int)Math.Floor((Size.Height) / (MenuHeight + MenuSpacing));
            }
        }

        public void MoveSelectionDown()
        {
            MoveSelection(Columns);
        }

        public void MoveSelectionUp()
        {
            MoveSelection(-Columns);
        }

        public void MoveSelectionLeft()
        {
            MoveSelection(-1);
        }

        public void MoveSelectionRight()
        {
            MoveSelection(1);
        }

        private void MoveSelection(int val)
        {
            var oldIdx = selectedIndex;
            selectedIndex += val; ;

            if (selectedIndex >= Items.Count)
                selectedIndex = Items.Count - 1;
            if (selectedIndex < 0)
                selectedIndex = 0;

            if (oldIdx != selectedIndex)
            {
                if (selectedIndex < ScrollOffset * Columns)
                    ScrollUp();
                else if (selectedIndex >= ScrollOffset * Columns + NrOfItemsVisible)
                    ScrollDown();
                else
                    UpdateShapes();
            }

            LoggingLib.Logging.Add("VGMenu", "Active item: " + Items[selectedIndex].Text, LoggingLib.Logging.LoggingEnum.Info);
        }

        public void ScrollUp()
        {
            ScrollOffset--;

            if (ScrollOffset < 0)
                ScrollOffset = 0;

            UpdateShapes();
        }

        public void ScrollDown()
        {
            ScrollOffset++;

            int max = Items.Count - NrOfItemsVisible;
            if (ScrollOffset > max)
                ScrollOffset = max;

            UpdateShapes();
        }

        public override void Refresh()
        {
            UpdateShapes();
        }

        private void UpdateShapes()
        {
            for (int i = 0; i < NrOfItemsVisible + Columns; i++)
            {
                MenuItem mnuItm;
                bool highlighted;

                if (ScrollOffset * Columns + i >= Items.Count)
                    mnuItm = null;
                else
                    mnuItm = Items[ScrollOffset * Columns + i];

                highlighted = ScrollOffset * Columns + i == SelectedIndex;

                shapeItems[i].Update(this, i, mnuItm, highlighted);
            }

            if (!HideScrollbar)
            {
                // update scrollbar shapes
                var scrollBarRect = new System.Drawing.RectangleF(ScrollBarLocation.X, ScrollBarLocation.Y, ScrollBarSize.Width, ScrollBarSize.Height);

                var scrollBarTrackRect = GetScrollThumbRect();

                if (shapeScrollThumb.y != scrollBarTrackRect.Y)
                {
                    shapeScrollThumb.y = scrollBarTrackRect.Y;
                    ShapeController.SetRect(shapeScrollThumb);
                    //Console.WriteLine("Setting scroll thumb: " + shapeScrollThumb.x + "," + shapeScrollThumb.y + " - " + shapeScrollThumb.width + "x" + shapeScrollThumb.height);
                }
            }
        }

        private RectangleF GetScrollThumbRect()
        {
            var nrOfRows = (float)Math.Ceiling((float)Items.Count / Columns);
            float scrollbarPercHeight;
            float scrollbarPercTop;
            if (nrOfRows < ((float)NrOfItemsVisible / Columns))
            {
                scrollbarPercHeight = 1;
                scrollbarPercTop = 0;
            }
            else
            {
                scrollbarPercHeight = ((float)NrOfItemsVisible / Columns) / nrOfRows;
                scrollbarPercTop = (float)ScrollOffset / nrOfRows;
            }


            //var scrollbarPercTop = ScrollOffset / (float)Items.Count;
            //var scrollbarPercHeight = NrOfItemsVisible / (float)Items.Count;

            var scrollBarThumbRect = new System.Drawing.RectangleF(ScrollBarLocation.X, ScrollBarLocation.Y + ScrollBarSize.Height * scrollbarPercTop, ScrollBarSize.Width, ScrollBarSize.Height * scrollbarPercHeight);
            return scrollBarThumbRect;
        }

        private void CreateShapes()
        {
            shapeItems = new IShapeMenuItem[NrOfItemsVisible + Columns];

            var mnuWidth = (Size.Width) / Columns - MenuSpacing;

            for (int i = 0; i < NrOfItemsVisible + Columns; i++)
            {

                var col = i % Columns;
                var mnuLeft = Location.X + col * (mnuWidth + MenuSpacing);
                var row = i / Columns;
                var mnuTop = Location.Y + row * (MenuHeight + MenuSpacing);

                var rect = new System.Drawing.RectangleF(mnuLeft,
                                                         mnuTop,
                                                         mnuWidth, MenuHeight);

                bool highlighted = ScrollOffset * Columns + i == SelectedIndex;

                // update shape_text[i] to text

                shapeItems[i] = CreateMenuItem(i, rect, highlighted);

            }

            CreateScrollBar();
        }

        protected virtual IShapeMenuItem CreateMenuItem(int i, RectangleF rect, bool highlighted)
        {
            ShapeMenuItem itm = new ShapeMenuItem();
            itm.Create(this, i, rect, highlighted);
            return itm;
        }

        private void CreateScrollBar()
        {
            if (!HideScrollbar)
            {
                // update scrollbar shapes
                var scrollBarRect = new System.Drawing.RectangleF(ScrollBarLocation.X, ScrollBarLocation.Y, ScrollBarSize.Width, ScrollBarSize.Height);
                shapeScroll = new ShapeController.SHAPE_RECT()
                {
                    id = ShapeController.NewId(),
                    visible = true,
                    width = scrollBarRect.Width,
                    height = scrollBarRect.Height,
                    x = scrollBarRect.X,
                    y = scrollBarRect.Y,
                    backColor = MenuBackgroundColor
                };
                ShapeController.SetRect(shapeScroll);
                //Console.WriteLine("Setting scroll: " + shapeScroll.x + "," + shapeScroll.y + " - " + shapeScroll.width + "x" + shapeScroll.height);


                var scrollBarTrackRect = GetScrollThumbRect();
                shapeScrollThumb = new ShapeController.SHAPE_RECT()
                {
                    id = ShapeController.NewId(),
                    visible = true,
                    width = scrollBarTrackRect.Width,
                    height = scrollBarTrackRect.Height,
                    x = scrollBarTrackRect.X,
                    y = scrollBarTrackRect.Y,
                    backColor = MenuSelectedColor,

                };
                ShapeController.SetRect(shapeScrollThumb);
                //Console.WriteLine("Setting scroll thumb: " + shapeScrollThumb.x + "," + shapeScrollThumb.y + " - " + shapeScrollThumb.width + "x" + shapeScrollThumb.height);
            }
        }


        public override void Dispose()
        {
            foreach (var s in shapeItems)
                s.Dispose();

            if (!HideScrollbar)
            {
                ShapeController.Remove(shapeScroll.id);
                ShapeController.Remove(shapeScrollThumb.id);
            }
        }


        public override bool Visible
        {
            get { return base.Visible; }
            set
            {
                if (base.Visible != value)
                {
                    base.Visible = value;
                    foreach (var s in shapeItems)
                        s.Visible = base.Visible;

                    if (!HideScrollbar)
                    {
                        shapeScroll.visible = base.Visible;
                        ShapeController.SetRect(shapeScroll);

                        shapeScrollThumb.visible = base.Visible;
                        ShapeController.SetRect(shapeScrollThumb);
                    }
                }
            }
        }


        public override void OnCommand(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
            {
                MoveSelectionUp();
            }
            else if (key == ConsoleKey.DownArrow)
            {
                MoveSelectionDown();
            }
            if (key == ConsoleKey.LeftArrow)
            {
                MoveSelectionLeft();
            }
            else if (key == ConsoleKey.RightArrow)
            {
                MoveSelectionRight();
            }
        }
    }

}
