using Accord.Controls;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Statistics.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CS_7850_Classifier
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable incomeTrainingDataset = LoadIncomeDataset(0);
            DataTable incomeTestingDataset = LoadIncomeDataset(1);
            IncomeDecision(incomeTrainingDataset, incomeTestingDataset);
        }

        //Method to load in a dataset from one of the CSVs (training or testing)
        //trainingOrTesting variable: = 0 means get training data, = 1 means get testing data
        public static DataTable LoadIncomeDataset(int trainingOrTesting)
        {
            var rowCount = 0;
            StreamReader reader;
            DataTable dataset;
            if (trainingOrTesting == 0)
            {
                dataset = new DataTable("Income Training Dataset");
            }
            else
            {
                dataset = new DataTable("Income Testing Dataset");
            }
            

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
            dataset.Columns.Add("Salary");

            //Ranges and median of range for each continuous column:
            //Age: 17 - 90; Median: 53.5
            //FinalWeight: 19302 - 1226583; Median: 622942.5
            //EducationNum: 1 - 16; Median: 8.5
            //CapitalGain: 0 - 99999; Median: 49999.5
            //CapitalLoss: 0 - 4356; Median: 2178
            //HoursPerWeek: 1 - 99; Median: 50


            //Read in data
            if(trainingOrTesting == 0)
            {
                reader = new StreamReader("Datasets/paper-dataset-income-training.data");
            }
            else
            {
                reader = new StreamReader("Datasets/paper-dataset-income-testing.data");
            }
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                //Strip whitespace from csv line
                var strippedLine = Regex.Replace(line, @"\s+", "");
                var values = strippedLine.Split(',');
                var age = 0;
                var finalWeight = 0;
                var educationNum = 0;
                var capitalGain = 0;
                var capitalLoss = 0;
                var hoursPerWeek = 0;
                var sex = 0;
                var salary = 0;

                //Binarize data based on medians of range
                if(Convert.ToInt32(values[0]) <= 53.5)
                {
                    age = 0;
                }
                else
                {
                    age = 1;
                }

                if (Convert.ToInt32(values[2]) <= 622942.5)
                {
                    finalWeight = 0;
                }
                else
                {
                    finalWeight = 1;
                }

                if (Convert.ToInt32(values[4]) <= 8.5)
                {
                    educationNum = 0;
                }
                else
                {
                    educationNum = 1;
                }

                if (values[9] == "Male")
                {
                    sex = 0;
                }
                else
                {
                    sex = 1;
                }

                if (Convert.ToInt32(values[10]) <= 49999.5)
                {
                    capitalGain = 0;
                }
                else
                {
                    capitalGain = 1;
                }

                if (Convert.ToInt32(values[11]) <= 2178)
                {
                    capitalLoss = 0;
                }
                else
                {
                    capitalLoss = 1;
                }

                if (Convert.ToInt32(values[12]) <= 50)
                {
                    hoursPerWeek = 0;
                }
                else
                {
                    hoursPerWeek = 1;
                }

                if (values[14] == "<=50K")
                {
                    salary = 0;
                }
                else
                {
                    salary = 1;
                }

                //Add row to dataset 
                dataset.Rows.Add(rowCount, age, finalWeight, educationNum, sex, capitalGain, capitalLoss, hoursPerWeek, salary);

                //Increment row count
                rowCount++;
            }

            return dataset;

        }

        //Run decision tree with trainig and testing datasets given.
        public static void IncomeDecision(DataTable incomeTrainingDataset, DataTable incomeTestingDataset)
        {
            //Set up which attributes from dataset will be used for the decision tree.
            DecisionVariable[] attributes =
            {
                new DecisionVariable("Age", 2), new DecisionVariable("FinalWeight", 2), new DecisionVariable("EducationNum", 2),
                new DecisionVariable("Sex", 2), new DecisionVariable("CapitalGain", 2), new DecisionVariable("CapitalLoss", 2),
                new DecisionVariable("HoursPerWeek", 2)
            };

            //Set up how many output classes there will be (2; <=50k, >50k)
            int outputClassCount = 2;

            //Create decision tree
            DecisionTree tree = new DecisionTree(attributes, outputClassCount);


            ID3Learning id3Algorithm = new ID3Learning(tree);

            //Get arrays for training inputs and outputs.
            int[][] trainingInputs = incomeTrainingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] trainingOutputs = incomeTrainingDataset.ToJagged<int>("Salary").GetColumn(0);

            //Build tree from learning
            id3Algorithm.Learn(trainingInputs, trainingOutputs);

            //Get arrays for testing inputs and outputs
            int[][] testingInputs = incomeTestingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] testingOutputs = incomeTestingDataset.ToJagged<int>("Salary").GetColumn(0);

            //Run classifier on testing inputs and get classified outputs
            int[] treeTestingOutputs = tree.Decide(testingInputs);

            //ScatterplotBox.Show();
            //ScatterplotBox.Show("Expected vs. Real", testingOutputs, treeTestingOutputs);
            //ScatterplotBox.Show("Expected results", testingInputs, testingOutputs);
            //ScatterplotBox.Show("Decision Tree results", testingInputs, treeTestingOutputs)
            //    .Hold();


            //Calculate accuracy
            double totalOutputs = 0;
            double totalCorrectOutputs = 0;
                        
            for(int i = 0; i < testingOutputs.Length; i++)
            {
                if(testingOutputs[i] == treeTestingOutputs[i])
                {
                    totalCorrectOutputs++;
                }

                totalOutputs++;
            }

            //76.85% accuracy on base dataset - slightly lower than paper
            //This is likely due to stripping out string-based attributes as well as simply a different ID3 algorithm.
            double accuracyScore = totalCorrectOutputs / totalOutputs;

        }
    }
}
