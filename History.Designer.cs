
namespace streamStore
{
    partial class HistoryTask
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
            this.historyTaskList = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // historyTaskList
            // 
            this.historyTaskList.BackColor = System.Drawing.SystemColors.Menu;
            this.historyTaskList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.historyTaskList.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.historyTaskList.ForeColor = System.Drawing.Color.Transparent;
            this.historyTaskList.Location = new System.Drawing.Point(0, 0);
            this.historyTaskList.Name = "historyTaskList";
            this.historyTaskList.Size = new System.Drawing.Size(414, 560);
            this.historyTaskList.TabIndex = 0;
            this.historyTaskList.Text = "";
            // 
            // HistoryTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 559);
            this.Controls.Add(this.historyTaskList);
            this.Name = "HistoryTask";
            this.Text = "歷史任務";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox historyTaskList;
    }
}