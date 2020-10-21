using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Raycast
{
    public partial class Form1 : Form
    {
        private Graphics gfx;
        private Bitmap canvas;
        private Random gen;
        private List<Line> barriers;
        private const float end = (float)(Math.PI * 2);
        private const float angleStep = 0.1f;
        private const int rayScaleDist = 5000;
        private const int numBarriers = 5;
        private int minIntersectionDistance;
        private Point mouse;
        private float currentAngle;
        private Line ray;
        private int dist;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gfx = Graphics.FromImage(canvas);
            gen = new Random();

            // Add borders of the form.
            barriers = new List<Line>()
            {
                new Line(0,1,canvas.Width, 1), // top
                new Line(0, pictureBox1.Height - 1, canvas.Width, pictureBox1.Height - 1), // bottom
                new Line(0,0, 0, pictureBox1.Height), // left
                new Line(pictureBox1.Width - 1, 0, pictureBox1.Width - 1, pictureBox1.Height)  // right
            };

            // Add the random lines that will act as additional barriers.
            for (int i = 0; i < numBarriers; i++)
            {
                int startX = gen.Next(0, canvas.Width);
                int endX = gen.Next(0, canvas.Width);
                int startY = gen.Next(0, canvas.Height);
                int endY = gen.Next(0, canvas.Height);
                barriers.Add(new Line(startX, startY, endX, endY));
            }
        }

        // Cast rays around the circle going from 0 to 2 pi.
        // Check for collision, and the ray's end point will be the closest intersection.
        // Draw the rays and the barriers.
        private void Timer1_Tick(object sender, EventArgs e)
        {
            gfx.Clear(Color.Black);

            mouse = pictureBox1.PointToClient(Cursor.Position);

            for (currentAngle = 0; currentAngle < end; currentAngle += angleStep)
            {
                ray = new Line(mouse.X, mouse.Y, mouse.X + (int)(Math.Cos(currentAngle) * rayScaleDist), mouse.Y + (int)(Math.Sin(currentAngle) * rayScaleDist));
                minIntersectionDistance = int.MaxValue;

                foreach (var barrier in barriers)
                {
                    if (LineIntersection(ray, barrier, out Point intersectPoint))
                    {
                        dist = DistanceSquared(ray.X1, ray.Y1, intersectPoint.X, intersectPoint.Y);
                        if (dist < minIntersectionDistance)
                        {
                            minIntersectionDistance = dist;

                            ray.X2 = intersectPoint.X;
                            ray.Y2 = intersectPoint.Y;
                        }
                    }
                }

                gfx.DrawLine(Pens.White, ray.X1, ray.Y1, ray.X2, ray.Y2);
            }

            foreach (var barrier in barriers)
            {
                gfx.DrawLine(Pens.White, barrier.X1, barrier.Y1, barrier.X2, barrier.Y2);
            }
            pictureBox1.Image = canvas;
        }

        // Found at https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        private bool LineIntersection(Line ray, Line barrier, out Point intersectionPoint)
        {
            intersectionPoint = default;
            int den = ((ray.X1 - ray.X2) * (barrier.Y1 - barrier.Y2)) - ((ray.Y1 - ray.Y2) * (barrier.X1 - barrier.X2));
            float t = ((ray.X1 - barrier.X1) * (barrier.Y1 - barrier.Y2)) - ((ray.Y1 - barrier.Y1) * (barrier.X1 - barrier.X2));
            float u = ((ray.X1 - ray.X2) * (ray.Y1 - barrier.Y1)) - ((ray.Y1 - ray.Y2) * (ray.X1 - barrier.X1));

            if (den == 0)
            {
                return false;
            }

            t /= den;
            u /= -den;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                intersectionPoint.X = (int)(ray.X1 + (t * (ray.X2 - ray.X1)));
                intersectionPoint.Y = (int)(ray.Y1 + (t * (ray.Y2 - ray.Y1)));
                return true;
            }
            return false;
        }

        // Distance formula.
        private int DistanceSquared(in int x1, in int y1, in int x2, in int y2)
        {
            int diffx = (x2 - x1) * (x2 - x1);
            int diffy = (y2 - y1) * (y2 - y1);

            // Compared against a larger number and other dist values, so we can forgo not using Math.Sqrt.
            return diffx + diffy;
        }
    }
}
