using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Diplom
{
    class NeuralNet
    {
        int s, bias, max_norm, Smax;  double Q;  static double[] max_norm_array_bias1; static double[] norm_coef;double []sigma;public double MAPE, MAE, RMSE;static int N;bool normalize, clasterize;static int C;
        static List<List<double>> incoming_data;
        static List<List<double>> recent_data;
        static List<List<double>> max_norm_array;
        static List<double> K;
        static List<double> XN;
        static List<List<double>> TestK;
        static List<List<double>> TestY;
        static List<List<double>> Clusters;
        static List<List<double>> test_clasters;
        Clasterisation clasters = new Clasterisation();

       // static List<List<double>> K_matrix;
        static int n, y;

        public NeuralNet()
        {
            incoming_data = new List<List<double>>();
            recent_data = new List<List<double>>();
            max_norm_array = new List<List<double>>();
            K = new List<double>();
            XN = new List<double>();
            TestK = new List<List<double>>();
            TestY = new List<List<double>>();
            Clusters = new List<List<double>>();
            test_clasters = new List<List<double>>();
            bias = 0;y = 1;s = 0;N = 1000;Q=0.000001;normalize = true;
        }
        public int Bias { set { if (value == 0 || value == 1) { bias = value; } } }
        public int Y { set { y = value; } get { return y; } }
        public int Nval { set { N = value; } }
        public double Qval { set { Q = value; } }
        public bool Norm { set { normalize = value; } }
        public bool Clast { set { clasterize = value; } get { return clasterize; } }
        public int Cval { set { C = value; } }
        //зчитування даних з файлу
        public bool ReadData(string filename)
        {
            string[] buf; int j = 0; double check;
            StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open));
            while (!reader.EndOfStream)
            {
                incoming_data.Add(new List<double>());
                buf = Regex.Split(reader.ReadLine(), " |;|\t");
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = buf[i].Replace('.', ',');
                    if (!Double.TryParse(buf[i], out check)) { incoming_data.Clear();return false; }
                    incoming_data[j].Add(Convert.ToDouble(buf[i]));
                }
                j++;
            }
            reader.Close();
            n = incoming_data[0].Count;
            return true;
        }
        //нормалізація вхідної матриці
        void NormalizeMatrix()
        {
            norm_coef = new double[n];
            for (int j = 0; j < n; j++)
            {
                //new Thread(NormalizationBody).Start(j);
                //Thread.Sleep(10);
                double max = 0;
                for (int i = 0; i < incoming_data.Count; i++)
                {
                    if (max < incoming_data[i][j])
                        max = incoming_data[i][j];
                }
                for (int i = 0; i < incoming_data.Count; i++)
                    incoming_data[i][j] /= max;
                norm_coef[j] = max;
            }
            
        }
        //тіло нормалізації для асинхронного обчислення(потім дороблю...може)
        static void NormalizationBody(object column)
        {
                double max = 0; int j = (int)column;
                for (int i = 0; i < incoming_data.Count; i++)
                {
                    if (max < incoming_data[i][j])
                        max = incoming_data[i][j];
                }
                for (int i = 0; i < incoming_data.Count; i++)
                    incoming_data[i][j] /= max;
            norm_coef[j] = max;
        }
        //нормалізація тестової вибірки
        void TestNormalizeMatrix()
        {
            for (int j = 0; j < n; j++)
            {
                //new Thread(TestNormalizationBody).Start(j);
                //Thread.Sleep(10);
                for (int i = 0; i < recent_data.Count; i++)
                    recent_data[i][j] /= norm_coef[j];
            }
        }
        //тіло тестової нормалізації для асинхронного обчислення(потім дороблю...може)
        static void TestNormalizationBody(object column)
        {
            int j = (int)column;
            for (int i = 0; i < incoming_data.Count; i++)
                incoming_data[i][j] /= norm_coef[j];
        }
        //створює матрицю, на якій буде навчатись мережа(обрізана квадратна матриця)
        void CreateRecentMatrix()
        {
            Random rnd = new Random();
            if (bias == 0)
            {
                //int i = rnd.Next(0, (incoming_data.Count - n));
                int i=0;
                int k = 0,checker = 0;
                while(checker<n)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[k].Add(incoming_data[i][j]);
                    }
                    k++;i++;checker++;
                }
            }
            else
            {
                //int i = rnd.Next(0, (incoming_data.Count - (n+1)));
                int i = 0;
                int k = 0, checker = 0;
                while (checker < n+1)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[k].Add(incoming_data[i][j]);
                    }
                    k++;i++;checker++;
                }
            }
        }
        //створює матрицю, на якій буде навчатись мережа, за кластерами
        void CreateRecenMatrixByClusters()
        {
            if (bias == 0)
            { for (int i = 0; i < n; i++)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[i].Add(Clusters[i][j]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < n+1; i++)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[i].Add(Clusters[i][j]);
                    }
                }
            }
        }

        void CreateRecenTestMatrixByClusters()
        {
            if (bias == 0)
            {
                for (int i = 0; i < n; i++)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[i].Add(test_clasters[i][j]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < n + 1; i++)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                    {
                        recent_data[i].Add(test_clasters[i][j]);
                    }
                }
            }
        }
        //тренування мережі
        public void TrainNet()
        {
            if(normalize)NormalizeMatrix();
            //if(clasterize)CreateRecenMatrixByClusters();
            CreateRecentMatrix();
            if (bias == 1)
            {
                max_norm_array_bias1 = new double[recent_data.Count];
                for (int i = 0; i < recent_data[recent_data.Count - 1].Count; i++)
                { max_norm_array_bias1[i] = recent_data[recent_data.Count - 1][i]; }
                for (int j = 0; j < n; j++)
                {
                    //new Thread(SubtractBody).Start(j);
                    //Thread.Sleep(10);
                    for (int i = 0; i < recent_data.Count; i++)
                        recent_data[i][j] -= recent_data[recent_data.Count - 1][j];
                }
            }
            int flag = 0;
            while (s <= N)
            {
                max_norm = 0; double buf = 0; double max_norm_val = 0; XN.Add(0);

                for (int i = 0; i < recent_data.Count; i++)
                {
                    for (int j = 0; j < n - y; j++)
                    {
                        buf += Math.Pow(recent_data[i][j], 2);
                    }
                    if (max_norm_val < Math.Sqrt(buf))
                    { max_norm = i;XN[s]=buf ;max_norm_val = Math.Sqrt(buf); }
                    buf = 0;
                }
                


                    max_norm_array.Add(new List<double>());
                    for (int i = 0; i < recent_data[max_norm].Count; i++)
                    {
                        max_norm_array[s].Add(recent_data[max_norm][i]);
                    }

                    

                    IAsyncResult res = null;
                    for (int i = 0; i < recent_data.Count; i++)
                    {
                        //res= new Action<int ,int>(CountKBody).BeginInvoke(i,s,null,null);
                        // Thread.Sleep(10);
                        double top = 0;
                        for (int j = 0; j < n - y; j++)
                        {
                            top += recent_data[i][j] * max_norm_array[s][j];
                        }
                        K.Add(top / XN[s]);
                    }
                    //while (!res.IsCompleted)
                    //{ }
                    for (int i = 0; i < recent_data.Count; i++)
                    {
                        //res=new Action<int,int>(NewRecentMatrixBody).BeginInvoke(i,s,null,null);
                        //Thread.Sleep(10);
                        for (int j = 0; j < n; j++)
                        {
                            var a = max_norm_array[s][j] * K[i];
                            var b = recent_data[i][j] - a;
                            recent_data[i][j] = b;
                        }
                    }
                    //while (!res.IsCompleted)
                    //{ }
                    s++; K.Clear();
                if (Math.Sqrt(XN[s-1]) < Q)
                {
                    if (s > 0) { Smax = s - 1; flag = 1; break; }
                    else break;
                }
            }
            if(flag!=1)
            Smax = s;
            incoming_data.Clear(); recent_data.Clear(); s = 0;
        }
        //тестування мережі
        public void TestNet()
        {
            //if (clasterize) CreateRecenTestMatrixByClusters();
            //else
            //{
                for (int i = 0; i < incoming_data.Count; i++)
                {
                    recent_data.Add(new List<double>());
                    for (int j = 0; j < incoming_data[0].Count; j++)
                    {
                        recent_data[i].Add(incoming_data[i][j]);
                    }
                }
            //}
            if(normalize)TestNormalizeMatrix(); 
            if (bias == 1)
            {
                for (int j = 0; j < n; j++)
                {
                    //new Thread(TestSubtractBody).Start(j);
                    //Thread.Sleep(10);
                    for (int i = 0; i < recent_data.Count; i++)
                        recent_data[i][j] -= max_norm_array_bias1[j];
                }
            }

            do
            {
               // IAsyncResult res = null;
                TestK.Add(new List<double>());
                for (int i = 0; i < recent_data.Count; i++)
                {
                    //res = new Action<int, int>(TestCountKBody).BeginInvoke(i, s, null, null);
                    double top = 0;
                    for (int j = 0; j < n - y; j++)
                    {
                        top += recent_data[i][j] * max_norm_array[s][j];
                    }
                    TestK[s].Add(top / XN[s]);
                }
                //while (!res.IsCompleted)
                //{ }
                for (int i = 0; i < recent_data.Count; i++)
                {
                    //res = new Action<int, int>(NewRecentMatrixX).BeginInvoke(i, s, null, null);
                    for (int j = 0; j < recent_data[0].Count - y; j++)
                    {
                        recent_data[i][j] -= max_norm_array[s][j] * TestK[s][i];
                    }
                }
                //while (!res.IsCompleted)
                //{ }
                s++;
            } while (s <= Smax);

            for (int i = 0; i < recent_data.Count; i++)
                TestY.Add(new List<double>());
            
                IAsyncResult res = null;
                for (int i = 0; i < recent_data.Count; i++)
                {
                    //res = new Action<int>(NewRecentMatrixY).BeginInvoke(i, null, null);
                    double ybuf = 0;
                    if (TestY[i].Count == 0)
                    {
                        for (int j = n - y; j < n; j++)
                        {
                            for (int l = 0; l < TestK.Count; l++)
                                ybuf += TestK[l][i] * max_norm_array[l][j];
                            TestY[i].Add(ybuf);
                        }
                    }
                    else
                    {
                        int r = 0;
                        for (int j = n - y; j < n; j++)
                        {
                            for (int l = 0; l < TestK.Count; l++)
                                TestY[i][r] += TestK[l][i] * max_norm_array[l][j];
                            r++;
                        }
                    }
                }
                //while (!res.IsCompleted)
                //{ }
                
            if (bias == 1)
            {
                int l = 0;
                for (int i = 0; i < TestY.Count; i++)
                {
                    for (int j = n - y; j < n; j++)
                    {
                        TestY[i][l] += max_norm_array_bias1[j];
                        l++;
                    }
                    l = 0;
                }
            }

            int k = 0;
            for (int i = 0; i < TestY.Count; i++)
            {
                for (int j = n - y; j < n; j++)
                {
                    TestY[i][k] *= norm_coef[j];
                    k++;
                }
                k = 0;
            }
            k = 0;sigma = new double[TestY.Count];
            for (int i = 0; i < TestY.Count; i++)
            {
                for (int j = n - y; j < n; j++)
                {
                    sigma[i] += Math.Abs((TestY[i][k] - incoming_data[i][j]) / incoming_data[i][j]);
                    k++;
                }
                k = 0;
            }
            MAPE = sigma.Sum() / TestY.Count;
            double buf=0;
            for (int i = 0; i < TestY.Count; i++)
            {
                for (int j = n - y; j < n; j++)
                {
                    buf+= Math.Abs(TestY[i][k] - incoming_data[i][j]);
                    k++;
                }
                k = 0;
            }
            MAE = buf / TestY.Count;
            buf = 0;
            for (int i = 0; i < TestY.Count; i++)
            {
                for (int j = n - y; j < n; j++)
                {
                    buf += Math.Pow(Math.Abs(TestY[i][k] - incoming_data[i][j]),2);
                    k++;
                }
                k = 0;
            }
            RMSE = Math.Sqrt(buf / TestY.Count);

            FileStream str = new FileStream("E:\\Downloads\\y.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(str);
            for (int i = 0; i < TestY.Count; i++)
            {
                for (int j = 0; j < TestY[0].Count; j++)
                {
                    writer.Write(TestY[i][j] + "\t");
                }
                writer.WriteLine();
                
            }
            writer.Close();
            str.Close();
           // ClearResourses();
        }

        public List<List<double>> GetTestY()
        {
            return TestY;
        }

        public void ClearResourses()
        {
            recent_data.Clear();
            incoming_data.Clear();
            max_norm_array.Clear();
            XN.Clear();
            TestK.Clear();
            TestY.Clear();
            s = 0;
        }
        //це вам поки не треба) (звідси і нижче, там хіба в самому кінці пошук кластерів буде)
        static void SubtractBody(object column)
        {
            int j = (int)column;
            for (int i = 0; i < recent_data.Count; i++)
                recent_data[i][j] -= recent_data[recent_data.Count-1][j];
        }

        static void TestSubtractBody(object column)
        {
            int j = (int)column;
            for (int i = 0; i < recent_data.Count; i++)
                recent_data[i][j] -= max_norm_array_bias1[j];
        }

        static void CountKBody(int row,int s)
        {
            int i = (int)row; double top = 0;
            for (int j = 0; j < n-y; j++)
            {
                top += recent_data[i][j] * max_norm_array[s][j];
            }
            K.Add(top / XN[s]);
        }

        static void TestCountKBody(int row,int s)
        {
            int i = row; double top = 0;
            for (int j = 0; j < n - y; j++)
            {
                top += recent_data[i][j] * max_norm_array[s][j];
            }
            TestK[s].Add(top / XN[s]);
        }

        static void NewRecentMatrixBody(int row,int s)
        {
            int i = (int)row;
            for (int j = 0; j < n; j++)
            {
                recent_data[i][j] -= max_norm_array[s][j] * K[i];
            }
        }

        static void NewRecentMatrixX(int row,int s)
        {
            int i = row;
            for (int j = 0; j < recent_data[0].Count-y; j++)
            {
              recent_data[i][j] -= max_norm_array[s][j] * TestK[s][i];
            }
        }

        static void NewRecentMatrixY(int row)
        {
            int i = row;
            //for (int j = recent_data.Count - y; j < recent_data.Count; j++)
            //{
            //    recent_data[i][j] += max_norm_array[j] * TestK[s][i];
            //}

            double ybuf=0;
            if (TestY[i].Count == 0)
            {
                for (int j = n - y; j < n; j++)
                {
                    for (int k = 0; k < TestK.Count; k++)
                        ybuf += TestK[k][i] * max_norm_array[i][j];
                    TestY[i].Add(ybuf);
                }
            }
            else
            {
                int r = 0;
                for (int j = n - y; j < n; j++)
                {
                    for (int k = 0; k < TestK.Count; k++)
                        TestY[i][r] += TestK[k][i] * max_norm_array[i][j];
                    r++;
                }
            }
        }
        //пошук кластерів
        public void FindClusters()
        {
            bool nochange = true;//int number_of_steps = 0;
            if (bias == 0)
            {
                clasters.N = n;
                clasters.Am = C;
                clasters.Matrix = incoming_data;
                Clusters.Clear();
                for (int i = 0; i < C; i++)
                {
                    Clusters.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                        Clusters[i].Add(0);
                }
                clasters.ChooseRandomCenters();
                do
                {
                    nochange = true;
                    clasters.CountDistances();
                    clasters.ClassifyVectors();
                    clasters.FindNewCenters();
                    for (int i = 0; i < C; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (Clusters[i][j] != clasters.Centers[i][j])
                            {
                                nochange = false;
                                break;
                            }

                        }
                        if (nochange == false)
                            break;
                    }
                    if (nochange == false)
                    {
                        for (int i = 0; i < C; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                Clusters[i][j] = clasters.Centers[i][j];
                            }
                        }
                    }
                    if (nochange == true)
                    {
                        FileStream str = new FileStream("E:\\Downloads\\clusters.txt", FileMode.Create);
                        StreamWriter writer = new StreamWriter(str);
                        for (int i = 0; i < Clusters.Count; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                writer.Write(clasters.Centers[i][j] + "\t");
                            }
                            writer.WriteLine();
                        }
                        writer.Close();
                        str.Close();
                    }
                    //   number_of_steps++;
                } while (nochange == false);

                clasters.WriteInFiles();
            }
            else
            {
                clasters.N = n;
                clasters.Am = C + 1;
                clasters.Matrix = incoming_data;
                Clusters.Clear();
                for (int i = 0; i < C+1; i++)
                {
                    Clusters.Add(new List<double>());
                    for (int j = 0; j < n; j++)
                        Clusters[i].Add(0);
                }
                clasters.ChooseRandomCenters();
                do
                {
                    nochange = true;
                    clasters.CountDistances();
                    clasters.ClassifyVectors();
                    clasters.FindNewCenters();
                    for (int i = 0; i < C+1; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (Clusters[i][j] != clasters.Centers[i][j])
                            {
                                nochange = false;
                                break;
                            }

                        }
                        if (nochange == false)
                            break;
                    }
                    if (nochange == false)
                    {
                        for (int i = 0; i < C+1; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                Clusters[i][j] = clasters.Centers[i][j];
                            }
                        }
                    }
                    if (nochange == true)
                    {
                        FileStream str = new FileStream("E:\\Downloads\\clusters.txt", FileMode.Create);
                        StreamWriter writer = new StreamWriter(str);
                        for (int i = 0; i < Clusters.Count; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                writer.Write(clasters.Centers[i][j] + "\t");
                            }
                            writer.WriteLine();
                        }
                        writer.Close();
                        str.Close();
                    }
                    //   number_of_steps++;
                } while (nochange == false);

                clasters.WriteInFiles();
            }
        }

        public void FindTestClusters()
        {
            
            bool nochange = true; test_clasters.Clear();
            if (bias == 0)
            {
                for (int i = 0; i < Clusters.Count; i++)
                {
                    test_clasters.Add(new List<double>());
                    for (int j = 0; j < Clusters[i].Count; j++)
                    {
                        test_clasters[i].Add(Clusters[i][j]);
                    }
                }
                Clasterisation cln = new Clasterisation(test_clasters);
                cln.N = n;
                cln.Am = C;
                cln.Matrix = incoming_data;

                do
                {
                    nochange = true;
                    cln.CountDistances();
                    cln.ClassifyVectors();
                    cln.FindNewCenters();
                    for (int i = 0; i < C; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (test_clasters[i][j] != cln.Centers[i][j])
                            {
                                nochange = false;
                                break;
                            }

                        }
                        if (nochange == false)
                            break;
                    }
                    if (nochange == false)
                    {
                        for (int i = 0; i < C; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                test_clasters[i][j] = cln.Centers[i][j];
                            }
                        }
                    }
                    //if (nochange == true)
                    //{
                    //    FileStream str = new FileStream("E:\\Downloads\\clusters.txt", FileMode.Create);
                    //    StreamWriter writer = new StreamWriter(str);
                    //    for (int i = 0; i < Clusters.Count; i++)
                    //    {
                    //        for (int j = 0; j < n; j++)
                    //        {
                    //            writer.Write(clasters.Centers[i][j] + "\t");
                    //        }
                    //        writer.WriteLine();
                    //    }
                    //    writer.Close();
                    //    str.Close();
                    //}

                } while (nochange == false);

                clasters.WriteInFiles();
            }
            else
            {
                for (int i = 0; i < Clusters.Count; i++)
                {
                    test_clasters.Add(new List<double>());
                    for (int j = 0; j < Clusters[i].Count; j++)
                    {
                        test_clasters[i].Add(Clusters[i][j]);
                    }
                }
                Clasterisation cln = new Clasterisation(test_clasters);
                cln.N = n;
                cln.Am = C + 1;
                cln.Matrix = incoming_data;

                do
                {
                    nochange = true;
                    cln.CountDistances();
                    cln.ClassifyVectors();
                    cln.FindNewCenters();
                    for (int i = 0; i < C + 1; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (test_clasters[i][j] != cln.Centers[i][j])
                            {
                                nochange = false;
                                break;
                            }

                        }
                        if (nochange == false)
                            break;
                    }
                    if (nochange == false)
                    {
                        for (int i = 0; i < C + 1; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                               test_clasters[i][j] = cln.Centers[i][j];
                            }
                        }
                    }
                    //if (nochange == true)
                    //{
                    //    FileStream str = new FileStream("E:\\Downloads\\clusters.txt", FileMode.Create);
                    //    StreamWriter writer = new StreamWriter(str);
                    //    for (int i = 0; i < Clusters.Count; i++)
                    //    {
                    //        for (int j = 0; j < n; j++)
                    //        {
                    //            writer.Write(clasters.Centers[i][j] + "\t");
                    //        }
                    //        writer.WriteLine();
                    //    }
                    //    writer.Close();
                    //    str.Close();
                    //}

                } while (nochange == false);

                clasters.WriteInFiles();
            }
        }
    }
}
