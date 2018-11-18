using Accord.Controls;
using Accord.Math;
using Accord.Statistics.Visualizations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace CS_7850_RR_Classifier
{
    class Program
    {

        //Not disguising the testing dataset, so don't need to worry about that calculation.
        static void Main(string[] args)
        {
            double theta = 0;

            DataTable incomeDataset = LoadIncomeDataset();
            DataTable incomeTrainingDataset = SplitIncomeDataset(incomeDataset, 0);
            DataTable incomeTestingDataset = SplitIncomeDataset(incomeDataset, 1);
            double benchmarkIncomeAccuracy = IncomeDecision(incomeTrainingDataset, incomeTestingDataset);

            
            double[] accuracyFor0 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.1;
            double[] accuracyFor01 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.2;
            double[] accuracyFor02 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.3;
            double[] accuracyFor03 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.4;
            double[] accuracyFor04 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.45;
            double[] accuracyFor045 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.51;
            double[] accuracyFor051 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.55;
            double[] accuracyFor055 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.6;
            double[] accuracyFor06 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.7;
            double[] accuracyFor07 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.8;
            double[] accuracyFor08 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 0.9;
            double[] accuracyFor09 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);
            theta = 1.0;
            double[] accuracyFor10 = testRandomization(incomeTrainingDataset, incomeTestingDataset, theta);

            //Get accuracy means for each theta
            double meanFor0 = Accord.Statistics.Measures.Mean(accuracyFor0);
            double meanFor01 = Accord.Statistics.Measures.Mean(accuracyFor01);
            double meanFor02 = Accord.Statistics.Measures.Mean(accuracyFor02);
            double meanFor03 = Accord.Statistics.Measures.Mean(accuracyFor03);
            double meanFor04 = Accord.Statistics.Measures.Mean(accuracyFor04);
            double meanFor045 = Accord.Statistics.Measures.Mean(accuracyFor045);
            double meanFor051 = Accord.Statistics.Measures.Mean(accuracyFor051);
            double meanFor055 = Accord.Statistics.Measures.Mean(accuracyFor055);
            double meanFor06 = Accord.Statistics.Measures.Mean(accuracyFor06);
            double meanFor07 = Accord.Statistics.Measures.Mean(accuracyFor07);
            double meanFor08 = Accord.Statistics.Measures.Mean(accuracyFor08);
            double meanFor09 = Accord.Statistics.Measures.Mean(accuracyFor09);
            double meanFor10 = Accord.Statistics.Measures.Mean(accuracyFor10);
            Dictionary<double, double> means = new Dictionary<double, double>();
            means.Add(0, meanFor0);
            means.Add(0.1, meanFor01);
            means.Add(0.2, meanFor02);
            means.Add(0.3, meanFor03);
            means.Add(0.4, meanFor04);
            means.Add(0.45, meanFor045);
            means.Add(0.51, meanFor051);
            means.Add(0.55, meanFor055);
            means.Add(0.6, meanFor06);
            means.Add(0.7, meanFor07);
            means.Add(0.8, meanFor08);
            means.Add(0.9, meanFor09);
            means.Add(1, meanFor10);

            //Get accuracy variances for each theta
            double varianceFor0 = Accord.Statistics.Measures.Variance(accuracyFor0);
            double varianceFor01 = Accord.Statistics.Measures.Variance(accuracyFor01);
            double varianceFor02 = Accord.Statistics.Measures.Variance(accuracyFor02);
            double varianceFor03 = Accord.Statistics.Measures.Variance(accuracyFor03);
            double varianceFor04 = Accord.Statistics.Measures.Variance(accuracyFor04);
            double varianceFor045 = Accord.Statistics.Measures.Variance(accuracyFor045);
            double varianceFor051 = Accord.Statistics.Measures.Variance(accuracyFor051);
            double varianceFor055 = Accord.Statistics.Measures.Variance(accuracyFor055);
            double varianceFor06 = Accord.Statistics.Measures.Variance(accuracyFor06);
            double varianceFor07 = Accord.Statistics.Measures.Variance(accuracyFor07);
            double varianceFor08 = Accord.Statistics.Measures.Variance(accuracyFor08);
            double varianceFor09 = Accord.Statistics.Measures.Variance(accuracyFor09);
            double varianceFor10 = Accord.Statistics.Measures.Variance(accuracyFor10);
            Dictionary<double, double> variances = new Dictionary<double, double>();
            variances.Add(0, varianceFor0);
            variances.Add(0.1, varianceFor01);
            variances.Add(0.2, varianceFor02);
            variances.Add(0.3, varianceFor03);
            variances.Add(0.4, varianceFor04);
            variances.Add(0.45, varianceFor045);
            variances.Add(0.51, varianceFor051);
            variances.Add(0.55, varianceFor055);
            variances.Add(0.6, varianceFor06);
            variances.Add(0.7, varianceFor07);
            variances.Add(0.8, varianceFor08);
            variances.Add(0.9, varianceFor09);
            variances.Add(1, varianceFor10);

            double[] thetas = means.Keys.ToArray<double>();
            double[] meanValues = means.Values.ToArray<double>();
            double[] varianceValues = variances.Values.ToArray<double>();


            int max = thetas.Length;

            double[] benchmarkAccuracies = new double[max];
            double[] benchmarkVariances = new double[max];

            //Create graph for means
            for (int i = 0; i < max; i++)
            {
                benchmarkAccuracies[i] = benchmarkIncomeAccuracy;
            }

            ScatterplotView meanPlot = new ScatterplotView();
            meanPlot.Dock = DockStyle.Fill;
            meanPlot.LinesVisible = true;

            meanPlot.Graph.GraphPane.AddCurve("Curve 1", thetas, meanValues, Color.Red, SymbolType.Circle);
            meanPlot.Graph.GraphPane.AddCurve("Curve 2", thetas, benchmarkAccuracies, Color.Blue, SymbolType.Diamond);

            meanPlot.Graph.GraphPane.AxisChange();

            Form meanForm = new Form();
            meanForm.Width = 600;
            meanForm.Height = 400;
            meanForm.Controls.Add(meanPlot);
            meanForm.ShowDialog();

            //Create graph for variances
            for (int i = 0; i < max; i++)
            {
                benchmarkVariances[i] = 0;
            }

            ScatterplotView variancePlot = new ScatterplotView();
            variancePlot.Dock = DockStyle.Fill;
            variancePlot.LinesVisible = true;

            variancePlot.Graph.GraphPane.AddCurve("Curve 1", thetas, varianceValues, Color.Red, SymbolType.Circle);
            variancePlot.Graph.GraphPane.AddCurve("Curve 2", thetas, benchmarkVariances, Color.Blue, SymbolType.Diamond);

            variancePlot.Graph.GraphPane.AxisChange();

            Form varianceForm = new Form();
            varianceForm.Width = 600;
            varianceForm.Height = 400;
            varianceForm.Controls.Add(variancePlot);
            varianceForm.ShowDialog();
        }

        //Method to load in a dataset from one of the CSVs (training or testing)
        //trainingOrTesting variable: = 0 means get training data, = 1 means get testing data
        public static DataTable LoadIncomeDataset()
        {
            var rowCount = 0;
            StreamReader reader;
            DataTable dataset = new DataTable("Income Dataset");

            //Stripping out all discrete string data - unsure how to binarize.
            dataset.Columns.Add("RowNumber");
            dataset.Columns.Add("Age");
            //dataset.Columns.Add("WorkClass");
            dataset.Columns.Add("FinalWeight");
            //dataset.Columns.Add("Education");
            dataset.Columns.Add("EducationNum");
            //dataset.Columns.Add("MaritalStatus");
            //dataset.Columns.Add("Occupation");
            //dataset.Columns.Add("Relationship");
            //dataset.Columns.Add("Race");
            dataset.Columns.Add("Sex");
            dataset.Columns.Add("CapitalGain");
            dataset.Columns.Add("CapitalLoss");
            dataset.Columns.Add("HoursPerWeek");
            //dataset.Columns.Add("NativeCountry");
            dataset.Columns.Add("SalaryLabel");

            reader = new StreamReader("Datasets/paper-dataset-income-top10k.data");
            List<double> ages = new List<double>();
            List<double> finalWeights = new List<double>();
            List<double> educationNums = new List<double>();
            List<double> capitalGains = new List<double>();
            List<double> capitalLosses = new List<double>();
            List<double> hoursPerWeeks = new List<double>();
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                //Strip whitespace from csv line
                var strippedLine = Regex.Replace(line, @"\s+", "");
                var values = strippedLine.Split(',');
    
                

                //Add row to dataset 
                dataset.Rows.Add(rowCount, values[0], values[2], values[4], values[9], values[10], values[11], values[12], values[14]);

                ages.Add(Convert.ToDouble(values[0]));
                finalWeights.Add(Convert.ToDouble(values[2]));
                educationNums.Add(Convert.ToDouble(values[4]));
                capitalGains.Add(Convert.ToDouble(values[10]));
                capitalLosses.Add(Convert.ToDouble(values[11]));
                hoursPerWeeks.Add(Convert.ToDouble(values[12]));

                //Increment row count
                rowCount++;
            }

            List<double> mediansOfValues = new List<double>();

            double medianAge = GetMedianFromArray(ages.ToArray<double>());
            double medianFinalWeight = GetMedianFromArray(finalWeights.ToArray<double>());
            double medianEducationNum = GetMedianFromArray(educationNums.ToArray<double>());
            double medianCapitalGain = GetMedianFromArray(capitalGains.ToArray<double>());
            double medianCapitalLoss = GetMedianFromArray(capitalLosses.ToArray<double>());
            double medianHoursPerWeek = GetMedianFromArray(hoursPerWeeks.ToArray<double>());

            mediansOfValues.Add(medianAge);
            mediansOfValues.Add(medianFinalWeight);
            mediansOfValues.Add(medianEducationNum);
            mediansOfValues.Add(medianCapitalGain);
            mediansOfValues.Add(medianCapitalLoss);
            mediansOfValues.Add(medianHoursPerWeek);

            DataTable binarizedDataset = BinarizeDataset(dataset, mediansOfValues);

            return dataset;
        }

        public static double GetMedianFromArray(double[] values)
        {
            double median;
            //sort the array
            Array.Sort(values);
            //Get min-max range of attribute and get median of range
            double max = values.Last();
            double min = values.First();
            median = (min + max) / 2.0;

            return median;
        }

        public static DataTable BinarizeDataset(DataTable dataset, List<double> medians)
        {
            double medianAge = medians.ElementAt(0);
            double medianFinalWeight = medians.ElementAt(1);
            double medianEducationNum = medians.ElementAt(2);
            double medianCapitalGain = medians.ElementAt(3);
            double medianCapitalLoss = medians.ElementAt(4);
            double medianHoursPerWeek = medians.ElementAt(5);


            foreach (DataRow row in dataset.Rows)
            {
                var age = row["Age"];
                var finalWeight = row["FinalWeight"];
                var educationNum = row["EducationNum"];
                var sex = row["Sex"];
                var capitalGain = row["CapitalGain"];
                var capitalLoss = row["CapitalLoss"];
                var hoursPerWeek = row["HoursPerWeek"];
                var salary = row["SalaryLabel"];

                var binAge = 0;
                var binFinalWeight = 0;
                var binEducationNum = 0;
                var binCapitalGain = 0;
                var binCapitalLoss = 0;
                var binHoursPerWeek = 0;
                var binSex = 0;
                var binSalary = 0;


                //Binarize data based on medians of range
                if (Convert.ToInt32(age) <= medianAge)
                {
                    binAge = 0;
                }
                else
                {
                    binAge = 1;
                }

                if (Convert.ToInt32(finalWeight) <= medianFinalWeight)
                {
                    binFinalWeight = 0;
                }
                else
                {
                    binFinalWeight = 1;
                }

                if (Convert.ToInt32(educationNum) <= medianEducationNum)
                {
                    binEducationNum = 0;
                }
                else
                {
                    binEducationNum = 1;
                }

                if (sex.ToString() == "Male")
                {
                    binSex = 0;
                }
                else
                {
                    binSex = 1;
                }

                if (Convert.ToInt32(capitalGain) <= medianCapitalGain)
                {
                    binCapitalGain = 0;
                }
                else
                {
                    binCapitalGain = 1;
                }

                if (Convert.ToInt32(capitalLoss) <= medianCapitalLoss)
                {
                    binCapitalLoss = 0;
                }
                else
                {
                    binCapitalLoss = 1;
                }

                if (Convert.ToInt32(hoursPerWeek) <= medianHoursPerWeek)
                {
                    binHoursPerWeek = 0;
                }
                else
                {
                    binHoursPerWeek = 1;
                }

                if (salary.ToString() == "<=50K")
                {
                    binSalary = 0;
                }
                else
                {
                    binSalary = 1;
                }

                row["Age"] = binAge;
                row["FinalWeight"] = binFinalWeight;
                row["EducationNum"] = binEducationNum;
                row["Sex"] = binSex;
                row["CapitalGain"] = binCapitalGain;
                row["CapitalLoss"] = binCapitalLoss;
                row["HoursPerWeek"] = binHoursPerWeek;
                row["SalaryLabel"] = binSalary;
            }

            return dataset;
        }

        //trainingOrTesting = 0 means training dataset, 1 means testing dataset
        public static DataTable SplitIncomeDataset(DataTable incomeDataset, int trainingOrTesting)
        {
            DataTable dataset;
            int startIndex, endIndex;
            if (trainingOrTesting == 0)
            {
                dataset = new DataTable("Income Training Dataset");
                startIndex = 0;
                endIndex = incomeDataset.Rows.Count / 2;
            }
            else
            {
                dataset = new DataTable("Income Testing Dataset");
                startIndex = incomeDataset.Rows.Count / 2;
                endIndex = incomeDataset.Rows.Count;
            }

            dataset.Columns.Add("RowNumber");
            dataset.Columns.Add("Age");
            dataset.Columns.Add("FinalWeight");
            dataset.Columns.Add("EducationNum");
            dataset.Columns.Add("Sex");
            dataset.Columns.Add("CapitalGain");
            dataset.Columns.Add("CapitalLoss");
            dataset.Columns.Add("HoursPerWeek");
            dataset.Columns.Add("SalaryLabel");

            int rowCount = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                DataRow currentRow = incomeDataset.Rows[i];
                //Generate new number from 0 to 1 (including 1, not including 0).
                //1 - rand is because NextDouble generates from 0 inclusive to 1 non-inclusive.  1 - rand
                //return 0 non-inclusive to 1 inclusive.
                //If generated number is greater than the theta provided, leave the response unchanged
                //Get current row's salary response
                var age = currentRow["Age"];
                var finalWeight = currentRow["FinalWeight"];
                var educationNum = currentRow["EducationNum"];
                var sex = currentRow["Sex"];
                var capitalGain = currentRow["CapitalGain"];
                var capitalLoss = currentRow["CapitalLoss"];
                var hoursPerWeek = currentRow["HoursPerWeek"];
                var salary = currentRow["SalaryLabel"];

                dataset.Rows.Add(rowCount, age, finalWeight, educationNum, sex, capitalGain, capitalLoss, hoursPerWeek, salary);

                rowCount++;
            }
            return dataset;
        }

        //trainingOrTesting variable: = 0 means get training data, = 1 means get testing data
        //Theta is the likelihood of flipping the response.
        public static DataTable RandomizeIncomeDataset(DataTable incomeDataset, int trainingOrTesting, double theta)
        {
            DataTable randomizedDataset = new DataTable("Randomized Income Dataset");
            randomizedDataset = incomeDataset.Copy();
            //Initialize RNG
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            //Iterate through all rows of datatable
            foreach (DataRow row in randomizedDataset.Rows)
            {
                //Generate new number from 0 to 1 (including 1, not including 0).
                //1 - rand is because NextDouble generates from 0 inclusive to 1 non-inclusive.  1 - rand
                //return 0 non-inclusive to 1 inclusive.
                double generatedNumber = 1 - rand.NextDouble();
                //If generated number is greater than the theta provided, leave the response unchanged
                if (generatedNumber > theta)
                {
                    //Get current row's salary response
                    var age = row["Age"];
                    var finalWeight = row["FinalWeight"];
                    var educationNum = row["EducationNum"];
                    var sex = row["Sex"];
                    var capitalGain = row["CapitalGain"];
                    var capitalLoss = row["CapitalLoss"];
                    var hoursPerWeek = row["HoursPerWeek"];

                    int newAge = 0;
                    int newFinalWeight = 0;
                    int newEducationNum = 0;
                    int newSex = 0;
                    int newCapitalGain = 0;
                    int newCapitalLoss = 0;
                    int newHoursPerWeek = 0;

                    //Flip attributes
                    if (Convert.ToInt32(age) == 0)
                    {
                        newAge = 1;
                    }
                    if (Convert.ToInt32(finalWeight) == 0)
                    {
                        newFinalWeight = 1;
                    }
                    if (Convert.ToInt32(educationNum) == 0)
                    {
                        newEducationNum = 1;
                    }
                    if (Convert.ToInt32(sex) == 0)
                    {
                        newSex = 1;
                    }
                    if (Convert.ToInt32(capitalGain) == 0)
                    {
                        newCapitalGain = 1;
                    }
                    if (Convert.ToInt32(capitalLoss) == 0)
                    {
                        newCapitalLoss = 1;
                    }
                    if (Convert.ToInt32(hoursPerWeek) == 0)
                    {
                        newHoursPerWeek = 1;
                    }


                    //Insert flipped attributes into datatable.
                    row["Age"] = newAge;
                    row["FinalWeight"] = newFinalWeight;
                    row["EducationNum"] = newEducationNum;
                    row["Sex"] = newSex;
                    row["CapitalGain"] = newCapitalGain;
                    row["CapitalLoss"] = newCapitalLoss;
                    row["HoursPerWeek"] = newHoursPerWeek;
                }

            }

            return randomizedDataset;
        }
        public static DataTable negateInputs(DataTable tableToNegate)
        {
            DataTable negatedTable = new DataTable("Negated Dataset");
            negatedTable = tableToNegate.Copy();
            //Iterate through all rows of datatable
            foreach (DataRow row in negatedTable.Rows)
            {
                //Get current row's salary response
                var age = row["Age"];
                var finalWeight = row["FinalWeight"];
                var educationNum = row["EducationNum"];
                var sex = row["Sex"];
                var capitalGain = row["CapitalGain"];
                var capitalLoss = row["CapitalLoss"];
                var hoursPerWeek = row["HoursPerWeek"];

                int newAge = 0;
                int newFinalWeight = 0;
                int newEducationNum = 0;
                int newSex = 0;
                int newCapitalGain = 0;
                int newCapitalLoss = 0;
                int newHoursPerWeek = 0;

                //Flip salary
                if (Convert.ToInt32(age) == 0)
                {
                    newAge = 1;
                }
                if (Convert.ToInt32(finalWeight) == 0)
                {
                    newFinalWeight = 1;
                }
                if (Convert.ToInt32(educationNum) == 0)
                {
                    newEducationNum = 1;
                }
                if (Convert.ToInt32(sex) == 0)
                {
                    newSex = 1;
                }
                if (Convert.ToInt32(capitalGain) == 0)
                {
                    newCapitalGain = 1;
                }
                if (Convert.ToInt32(capitalLoss) == 0)
                {
                    newCapitalLoss = 1;
                }
                if (Convert.ToInt32(hoursPerWeek) == 0)
                {
                    newHoursPerWeek = 1;
                }

                //Insert flipped attributes into datatable.
                row["Age"] = newAge;
                row["FinalWeight"] = newFinalWeight;
                row["EducationNum"] = newEducationNum;
                row["Sex"] = newSex;
                row["CapitalGain"] = newCapitalGain;
                row["CapitalLoss"] = newCapitalLoss;
                row["HoursPerWeek"] = newHoursPerWeek;
            }

            return negatedTable;
        }
        public static double IncomeDecision(DataTable incomeTrainingDataset, DataTable incomeTestingDataset)
        {
            //Get arrays for training inputs and outputs.
            int[][] trainingInputs = incomeTrainingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] trainingOutputs = incomeTrainingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Build tree from learning
            Trie id3Trie = new Trie(trainingInputs, trainingOutputs, trainingOutputs.Length);

            //Get arrays for testing inputs and outputs
            int[][] testingInputs = incomeTestingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] testingOutputs = incomeTestingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Run classifier on testing inputs and get classified outputs
            int[] treeTestingOutputs = id3Trie.Decide(testingInputs);

            //Calculate accuracy
            double totalOutputs = 0;
            double totalCorrectOutputs = 0;

            for (int i = 0; i < testingOutputs.Length; i++)
            {
                if (testingOutputs[i] == treeTestingOutputs[i])
                {
                    totalCorrectOutputs++;
                }
                //If -1 is in any outputs, something went wrong.  This shouldn't be reached - it's for debugging.
                if(treeTestingOutputs[i] == -1)
                {
                    return -1;
                }

                totalOutputs++;
            }

            double accuracy = totalCorrectOutputs / totalOutputs;
            return accuracy;
        }

        public static double ModifiedIncomeDecision(DataTable incomeTrainingDataset, DataTable incomeTestingDataset, double theta)
        {
            //Get arrays for training inputs and outputs.
            int[][] trainingInputs = incomeTrainingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] trainingOutputs = incomeTrainingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Build tree from learning
            ModifiedTrie id3Trie = new ModifiedTrie(trainingInputs, trainingOutputs, trainingOutputs.Length, theta);
            

            //Get arrays for testing inputs and outputs
            int[][] testingInputs = incomeTestingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] testingOutputs = incomeTestingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Run classifier on testing inputs and get classified outputs
            int[] treeTestingOutputs = id3Trie.Decide(testingInputs);

            //Calculate accuracy
            double totalOutputs = 0;
            double totalCorrectOutputs = 0;

            for (int i = 0; i < testingOutputs.Length; i++)
            {
                if (testingOutputs[i] == treeTestingOutputs[i])
                {
                    totalCorrectOutputs++;
                }
                //If -1 is in any outputs, something went wrong.  This shouldn't be reached - it's for debugging.
                if (treeTestingOutputs[i] == -1)
                {
                    return -1;
                }

                totalOutputs++;
            }

            double accuracy = totalCorrectOutputs / totalOutputs;
            return accuracy;
        }

        public static double[] testRandomization(DataTable incomeTrainingDataset, DataTable incomeTestingDataset, double theta)
        {
            List<double> accuracyMeasurements = new List<double>();
            double randomizedIncomeAccuracy;
            for (int i = 0; i < 3; i++)
            {
                DataTable randomizedIncomeTrainingDataset = RandomizeIncomeDataset(incomeTrainingDataset, 0, theta);
                //The commented out code below flips the dataset at thetas under 0.5, greatly reducing the performance degradation by artifically keeping theta rates
                //always above 0.5 for the purposes of the classifier.
                //if(theta < 0.5)
                //{
                //    randomizedIncomeTrainingDataset = negateInputs(randomizedIncomeTrainingDataset);
                //    randomizedIncomeAccuracy = ModifiedIncomeDecision(randomizedIncomeTrainingDataset, incomeTestingDataset, 1 - theta);
                //}
                //else
                //{
                    randomizedIncomeAccuracy = ModifiedIncomeDecision(randomizedIncomeTrainingDataset, incomeTestingDataset, theta);
                //}
                accuracyMeasurements.Add(randomizedIncomeAccuracy);
            }
            double[] accuracyArray = accuracyMeasurements.ToArray<double>();

            return accuracyArray;
        }
    }
}
