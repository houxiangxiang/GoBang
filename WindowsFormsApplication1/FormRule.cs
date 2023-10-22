using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class FormRule : Form
    {
        public FormRule()
        {
            InitializeComponent();
        }

        
        private void FormRule_OnPaint(object sender, PaintEventArgs e)
        {
            // base.OnPaint(e);

            Brush blackBrush = new SolidBrush(Color.Black);

            // draw double-live-3
            Graphics g_double_live_3 = panel_double3.CreateGraphics();
            float lstep = panel_double3.Width / 5;

            g_double_live_3.FillEllipse(blackBrush, new Rectangle(new Point((int)(panel_double3.Width / 2 - lstep), (int)(panel_double3.Height / 2)), new Size(10, 10)));
            g_double_live_3.FillEllipse(blackBrush, new Rectangle(new Point((int)(panel_double3.Width / 2), (int)(panel_double3.Height / 2)), new Size(10, 10)));
            g_double_live_3.FillEllipse(blackBrush, new Rectangle(new Point((int)(panel_double3.Width / 2 + lstep), (int)(panel_double3.Height / 2)), new Size(10, 10)));
            
            g_double_live_3.FillEllipse(blackBrush, new Rectangle(new Point((int)(panel_double3.Width / 2), (int)(panel_double3.Height / 2 - lstep)), new Size(10, 10)));
            g_double_live_3.FillEllipse(blackBrush, new Rectangle(new Point((int)(panel_double3.Width / 2), (int)(panel_double3.Height / 2 + lstep)), new Size(10, 10)));


            // draw double-4
            Graphics g_double_4 = panelDouble4.CreateGraphics();
            for (int i = 0; i < 4; i++)
            {
                g_double_4.FillEllipse(blackBrush, new Rectangle(new Point((int)(lstep * (i + 1)), (int)(panelDouble4.Height / 2)), new Size(10, 10)));
            }
            int x = (int)(lstep * 3);
            g_double_4.FillEllipse(blackBrush, new Rectangle(new Point(x, (int)(panelDouble4.Height / 2 - lstep * 2)), new Size(10, 10)));
            g_double_4.FillEllipse(blackBrush, new Rectangle(new Point(x, (int)(panelDouble4.Height / 2 - lstep)), new Size(10, 10)));
            g_double_4.FillEllipse(blackBrush, new Rectangle(new Point(x, (int)(panelDouble4.Height / 2 + lstep)), new Size(10, 10)));


            // draw long-link
            Graphics g_longlink = panelLonglink.CreateGraphics();
            for (int i=0; i<6; i++)
            {
                g_longlink.FillEllipse(blackBrush, new Rectangle(new Point((int)(lstep * (i + 1)), (int)(panelLonglink.Height / 2)), new Size(10, 10)));
            }
        }
    }
}
