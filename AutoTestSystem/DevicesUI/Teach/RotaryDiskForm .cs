using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class RotaryTenShapeDiskView : Control
{
    public List<DUT_BASE> DUTs { get; set; }
    public int CurrentAngleIndex { get; set; } = 0; // 0=正下方
    public int StationCount => 4;

    public RotaryTenShapeDiskView()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 14, FontStyle.Bold);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int W = Width, H = Height;
        Point center = new Point(W / 2, H / 2 + 12); // 整體中心往下偏移
        int radius = Math.Min(W, H) / 2 - 90;

        // 1. 畫大圓
        using (var pen = new Pen(Color.SteelBlue, 10))
            g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);

        // 2. 畫中心小圓
        int smallR = 36;
        g.FillEllipse(Brushes.White, center.X - smallR, center.Y - smallR, smallR * 2, smallR * 2);
        g.DrawEllipse(new Pen(Color.SteelBlue, 3), center.X - smallR, center.Y - smallR, smallR * 2, smallR * 2);

        // 3. 畫出料指針（指向工位1，正下）
        int arrowLen = radius - 10;
        Point arrowTip = new Point(center.X, center.Y + arrowLen);
        g.DrawLine(new Pen(Color.SteelBlue, 9) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round }, center, arrowTip);

        // 4. 定義十字工位位置
        int cardW = 128, cardH = 85, cardRadius = 22;
        Point[] pos = new Point[4];
        pos[0] = new Point(center.X, center.Y + radius); // 工位1(正下)
        pos[1] = new Point(center.X + radius, center.Y); // 工位2(正右)
        pos[2] = new Point(center.X, center.Y - radius); // 工位3(正上)
        pos[3] = new Point(center.X - radius, center.Y); // 工位4(正左)

        string[] stationTitles = { "工位1", "工位2", "工位3", "工位4" };
        Color[] stationColors = { Color.Orange, Color.DeepSkyBlue, Color.LimeGreen, Color.Violet };

        StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // 5. 畫4個工位卡片，分三層顯示
        for (int i = 0; i < 4; i++)
        {
            // 逆時針映射
            int realIdx = (i + StationCount - CurrentAngleIndex) % StationCount;
            var pt = pos[i];
            var dut = DUTs[i];
            Rectangle rect = new Rectangle(pt.X - cardW / 2, pt.Y - cardH / 2, cardW, cardH);

            // 工位底色
            Color fillColor = Color.FromArgb(245, 245, 245);
            if (dut?.testUnit != null && dut.testUnit.IsActive)
            {
                fillColor = dut.testUnit.IsFailed ? Color.FromArgb(230, 60, 60) : Color.LightYellow;
                if (dut.testUnit.AllStationsCompleted)
                    fillColor = Color.LimeGreen;
            }

            // 畫卡片背景
            using (var brush = new SolidBrush(fillColor))
                g.FillRoundedRectangle(brush, rect, cardRadius);
            g.DrawRoundedRectangle(new Pen(Color.Gray, 4), rect, cardRadius);

            // 工位大標題（顏色分層）
            using (var font = new Font("Segoe UI", 20, FontStyle.Bold))
                g.DrawString(stationTitles[i], font, new SolidBrush(stationColors[i]), rect.X + cardW / 2, rect.Y + 23, sfCenter);

            // SN（第二行，小字體，灰色）
            string sn = (dut?.testUnit != null && dut.testUnit.IsActive) ? (dut.Description ?? "—") : "—";
            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
                g.DrawString("SN: " + sn, font, Brushes.DimGray, rect.X + cardW / 2, rect.Y + 47, sfCenter);

            // 狀態（第三行）
            string state = "待機";
            Brush stateBrush = Brushes.DimGray;
            if (dut?.testUnit != null)
            {
                if (dut.testUnit.IsActive && dut.testUnit.IsFailed)
                {
                    state = "FAIL";
                    stateBrush = Brushes.Red;
                }
                else if (dut.testUnit.IsActive && dut.testUnit.AllStationsCompleted)
                {
                    state = "PASS";
                    stateBrush = Brushes.DarkGreen;
                }
                else if (dut.testUnit.IsActive)
                {
                    state = "測試中";
                    stateBrush = Brushes.Orange;
                }
            }
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                g.DrawString(state, font, stateBrush, rect.X + cardW / 2, rect.Y + 67, sfCenter);

            // 進度圓點（底部排成一行）
            if (dut?.testUnit?.StationCompleted != null)
            {
                int dotsX = rect.X + cardW / 2 - 30, dotsY = rect.Y + cardH - 16;
                for (int s = 0; s < dut.testUnit.StationCompleted.Length; s++)
                {
                    Color dotColor = dut.testUnit.StationCompleted[s]
                        ? Color.MediumSeaGreen
                        : (s == dut.testUnit.CurrentStationIndex ? Color.Orange : Color.LightGray);
                    using (var brush = new SolidBrush(dotColor))
                        g.FillEllipse(brush, dotsX + s * 20, dotsY, 12, 12);
                    g.DrawEllipse(Pens.Gray, dotsX + s * 20, dotsY, 12, 12);
                }
            }
        }

        // 6. 畫中心主標題
        using (var font = new Font("Segoe UI", 21, FontStyle.Bold))
            g.DrawString("圓盤工位狀態", font, Brushes.SteelBlue, center.X, center.Y - 38, sfCenter);

        // 7. 畫出料對齊工位提示
        using (var font = new Font("Segoe UI", 12, FontStyle.Italic))
            g.DrawString("出料對齊：工位1", font, Brushes.SteelBlue, center.X, center.Y + 35, sfCenter);
    }
}

// 擴充圓角
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
    {
        using (var path = RoundedRect(bounds, cornerRadius))
            g.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
    {
        using (var path = RoundedRect(bounds, cornerRadius))
            g.DrawPath(pen, path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}


public class RotaryDiskForm : Form
{
    private RotaryTenShapeDiskView diskView;
    private Timer updateTimer;
    private RotaryTestController controller;

    public RotaryDiskForm(RotaryTestController controller)
    {
        this.controller = controller;
        diskView = new RotaryTenShapeDiskView
        {
            DUTs = controller.UnitsOnDisk,
            Dock = DockStyle.Fill
        };
        this.Text = "圓盤DUT狀態面板（十字）";
        this.Size = new System.Drawing.Size(1040, 740);
        this.Controls.Add(diskView);

        updateTimer = new Timer();
        updateTimer.Interval = 300;
        updateTimer.Tick += (s, e) => UpdateView();
        updateTimer.Start();
    }

    private void UpdateView()
    {
        diskView.CurrentAngleIndex = controller.StationAngles.IndexOf(controller.CurrentAngle);
        diskView.DUTs = controller.UnitsOnDisk;
        diskView.Invalidate();
    }
}
