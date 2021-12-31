
using System;
using System.IO;

namespace streamStore
{
    partial class IOForm
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
            this.request_area = new System.Windows.Forms.GroupBox();
            this.request_record = new System.Windows.Forms.RichTextBox();
            this.response_area = new System.Windows.Forms.GroupBox();
            this.response_record = new System.Windows.Forms.RichTextBox();
            this.stop_btn = new System.Windows.Forms.Button();
            this.clear_btn = new System.Windows.Forms.Button();
            this.checkTarget = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.request_area.SuspendLayout();
            this.response_area.SuspendLayout();
            this.SuspendLayout();
            // 
            // request_area
            // 
            this.request_area.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.request_area.Controls.Add(this.request_record);
            this.request_area.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.request_area.Location = new System.Drawing.Point(12, 46);
            this.request_area.Name = "request_area";
            this.request_area.Size = new System.Drawing.Size(399, 599);
            this.request_area.TabIndex = 0;
            this.request_area.TabStop = false;
            this.request_area.Text = "request";
            // 
            // request_record
            // 
            this.request_record.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.request_record.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.request_record.Location = new System.Drawing.Point(6, 26);
            this.request_record.Name = "request_record";
            this.request_record.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.request_record.Size = new System.Drawing.Size(387, 567);
            this.request_record.TabIndex = 0;
            this.request_record.Text = "";
            // 
            // response_area
            // 
            this.response_area.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.response_area.Controls.Add(this.response_record);
            this.response_area.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.response_area.Location = new System.Drawing.Point(417, 46);
            this.response_area.Name = "response_area";
            this.response_area.Size = new System.Drawing.Size(401, 599);
            this.response_area.TabIndex = 1;
            this.response_area.TabStop = false;
            this.response_area.Text = "response";
            // 
            // response_record
            // 
            this.response_record.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.response_record.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.response_record.Location = new System.Drawing.Point(6, 26);
            this.response_record.Name = "response_record";
            this.response_record.Size = new System.Drawing.Size(387, 567);
            this.response_record.TabIndex = 0;
            this.response_record.Text = "";
            // 
            // stop_btn
            // 
            this.stop_btn.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.stop_btn.Location = new System.Drawing.Point(421, 653);
            this.stop_btn.Name = "stop_btn";
            this.stop_btn.Size = new System.Drawing.Size(195, 36);
            this.stop_btn.TabIndex = 2;
            this.stop_btn.Text = "暫停紀錄";
            this.stop_btn.UseVisualStyleBackColor = true;
            this.stop_btn.Click += new System.EventHandler(this.IO_btn_Click);
            // 
            // clear_btn
            // 
            this.clear_btn.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.clear_btn.Location = new System.Drawing.Point(623, 653);
            this.clear_btn.Name = "clear_btn";
            this.clear_btn.Size = new System.Drawing.Size(195, 36);
            this.clear_btn.TabIndex = 3;
            this.clear_btn.Text = "清空紀錄";
            this.clear_btn.UseVisualStyleBackColor = true;
            this.clear_btn.Click += new System.EventHandler(this.IO_btn_Click);
            // 
            // checkTarget
            // 
            this.checkTarget.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.checkTarget.FormattingEnabled = true;
            this.checkTarget.Location = new System.Drawing.Point(502, 12);
            this.checkTarget.Name = "checkTarget";
            this.checkTarget.Size = new System.Drawing.Size(316, 27);
            this.checkTarget.TabIndex = 4;
            this.checkTarget.SelectedIndexChanged += new System.EventHandler(this.checkTarget_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(17, 660);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "紀錄狀態";
            // 
            // IOForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(830, 695);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkTarget);
            this.Controls.Add(this.clear_btn);
            this.Controls.Add(this.stop_btn);
            this.Controls.Add(this.response_area);
            this.Controls.Add(this.request_area);
            this.Name = "IOForm";
            this.Text = "IOForm";
            this.request_area.ResumeLayout(false);
            this.response_area.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox request_area;
        private System.Windows.Forms.GroupBox response_area;
        private System.Windows.Forms.Button stop_btn;
        private System.Windows.Forms.Button clear_btn;
        public System.Windows.Forms.ComboBox checkTarget;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox request_record;
        public System.Windows.Forms.RichTextBox response_record;
    }
}