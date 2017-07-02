using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Diplom
{
    class Clasterisation
    {
        static List<List<double>> distances;
        static List<List<double>> centers;
        static List<double> center_numbers;
        int n, amount; bool test=false;
        List<List<double>> matrix;
        Random rnd = new Random();
        public Clasterisation()
        {
            distances = new List<List<double>>();
            centers = new List<List<double>>();
            center_numbers = new List<double>();
            matrix = new List<List<double>>();
             
        }

        public Clasterisation(List<List<double>> test_centers)
        {
            distances = new List<List<double>>();
            centers = new List<List<double>>();
            center_numbers = new List<double>();
            matrix = new List<List<double>>();
            for (int i = 0; i < test_centers.Count; i++)
            {
                centers.Add(new List<double>());
                for (int j = 0; j < test_centers[i].Count; j++)
                {
                    centers[i].Add(test_centers[i][j]);
                }
            }
        }

        public List<List<double>> Centers { get { return centers; } }
        public int N { set { n = value; } }
        public int Am { set { amount = value; } }
        public List<List<double>> Matrix { set { matrix = value; } }
        //обираємо рандомно центри кластерів
        public void ChooseRandomCenters()
        {
            
            for (int i = 0; i < amount; i++)
            {
                //centers.Add(new List<double>());
                centers.Add(matrix[rnd.Next(0, matrix.Count)]);
            }
        }
        //рахуєм відстані
        public void CountDistances()
        {
            double res=0;
            for (int i = 0; i < amount; i++)
            {
                distances.Add(new List<double>());
                for (int j = 0; j < matrix.Count; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        res += Math.Pow((centers[i][k] - matrix[j][k]), 2);
                    }
                    distances[i].Add(Math.Sqrt(res));
                    res = 0;
                }
            }
        }
        //знаходимо,до якого кластеру відноситься кожен з векторів
        public void ClassifyVectors()
        {
            double min;int min_point;
            for (int i = 0; i < matrix.Count; i++)
            {
                min = distances[0][i];min_point = 0;
                for (int j = 0; j < amount; j++)
                {
                    if (distances[j][i] < min)
                    { min = distances[j][i]; min_point = j; }
                }
                center_numbers.Add(min_point);
            }

        }
        //перераховуємо центри
        public void FindNewCenters()
        {
            List<double> sum=new List<double>();int counter;
            centers.Clear();distances.Clear();
            for (int i = 0; i < amount; i++)
            {
                centers.Add(new List<double>());
                for (int k = 0; k < n; k++)
                {
                    sum.Add(0);
                }
                counter = 0;
                for (int j = 0; j < matrix.Count; j++)
                {
                    if (center_numbers[j] == i)
                    {
                        sum = StringsSum(sum, matrix[j]);
                        counter++;
                    }
                }
                foreach (double val in sum)
                {
                    centers[i].Add(val / counter);
                }
                counter = 0;sum.Clear();
            }
            
        }
        //метод для додавання 2-х масивів
        List<double> StringsSum(List<double> string1, List<double> string2)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < string1.Count; i++)
            {
                result.Add(string1[i] + string2[i]);
            }
            return result;
        }

        public void WriteInFiles()
        {
            if (test == false)
            {
                FileStream str = new FileStream("E:\\Downloads\\numbers_of_clusters_matrix.txt", FileMode.Create);
                StreamWriter writer = new StreamWriter(str);
                for (int i = 0; i < matrix.Count; i++)
                {
                    for (int j = 0; j < n - 1; j++)
                    {
                        writer.Write(matrix[i][j] + "\t");
                    }
                    for (int k = 0; k < amount; k++)
                    {
                        if (center_numbers[i] == k)
                            writer.Write("1" + "\t");
                        else writer.Write("0" + "\t");
                    }
                    writer.Write(matrix[i][n - 1]);
                    writer.WriteLine();
                }
                writer.Close();
                str.Close();

                str = new FileStream("E:\\Downloads\\numbers_of_clusters.txt", FileMode.Create);
                writer = new StreamWriter(str);
                for (int i = 0; i < matrix.Count; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        writer.Write(matrix[i][j] + "\t");
                    }
                    writer.Write(center_numbers[i]);
                    writer.WriteLine();
                }
                writer.Close();
                str.Close();
                center_numbers.Clear();

                test = true;
            }
            else
            {
                FileStream str = new FileStream("E:\\Downloads\\numbers_of_clusters_test_matrix.txt", FileMode.Create);
                StreamWriter writer = new StreamWriter(str);
                for (int i = 0; i < matrix.Count; i++)
                {
                    for (int j = 0; j < n - 1; j++)
                    {
                        writer.Write(matrix[i][j] + "\t");
                    }
                    for (int k = 0; k < amount; k++)
                    {
                        if (center_numbers[i] == k)
                            writer.Write("1" + "\t");
                        else writer.Write("0" + "\t");
                    }
                    writer.Write(matrix[i][n - 1]);
                    writer.WriteLine();
                }
                writer.Close();
                str.Close();

                str = new FileStream("E:\\Downloads\\numbers_of_test_clusters.txt", FileMode.Create);
                writer = new StreamWriter(str);
                for (int i = 0; i < matrix.Count; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        writer.Write(matrix[i][j] + "\t");
                    }
                    writer.Write(center_numbers[i]);
                    writer.WriteLine();
                }
                writer.Close();
                str.Close();
                center_numbers.Clear();

                test = false;
            }
        }
    }
}
