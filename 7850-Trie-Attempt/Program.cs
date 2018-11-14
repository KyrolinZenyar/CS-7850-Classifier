using Accord.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _7850_Trie_Attempt
{
    class Program
    {

        //Not disguising the testing dataset, so don't need to worry about that calculation.
        static void Main(string[] args)
        {
            double theta = 0.51;
            DataTable incomeTrainingDataset = LoadIncomeDataset(0);
            DataTable incomeTestingDataset = LoadIncomeDataset(1);
            double benchmarkIncomeAccuracy = IncomeDecision(incomeTrainingDataset, incomeTestingDataset);
            DataTable randomizedIncomeTrainingDataset01 = RandomizeIncomeDataset(incomeTrainingDataset, 0, theta);
            DataTable randomizedIncomeTestingDataset01 = RandomizeIncomeDataset(incomeTestingDataset, 1, theta);
            //double randomizedIncomeAccuracy = ModifiedIncomeDecision(randomizedIncomeTrainingDataset01, randomizedIncomeTestingDataset01, theta);
            //double randomizedIncomeAccuracy = IncomeDecision(randomizedIncomeTrainingDataset01, randomizedIncomeTestingDataset01);
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
            dataset.Columns.Add("SalaryLabel");

            //Ranges and median of range for each continuous column:
            //Age: 17 - 90; Median: 53.5
            //FinalWeight: 19302 - 1226583; Median: 622942.5
            //EducationNum: 1 - 16; Median: 8.5
            //CapitalGain: 0 - 99999; Median: 49999.5
            //CapitalLoss: 0 - 4356; Median: 2178
            //HoursPerWeek: 1 - 99; Median: 50


            //Read in data
            if (trainingOrTesting == 0)
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
                if (Convert.ToInt32(values[0]) <= 53.5)
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
                    //dataset.Columns.Add("SalaryLabel");
                    //int newSalary = 0;   
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
                    //row["Salary"] = newSalary;
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

        public static double IncomeDecision(DataTable incomeTrainingDataset, DataTable incomeTestingDataset)
        {
            //Set up which attributes from dataset will be used for the decision tree.
            //DecisionVariable[] attributes =
            //{
            //    new DecisionVariable("Age", 2), new DecisionVariable("FinalWeight", 2), new DecisionVariable("EducationNum", 2),
            //    new DecisionVariable("Sex", 2), new DecisionVariable("CapitalGain", 2), new DecisionVariable("CapitalLoss", 2),
            //    new DecisionVariable("HoursPerWeek", 2)
            //};

            //Set up how many output classes there will be (2; <=50k, >50k)
            //int outputClassCount = 2;

            //Create decision tree
            //DecisionTree tree = new DecisionTree(attributes, outputClassCount);


            //ID3Learning id3Algorithm = new ID3Learning(tree);
            //Get arrays for training inputs and outputs.
            int[][] trainingInputs = incomeTrainingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] trainingOutputs = incomeTrainingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Build tree from learning
            //id3Algorithm.Learn(trainingInputs, trainingOutputs);
            Trie id3Trie = new Trie(trainingInputs, trainingOutputs, trainingOutputs.Length);

            //Get arrays for testing inputs and outputs
            int[][] testingInputs = incomeTestingDataset.ToJagged<int>("Age", "FinalWeight", "EducationNum", "Sex", "CapitalGain", "CapitalLoss", "HoursPerWeek");
            int[] testingOutputs = incomeTestingDataset.ToJagged<int>("SalaryLabel").GetColumn(0);

            //Run classifier on testing inputs and get classified outputs
            int[] treeTestingOutputs = id3Trie.Decide(testingInputs);

            //ScatterplotBox.Show();
            //ScatterplotBox.Show("Expected vs. Real", testingOutputs, treeTestingOutputs);
            //ScatterplotBox.Show("Expected results", testingInputs, testingOutputs);
            //ScatterplotBox.Show("Decision Tree results", testingInputs, treeTestingOutputs)
            //    .Hold();


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
    }
}
