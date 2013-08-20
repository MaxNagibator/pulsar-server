/*
 * Сделано в SharpDevelop.
 * Пользователь: Alexey
 * Дата: 17.08.2013
 * Время: 15:12
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
namespace TestServerPulsar
{
    partial class AlarmForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlarmForm));
            this.uiMessageTextBox = new System.Windows.Forms.TextBox();
            this.uiServerLabel = new System.Windows.Forms.Label();
            this.uiMainNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.uiSendToPulsarButton = new System.Windows.Forms.Button();
            this.uiIpAddressTextBox = new System.Windows.Forms.TextBox();
            this.uiStartServerButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // uiMessageTextBox
            // 
            this.uiMessageTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uiMessageTextBox.Location = new System.Drawing.Point(0, 69);
            this.uiMessageTextBox.Multiline = true;
            this.uiMessageTextBox.Name = "uiMessageTextBox";
            this.uiMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.uiMessageTextBox.Size = new System.Drawing.Size(629, 296);
            this.uiMessageTextBox.TabIndex = 0;
            // 
            // uiServerLabel
            // 
            this.uiServerLabel.Location = new System.Drawing.Point(12, 43);
            this.uiServerLabel.Name = "uiServerLabel";
            this.uiServerLabel.Size = new System.Drawing.Size(100, 23);
            this.uiServerLabel.TabIndex = 5;
            this.uiServerLabel.Text = "Server";
            // 
            // uiMainNotifyIcon
            // 
            this.uiMainNotifyIcon.BalloonTipText = "succesed connect";
            this.uiMainNotifyIcon.BalloonTipTitle = "grac";
            this.uiMainNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("uiMainNotifyIcon.Icon")));
            this.uiMainNotifyIcon.Text = "notifyIcon1";
            this.uiMainNotifyIcon.Visible = true;
            this.uiMainNotifyIcon.DoubleClick += new System.EventHandler(this.UiMainNotifyIcon_DoubleClick);
            // 
            // uiSendToPulsarButton
            // 
            this.uiSendToPulsarButton.Location = new System.Drawing.Point(234, 10);
            this.uiSendToPulsarButton.Name = "uiSendToPulsarButton";
            this.uiSendToPulsarButton.Size = new System.Drawing.Size(100, 23);
            this.uiSendToPulsarButton.TabIndex = 6;
            this.uiSendToPulsarButton.Text = "send test query";
            this.uiSendToPulsarButton.UseVisualStyleBackColor = true;
            this.uiSendToPulsarButton.Click += new System.EventHandler(this.UiSendToPulsarButton_Click);
            // 
            // uiIpAddressTextBox
            // 
            this.uiIpAddressTextBox.Location = new System.Drawing.Point(12, 12);
            this.uiIpAddressTextBox.Multiline = true;
            this.uiIpAddressTextBox.Name = "uiIpAddressTextBox";
            this.uiIpAddressTextBox.Size = new System.Drawing.Size(89, 19);
            this.uiIpAddressTextBox.TabIndex = 7;
            this.uiIpAddressTextBox.Text = "192.168.1.102";
            // 
            // uiStartServerButton
            // 
            this.uiStartServerButton.Location = new System.Drawing.Point(107, 10);
            this.uiStartServerButton.Name = "uiStartServerButton";
            this.uiStartServerButton.Size = new System.Drawing.Size(100, 23);
            this.uiStartServerButton.TabIndex = 8;
            this.uiStartServerButton.Text = "server start";
            this.uiStartServerButton.UseVisualStyleBackColor = true;
            this.uiStartServerButton.Click += new System.EventHandler(this.UiStartServerButton_Click);
            // 
            // AlarmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 365);
            this.Controls.Add(this.uiStartServerButton);
            this.Controls.Add(this.uiIpAddressTextBox);
            this.Controls.Add(this.uiSendToPulsarButton);
            this.Controls.Add(this.uiServerLabel);
            this.Controls.Add(this.uiMessageTextBox);
            this.Name = "AlarmForm";
            this.Text = "AlarmServerPulsar";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Button uiStartServerButton;
		private System.Windows.Forms.TextBox uiIpAddressTextBox;
		private System.Windows.Forms.Button uiSendToPulsarButton;
		private System.Windows.Forms.NotifyIcon uiMainNotifyIcon;
		private System.Windows.Forms.Label uiServerLabel;
        private System.Windows.Forms.TextBox uiMessageTextBox;
	}
}
