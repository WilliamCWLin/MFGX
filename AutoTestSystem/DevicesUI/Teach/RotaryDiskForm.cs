using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using AutoTestSystem.Base;
using System.Windows.Forms;

namespace AutoTestSystem.DevicesUI.Teach
{

    public partial class RotaryDiskForm : Form
    {
        private Dictionary<int, string> stationAngles;
        private List<DUT_BASE> unitsOnDisk;

        public RotaryDiskForm(Dictionary<int, string> stationAngles, List<DUT_BASE> unitsOnDisk)
        {
            this.stationAngles = stationAngles;
            this.unitsOnDisk = unitsOnDisk;
            this.Text = "Rotary Disk Status";
            this.Size = new Size(400, 400);
            this.Paint += new PaintEventHandler(this.RotaryDiskForm_Paint);
        }

        private void RotaryDiskForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            int radius = Math.Min(centerX, centerY) - 20;

            foreach (var station in stationAngles)
            {
                double angleRad = station.Key * Math.PI / 180;
                int x = centerX + (int)(radius * Math.Cos(angleRad));
                int y = centerY + (int)(radius * Math.Sin(angleRad));

                g.FillEllipse(Brushes.LightBlue, x - 20, y - 20, 40, 40);
                g.DrawString(station.Value, this.Font, Brushes.Black, x - 20, y - 20);

                // Display DUT status if available
                //var dut = unitsOnDisk.Find(u => u.StationAngle == station.Key);
                //if (dut != null)
                //{
                //    g.DrawString(dut.Status, this.Font, Brushes.Red, x - 20, y + 20);
                //}
            }
        }
    }

}
