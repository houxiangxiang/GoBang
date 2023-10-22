namespace WindowsFormsApplication1
{
    partial class FormRule
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel_double3 = new System.Windows.Forms.Panel();
            this.panelDouble4 = new System.Windows.Forms.Panel();
            this.panelLonglink = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial Unicode MS", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(258, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Forbidden Rule For Forthgoner";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(72, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(184, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Double-Live-3 is forbidden";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(72, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(151, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Double-4 is forbidden";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(72, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 18);
            this.label4.TabIndex = 3;
            this.label4.Text = "Long-Link is forbidden";
            // 
            // panel_double3
            // 
            this.panel_double3.Location = new System.Drawing.Point(262, 56);
            this.panel_double3.Name = "panel_double3";
            this.panel_double3.Size = new System.Drawing.Size(56, 50);
            this.panel_double3.TabIndex = 4;
            this.panel_double3.Paint += new System.Windows.Forms.PaintEventHandler(this.FormRule_OnPaint);
            // 
            // panelDouble4
            // 
            this.panelDouble4.Location = new System.Drawing.Point(262, 112);
            this.panelDouble4.Name = "panelDouble4";
            this.panelDouble4.Size = new System.Drawing.Size(56, 53);
            this.panelDouble4.TabIndex = 5;
            // 
            // panelLonglink
            // 
            this.panelLonglink.Location = new System.Drawing.Point(262, 171);
            this.panelLonglink.Name = "panelLonglink";
            this.panelLonglink.Size = new System.Drawing.Size(145, 45);
            this.panelLonglink.TabIndex = 6;
            // 
            // FormRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 266);
            this.Controls.Add(this.panelLonglink);
            this.Controls.Add(this.panelDouble4);
            this.Controls.Add(this.panel_double3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormRule";
            this.Text = "RuleIntroduction";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel_double3;
        private System.Windows.Forms.Panel panelDouble4;
        private System.Windows.Forms.Panel panelLonglink;
    }
}