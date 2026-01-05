using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem.DevicesUI.Teach
{
    public partial class MotionParamPanel : UserControl
    {

        public static double Percentage = 1.0;
        MotionBase mot = null;

        public MotionParamPanel(string Name, MotionBase motion)
        {
            InitializeComponent();
            mot = motion;
            label1.Text = Name;
            radioButtonJog.Checked = true;

            textVel.Text = "0.1";
            textMaxVel.Text = "0.1";
            textAcc.Text = "0.1";
            textDec.Text = "0.1";
            textMoveValue.Text = "0.1";
        }

        public void UpdateMotorMotionFromTextBox(MotorMotion m)
        {
            double v;
            if (double.TryParse(textPosition.Text, out v)) m.Position = v;
            if (double.TryParse(textVel.Text, out v)) m.StartSpeed = v;
            if (double.TryParse(textMaxVel.Text, out v)) m.MaxVel = v;
            if (double.TryParse(textAcc.Text, out v)) m.Acceleration = v;
            if (double.TryParse(textDec.Text, out v)) m.Deceleration = v;
        }
        public void UpdateTextBoxFromMotorMotion(MotorMotion m)
        {
            textMoveValue.Text = m.Position.ToString("F3");
            textVel.Text = m.StartSpeed.ToString("F3");
            textMaxVel.Text = m.MaxVel.ToString("F3");
            textAcc.Text = m.Acceleration.ToString("F3");
            textDec.Text = m.Deceleration.ToString("F3");
            radioButtonAbs.Checked = true;
        }
        private bool ValidateTextBox(TextBox tb, string name, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                MessageBox.Show($"{name} 不可為空", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Focus();
                return false;
            }
            if (!double.TryParse(tb.Text, out value))
            {
                MessageBox.Show($"{name} 必須為非零數值", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Focus();
                return false;
            }
            return true;
        }
        public void UpdatePosition(double value)
        {
            textPosition.Text = value.ToString("F3");
        }
        public void UpdateRelPosition(double value)
        {
            label_rel.Text = value.ToString("F3");
        }

        public void UpdateErrMessage(string value)
        {
            textStatus.Text = value;
        }
        public void UpdateLabelColor(Color c)
        {
            label1.BackColor = c;
        }
        private void RunMotion(Func<double, double, double, double, double, bool> moveFunc, double position, double startSpeed, double maxVel, double acc, double dec)
        {
            var t = Task.Run(async () =>
            {
                label1.BackColor = Color.GreenYellow;
                moveFunc(position, startSpeed, maxVel, acc, dec);
                int status = 1;
                double pos = 0;
                int TimeOut = 60000;
                var Movewatch = Stopwatch.StartNew();
                while (true)
                {
                    mot.GetMotionStatus(ref status);
                    mot.GetCurrentPos(ref pos);
                    this.BeginInvoke((Action)(() =>
                    {
                        textPosition.Text = $"{pos:F5}";
                        textStatus.Text = $"{status}";
                        if (radioButtonAbs.Checked)
                            label_rel.Text = (position - pos).ToString("0.###");
                        else if (radioButtonRel.Checked)
                            label_rel.Text = "";
                        else if (radioButtonJog.Checked)
                            label_rel.Text = "";
                    }));
                    if (status == 0)
                        break;
                    if (status == -99)
                        break;
                    if (Movewatch.ElapsedMilliseconds > TimeOut)
                    {
                        mot.EmgStop();
                        MessageBox.Show($"Motion TimeOut!: 超過{TimeOut / 1000}s", "回Home失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    }
                    await Task.Delay(100);
                }
                label1.BackColor = SystemColors.Control;
            });
        }
        private bool TryGetMotionParams(out double position, out double startSpeed, out double maxVel, out double acc, out double dec)
        {
            position = startSpeed = maxVel = acc = dec = 0;
            if (!ValidateTextBox(textMoveValue, "位置", out position)) return false;
            if (!ValidateTextBox(textVel, "起始速度", out startSpeed)) return false;
            if (!ValidateTextBox(textMaxVel, "最大速度", out maxVel)) return false;
            if (!ValidateTextBox(textAcc, "加速度", out acc)) return false;
            if (!ValidateTextBox(textDec, "減速度", out dec)) return false;
            return true;
        }
        private void btnN_Click(object sender, EventArgs e)
        {
            if (!TryGetMotionParams(out var MoveValue, out var StartSpeed, out var MaxVel, out var Acceleration, out var Deceleration))
                return;

            if (radioButtonAbs.Checked)
                RunMotion(mot.Absolute_Move, MoveValue, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
            else if (radioButtonRel.Checked)
                RunMotion(mot.Relative_Move, -MoveValue, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
            else if (radioButtonJog.Checked)
                RunMotion(mot.Relative_Move, -1.0, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
        }

        private void btnP_Click(object sender, EventArgs e)
        {
            if (!TryGetMotionParams(out var MoveValue, out var StartSpeed, out var MaxVel, out var Acceleration, out var Deceleration))
                return;

            if (radioButtonAbs.Checked)
                RunMotion(mot.Absolute_Move, MoveValue, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
            else if (radioButtonRel.Checked)
                RunMotion(mot.Relative_Move, MoveValue, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
            else if (radioButtonJog.Checked)
                RunMotion(mot.Relative_Move, 1.0, StartSpeed, MaxVel * Percentage, Acceleration, Deceleration);
        }

        private void btn_home_Click(object sender, EventArgs e)
        {
            /*if (!TryGetMotionParams(out var Position, out var StartSpeed, out var MaxVel, out var Acceleration, out var Deceleration))
                return;*/
            if (!mot.Init(""))
            {
                MessageBox.Show("Motion Init Fail", "Note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //bool home_done = true;
            double pos = 0;
            bool home_ret = mot.SyncHome();
            //home_done = false;
            //await t; // 等待 Task 完成

            int status = 0;
            int HomeTimeOut = 60000;
            var Resetwatch = Stopwatch.StartNew();
            var t = Task.Run(async () =>
            {
                do
                {
                    if (mot.HomeDone(ref status))
                    {
                        mot.GetCurrentPos(ref pos);
                        this.BeginInvoke((Action)(() =>
                        {
                            textPosition.Text = $"{pos:F5}";
                        }));
                        //LogMessage($"=====Home Status:{status}=====");
                        if (Resetwatch.ElapsedMilliseconds > HomeTimeOut)
                        {
                            mot.EmgStop();
                            MessageBox.Show($"Reset Home Fail, Home TimeOut!: 超過{HomeTimeOut / 1000}s", "回Home失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }
                    }
                    else
                    {
                        mot.EmgStop();
                        MessageBox.Show($"Reset Home Fail, EmgStop !!!!", "回Home失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    }
                    await Task.Delay(100);
                } while (status != 0);
            });                     
        }
    }

}
