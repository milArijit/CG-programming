using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GrafPack
{
    public partial class GrafPack : Form
    {
        private MainMenu mainMenu;
        private MenuItem transformItem, moveItem, rotateItem, deleteItem;

        // Modes
        private bool selectSquareStatus = false;
        private bool selectTriangleStatus = false;
        private bool selectCircleStatus = false;
        private bool selectMode = false;
        private bool moveMode = false;
        private bool rotateMode = false;

        private int clicknumber = 0;
        private Point one;
        private Point two;
        private Point lastMousePos;

        private List<Shape> shapesList = new List<Shape>();
        private Shape selectedShape = null;

        public GrafPack()
        {
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true); // Prevents flickering
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            mainMenu = new MainMenu();
            MenuItem createItem = new MenuItem("&Create");
            MenuItem selectItem = new MenuItem("&Select");

            MenuItem squareItem = new MenuItem("&Square");
            MenuItem triangleItem = new MenuItem("&Triangle");
            MenuItem circleItem = new MenuItem("&Circle");

            // New Menu Items
            transformItem = new MenuItem("&Transform");
            moveItem = new MenuItem("&Move");
            rotateItem = new MenuItem("&Rotate");
            deleteItem = new MenuItem("&Delete");

            mainMenu.MenuItems.Add(createItem);
            mainMenu.MenuItems.Add(selectItem);
            mainMenu.MenuItems.Add(transformItem);
            mainMenu.MenuItems.Add(deleteItem);

            createItem.MenuItems.Add(squareItem);
            createItem.MenuItems.Add(triangleItem);
            createItem.MenuItems.Add(circleItem);

            transformItem.MenuItems.Add(moveItem);
            transformItem.MenuItems.Add(rotateItem);

            // Event Subscriptions
            selectItem.Click += selectShapeMenu;
            squareItem.Click += selectSquare;
            triangleItem.Click += selectTriangle;
            circleItem.Click += selectCircle;
            moveItem.Click += setMoveMode;
            rotateItem.Click += setRotateMode;
            deleteItem.Click += deleteSelectedShape;

            // Initially disable transform/delete until a shape is selected
            transformItem.Enabled = false;
            deleteItem.Enabled = false;

            this.Menu = mainMenu;
            this.MouseDown += handleMouseDown;
            this.MouseMove += handleMouseMove;
            this.MouseUp += handleMouseUp;
        }

        private void ResetModes()
        {
            selectSquareStatus = false;
            selectTriangleStatus = false;
            selectCircleStatus = false;
            selectMode = false;
            moveMode = false;
            rotateMode = false;
            clicknumber = 0;
        }

        private void selectCircle(object sender, EventArgs e)
        {
            ResetModes();
            selectCircleStatus = true;
            MessageBox.Show("Circle tool active. Click once for the center, and once for the edge.");
        }

        private void selectSquare(object sender, EventArgs e)
        {
            ResetModes();
            selectSquareStatus = true;
            MessageBox.Show("Square tool active. Click twice to create a square.");
        }

        private void selectTriangle(object sender, EventArgs e)
        {
            ResetModes();
            selectTriangleStatus = true;
            MessageBox.Show("Triangle tool active. Click twice to create a triangle.");
        }

        private void selectShapeMenu(object sender, EventArgs e)
        {
            ResetModes();
            selectMode = true;
            MessageBox.Show("Select mode active. Click on a shape to select it.");
        }

        private void setMoveMode(object sender, EventArgs e)
        {
            if (selectedShape == null) return;
            ResetModes();
            moveMode = true;
            MessageBox.Show("Move mode active. Drag the selected shape with your mouse.");
        }

        private void setRotateMode(object sender, EventArgs e)
        {
            if (selectedShape == null) return;
            ResetModes();
            rotateMode = true;
            MessageBox.Show("Rotate mode active. Drag left/right to rotate.");
        }

        private void deleteSelectedShape(object sender, EventArgs e)
        {
            if (selectedShape != null)
            {
                shapesList.Remove(selectedShape);
                selectedShape = null;
                transformItem.Enabled = false;
                deleteItem.Enabled = false;
                this.Invalidate();
            }
        }

        private void handleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            lastMousePos = e.Location;

            if (selectMode)
            {
                selectedShape = null;
                // Reverse loop so we select the shape drawn on top
                for (int i = shapesList.Count - 1; i >= 0; i--)
                {
                    if (shapesList[i].Contains(e.Location))
                    {
                        selectedShape = shapesList[i];
                        break;
                    }
                }

                // Enable sub-menus if a shape was selected
                bool hasSelection = (selectedShape != null);
                transformItem.Enabled = hasSelection;
                deleteItem.Enabled = hasSelection;
                this.Invalidate();
            }
            else if (selectSquareStatus || selectTriangleStatus || selectCircleStatus)
            {
                if (clicknumber == 0)
                {
                    one = e.Location;
                    clicknumber = 1;
                }
                else
                {
                    two = e.Location;
                    clicknumber = 0;

                    if (selectSquareStatus)
                        shapesList.Add(new Square(one, two));
                    else if (selectTriangleStatus)
                        shapesList.Add(new Triangle(one, two));
                    else if (selectCircleStatus)
                        shapesList.Add(new Circle(one, two));

                    ResetModes(); // Reset after drawing
                    this.Invalidate();
                }
            }
        }

        private void handleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selectedShape != null)
            {
                if (moveMode)
                {
                    int dx = e.X - lastMousePos.X;
                    int dy = e.Y - lastMousePos.Y;
                    selectedShape.Move(dx, dy);
                    lastMousePos = e.Location;
                    this.Invalidate();
                }
                else if (rotateMode)
                {
                    // Use horizontal mouse movement to dictate rotation angle
                    int dx = e.X - lastMousePos.X;
                    if (dx != 0)
                    {
                        selectedShape.Rotate(dx); // 1 pixel = 1 degree
                        lastMousePos = e.Location;
                        this.Invalidate();
                    }
                }
            }
        }

        private void handleMouseUp(object sender, MouseEventArgs e)
        {
            // Optional: reset drag state if necessary
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Pen blackPen = new Pen(Color.Black, 2);
            Pen highlightPen = new Pen(Color.Red, 2); // Highlight for selected shape

            foreach (Shape s in shapesList)
            {
                s.draw(g, s == selectedShape ? highlightPen : blackPen);
            }

            blackPen.Dispose();
            highlightPen.Dispose();
        }
    }

    abstract class Shape
    {
        public Shape() { }
        public abstract void draw(Graphics g, Pen pen);
        public abstract bool Contains(Point p);
        public abstract void Move(int dx, int dy);
        public abstract void Rotate(float angleDegrees);

        // Helper to rotate a single point around a center point
        protected Point RotatePoint(Point pt, Point center, float angleDegrees)
        {
            double angleRadians = angleDegrees * (Math.PI / 180.0);
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            int x = (int)(cosTheta * (pt.X - center.X) - sinTheta * (pt.Y - center.Y) + center.X);
            int y = (int)(sinTheta * (pt.X - center.X) + cosTheta * (pt.Y - center.Y) + center.Y);

            return new Point(x, y);
        }
    }

    class Square : Shape
    {
        Point keyPt, oppPt;

        public Square(Point keyPt, Point oppPt)
        {
            this.keyPt = keyPt;
            this.oppPt = oppPt;
        }

        public override void draw(Graphics g, Pen pen)
        {
            double xDiff = oppPt.X - keyPt.X;
            double yDiff = oppPt.Y - keyPt.Y;
            double xMid = (oppPt.X + keyPt.X) / 2;
            double yMid = (oppPt.Y + keyPt.Y) / 2;

            g.DrawLine(pen, (int)keyPt.X, (int)keyPt.Y, (int)(xMid + yDiff / 2), (int)(yMid - xDiff / 2));
            g.DrawLine(pen, (int)(xMid + yDiff / 2), (int)(yMid - xDiff / 2), (int)oppPt.X, (int)oppPt.Y);
            g.DrawLine(pen, (int)oppPt.X, (int)oppPt.Y, (int)(xMid - yDiff / 2), (int)(yMid + xDiff / 2));
            g.DrawLine(pen, (int)(xMid - yDiff / 2), (int)(yMid + xDiff / 2), (int)keyPt.X, (int)keyPt.Y);
        }

        public override void Move(int dx, int dy)
        {
            keyPt.X += dx; keyPt.Y += dy;
            oppPt.X += dx; oppPt.Y += dy;
        }

        public override void Rotate(float angleDegrees)
        {
            Point center = new Point((keyPt.X + oppPt.X) / 2, (keyPt.Y + oppPt.Y) / 2);
            keyPt = RotatePoint(keyPt, center, angleDegrees);
            oppPt = RotatePoint(oppPt, center, angleDegrees);
        }

        public override bool Contains(Point p)
        {
            // Simple bounding circle check for hit testing ease
            Point center = new Point((keyPt.X + oppPt.X) / 2, (keyPt.Y + oppPt.Y) / 2);
            double radius = Math.Sqrt(Math.Pow(oppPt.X - center.X, 2) + Math.Pow(oppPt.Y - center.Y, 2));
            return Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2)) <= radius;
        }
    }

    class Triangle : Shape
    {
        Point topPt, botRightPt, botLeftPt;

        public Triangle(Point top, Point botRight)
        {
            this.topPt = top;
            this.botRightPt = botRight;
            int width = botRightPt.X - topPt.X;
            this.botLeftPt = new Point(topPt.X - width, botRightPt.Y);
        }

        public override void draw(Graphics g, Pen pen)
        {
            g.DrawLine(pen, topPt.X, topPt.Y, botRightPt.X, botRightPt.Y);
            g.DrawLine(pen, botRightPt.X, botRightPt.Y, botLeftPt.X, botLeftPt.Y);
            g.DrawLine(pen, botLeftPt.X, botLeftPt.Y, topPt.X, topPt.Y);
        }

        public override void Move(int dx, int dy)
        {
            topPt.X += dx; topPt.Y += dy;
            botRightPt.X += dx; botRightPt.Y += dy;
            botLeftPt.X += dx; botLeftPt.Y += dy;
        }

        public override void Rotate(float angleDegrees)
        {
            Point center = new Point((topPt.X + botRightPt.X + botLeftPt.X) / 3,
                                     (topPt.Y + botRightPt.Y + botLeftPt.Y) / 3);
            topPt = RotatePoint(topPt, center, angleDegrees);
            botRightPt = RotatePoint(botRightPt, center, angleDegrees);
            botLeftPt = RotatePoint(botLeftPt, center, angleDegrees);
        }

        public override bool Contains(Point p)
        {
            // Simple bounding box check
            int minX = Math.Min(topPt.X, Math.Min(botRightPt.X, botLeftPt.X));
            int maxX = Math.Max(topPt.X, Math.Max(botRightPt.X, botLeftPt.X));
            int minY = Math.Min(topPt.Y, Math.Min(botRightPt.Y, botLeftPt.Y));
            int maxY = Math.Max(topPt.Y, Math.Max(botRightPt.Y, botLeftPt.Y));
            return p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;
        }
    }

    class Circle : Shape
    {
        Point center;
        int radius;

        public Circle(Point center, Point edge)
        {
            this.center = center;
            this.radius = (int)Math.Sqrt(Math.Pow(edge.X - center.X, 2) + Math.Pow(edge.Y - center.Y, 2));
        }

        public override void draw(Graphics g, Pen pen)
        {
            int x = 0;
            int y = radius;
            int d = 3 - 2 * radius;

            while (y >= x)
            {
                PutPixel(g, pen, center.X + x, center.Y + y);
                PutPixel(g, pen, center.X - x, center.Y + y);
                PutPixel(g, pen, center.X + x, center.Y - y);
                PutPixel(g, pen, center.X - x, center.Y - y);
                PutPixel(g, pen, center.X + y, center.Y + x);
                PutPixel(g, pen, center.X - y, center.Y + x);
                PutPixel(g, pen, center.X + y, center.Y - x);
                PutPixel(g, pen, center.X - y, center.Y - x);

                x++;
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }
            }
        }

        private void PutPixel(Graphics g, Pen pen, int x, int y)
        {
            g.FillRectangle(pen.Brush, x, y, 2, 2);
        }

        public override void Move(int dx, int dy)
        {
            center.X += dx;
            center.Y += dy;
        }

        public override void Rotate(float angleDegrees)
        {
            // A perfect circle visually doesn't change when rotated around its center.
            // If you wanted to rotate it around a different pivot, you would apply logic here.
        }

        public override bool Contains(Point p)
        {
            return Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2)) <= radius;
        }
    }
}