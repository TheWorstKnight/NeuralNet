using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace Diplom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        NeuralNet net = new NeuralNet();
        bool error = false;

        public void BuildChart()
        {
            List<Series> sers = new List<Series>();
            List<List<double>> ys = net.GetTestY();
            for (int i = 0; i < net.Y; i++)
            {
                sers.Add(new Series());
                sers[i].Name = "Series" + i;
                sers[i].Color = Color.FromArgb(255 - i * 5, 0, 255 - i * 10);
                sers[i].IsVisibleInLegend = false;
                sers[i].IsXValueIndexed = true;
                sers[i].ChartType = SeriesChartType.Line;
                
                chart1.Series.Add(sers[i]);
                
                for (int j = 0; j < ys.Count; j++)
                {
                    sers[i].Points.AddXY(j, ys[j][i]);
                }
                //Legend leg = new Legend();
                //leg.Name = "Y Legend";

            }
            sers.Clear();
        }

        public void ClearChart()
        {
            chart1.Series.Clear();
        }

        public void DisableControlls()
        {
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            radioButton4.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
        }

        public void EnableControlls()
        {
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
            radioButton4.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
        }

        public void CheckBias()
        {
            if (radioButton1.Checked) net.Bias = 0;
            else net.Bias = 1;
        }

        public void CheckNormalization()
        {
            if (radioButton3.Checked) net.Norm = true;
            else net.Norm = false;
        }

        public void CheckClasterisation()
        {
            if (radioButton5.Checked) net.Clast = true;
            else net.Clast = false;
        }

        public void CheckY()
        {
            int yval;
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Y field is empty", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else if (!int.TryParse(textBox1.Text, out yval))
            {
                MessageBox.Show("Y value is not valid", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else net.Y = yval;

        }

        public void CheckN()
        {
            int nval;
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("N field is empty", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else if (!int.TryParse(textBox2.Text, out nval))
            {
                MessageBox.Show("N value is not valid", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else net.Nval = nval;

        }

        public void CheckQ()
        {
            double qval;
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Q field is empty", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else if (!double.TryParse(textBox3.Text, out qval))
            {
                MessageBox.Show("Q value is not valid", "Error", MessageBoxButtons.OK);
                error = true;
            }
            else net.Qval = qval;

        }

        public void CheckC()
        {
            int cval;
            if (net.Clast==true)
            {
                if (string.IsNullOrWhiteSpace(textBox4.Text))
                {
                    MessageBox.Show("C field is empty", "Error", MessageBoxButtons.OK);
                    error = true;
                }
                else if (!int.TryParse(textBox4.Text, out cval))
                {
                    MessageBox.Show("C value is not valid", "Error", MessageBoxButtons.OK);
                    error = true;
                }
                else net.Cval = cval;
            }
        }

        public bool CheckExtension(string filename)
        {
            string extension = Path.GetExtension(filename);
            if (extension == ".txt" || extension == ".csv")
                return true;
            else return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckBias();
            CheckY();
            CheckN();
            CheckQ();
            CheckNormalization();
            CheckClasterisation();
            CheckC();
            if (error == false)
            {
                OpenFileDialog dialog1 = new OpenFileDialog();
                do
                {
                    if (dialog1.ShowDialog() == DialogResult.OK)
                    {
                        if (CheckExtension(dialog1.FileName))
                        {
                            if (net.ReadData(dialog1.FileName))
                            {
                                if (radioButton5.Checked)
                                    net.FindClusters();
                                net.TrainNet();
                                richTextBox1.Clear();
                                DisableControlls();
                                ClearChart();
                            }
                            else MessageBox.Show("Wrong file content", "Error", MessageBoxButtons.OK);
                        }
                        else MessageBox.Show("Wrong file extention", "Error", MessageBoxButtons.OK);
                    }
                    else break;
                } while (!CheckExtension(dialog1.FileName));
            }
            error = false;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog2 = new OpenFileDialog();
            do
            {
                if (dialog2.ShowDialog() == DialogResult.OK)
                {
                    if (CheckExtension(dialog2.FileName))
                    {
                        if (net.ReadData(dialog2.FileName))
                        {

                            if (radioButton5.Checked)
                                net.FindTestClusters();
                            net.TestNet();
                            StringBuilder str = new StringBuilder();
                            str.AppendLine("MAPE = " + net.MAPE + "\nMAE = " + net.MAE + "\nRMSE = " + net.RMSE);

                            richTextBox1.Text = str.ToString();
                            BuildChart();
                            net.ClearResourses();
                            EnableControlls();
                        }
                        else MessageBox.Show("Wrong file content", "Error", MessageBoxButtons.OK);
                    }
                    else MessageBox.Show("Wrong file extention", "Error", MessageBoxButtons.OK);
                }
                else break;
            } while (!CheckExtension(dialog2.FileName)) ;
        }
    }
}
