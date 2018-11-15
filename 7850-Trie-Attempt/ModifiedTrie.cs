using Accord.Math;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_7850_RR_Classifier
{
    //Basic Trie code taken and heavily modified from
    //https://stackoverflow.com/questions/6416050/how-to-create-a-trie-in-c-sharp

    public class ModifiedTrie
    {
        public class Node
        {
            public int Class;
            public int Attribute;
            public bool IsTerminal { get { return Class != -1; } }
            //the int is the value of the attribute (0 or 1), the Node is the next node.
            public Dictionary<int, Node> Edges = new Dictionary<int, Node>();

            //expressionE is a Dictionary used to set up the attribute expression as described in the paper - the first int is the attribute number and the second int is the value
            //, Dictionary<int, int> expressionE

            //P*(E) is the number of elements in outputs (as the tree goes down the edges, outputs shrinks; the number of items in outputs is equivalent 
            //to P*(E) where E is the attributes as given so far.

            public Node(int[][] inputs, int[] outputs, int[] attributes, int height, int majorityClassOfParentNode, double theta, Dictionary<int, int> expressionE)
            {
                //If outputs is empty, set the class to be the majority class of the parent node
                if (outputs.Length == 0) {
                    Class = majorityClassOfParentNode;
                }
                else { 


                    //Set class to -1 to mark as non-leaf unless it gets changed.
                    Class = -1;

                    //2. Check if S consists of all the same class - if so, return node as a leaf labeled with the class
                    int numberOfClassZeroItems = 0;
                    //Run through all class labels and tally how many are 0.
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        if (outputs[i] == 0)
                        {
                            numberOfClassZeroItems++;
                        }
                    }

                    //If all or none of the class labels are 0, then all items are one class - label as a leaf and assign class label.
                    if (numberOfClassZeroItems == 0 || numberOfClassZeroItems == outputs.Length)
                    {
                        Class = outputs[0];
                        //IsTerminal = true;
                    }
                    else
                    {


                        //3. If attribute list is empty, return node as leaf labeled with majority class in S
                        if (attributes.Length == 0)
                        {
                            Class = Accord.Statistics.Measures.Mode(outputs);
                            Attribute = -1;
                            //IsTerminal = true;
                        }
                        else
                        {
                            double maxInfoGain = 0;
                            int maxInfoGainAttribute = 0;
                            //outputs, 
                            double entropyOfSet = Entropy(theta, expressionE);

                            //4. Select test attribute with highest information gain.

                            //Iterate through all attributes
                            for (int i = 0; i < attributes.Length; i++)
                            {
                                //Calculate information gain on each attribute
                                double attributeInfoGain = CalculateInformationGain(inputs, outputs, attributes[i], entropyOfSet, theta, expressionE);
                                //If info gain is greater than the current max, set new max info gain and associated attribute
                                if(i == 0)
                                {
                                    maxInfoGain = attributeInfoGain;
                                    maxInfoGainAttribute = attributes[i];
                                }
                                if (attributeInfoGain > maxInfoGain)
                                {
                                    maxInfoGain = attributeInfoGain;
                                    maxInfoGainAttribute = attributes[i];
                                }
                            }

                            //5. Label node with max info gain test attribute
                            Attribute = maxInfoGainAttribute;

                            List<int> newAttributeList = attributes.ToList();
                            newAttributeList.Remove(maxInfoGainAttribute);

                            ////Make new array of attributes without the max gain attribute
                            //for (int i = 0; i < attributes.Length; i++)
                            //{
                            //    if (i != maxInfoGainAttribute)
                            //    {
                            //        newAttributeList.Add(i);
                            //    }
                            //}

                            int[] newAttributes = newAttributeList.ToArray<int>();

                            //6. For each value of the attribute chosen (0, 1)
                            //a. Grow a branch from Node for condition Attribute = i (will be done last - need to determine whether to build leaf node or non-leaf node)

                            double elementsWithAttribute0 = 0;
                            double elementsWithAttribute1 = 0;
                            List<int> outputSubsetAttribute0 = new List<int>();
                            List<int> outputSubsetAttribute1 = new List<int>();
                            List<int[]> inputSubsetAttribute0 = new List<int[]>();
                            List<int[]> inputSubsetAttribute1 = new List<int[]>();

                            //Get elements where max info attribute is 0 or 1
                            for (int j = 0; j < inputs.Length; j++)
                            {
                                if (inputs[j][maxInfoGainAttribute] == 0)
                                {
                                    elementsWithAttribute0++;
                                    outputSubsetAttribute0.Add(outputs[j]);
                                    inputSubsetAttribute0.Add(inputs[j]);
                                }
                                else
                                {
                                    elementsWithAttribute1++;
                                    outputSubsetAttribute1.Add(outputs[j]);
                                    inputSubsetAttribute1.Add(inputs[j]);
                                }
                            }

                            int[][] inputSubsetAttribute0Array = inputSubsetAttribute0.ToArray<int[]>();
                            int[][] inputSubsetAttribute1Array = inputSubsetAttribute1.ToArray<int[]>();
                            int[] outputSubsetAttribute0Array = outputSubsetAttribute0.ToArray<int>();
                            int[] outputSubsetAttribute1Array = outputSubsetAttribute1.ToArray<int>();


                            //b. Let si be the set of samples in S where TA = ai.
                            //c. If si is empty, then attach a leaf labeled with majority class in S (create new node, it'll figure it out)
                            //d. Else attach the node returned by ID3(si, AL-TA) (create new node, it will be done programatically).

                            int majorityClassOfS = Accord.Statistics.Measures.Mode(outputs);

                            //Build out expression E defining node logic
                            Dictionary<int, int> expressionE0 = new Dictionary<int, int>();
                            Dictionary<int, int> expressionE1 = new Dictionary<int, int>();

                            foreach (KeyValuePair<int, int> pair in expressionE)
                            {
                                expressionE0.Add(pair.Key, pair.Value);
                                expressionE1.Add(pair.Key, pair.Value);
                            }

                            expressionE0.Add(maxInfoGainAttribute, 0);
                            expressionE1.Add(maxInfoGainAttribute, 1);

                            //Create node for attribute 0
                            Node attribute0Node = new Node(inputSubsetAttribute0Array, outputSubsetAttribute0Array, newAttributes, height++, majorityClassOfS, theta, expressionE0);
                            Node attribute1Node = new Node(inputSubsetAttribute1Array, outputSubsetAttribute1Array, newAttributes, height++, majorityClassOfS, theta, expressionE1);

                            //Connect nodes by an edge
                            Edges.Add(0, attribute0Node);
                            Edges.Add(1, attribute1Node);
                        }
                    }
                }
            }
        }

        public Node Root;
        public static int NumberOfItemsInOriginalDataset;
        public static int[][] fullInputs;
        public static int[] fullOutputs;


        public ModifiedTrie(int[][] inputs, int[] outputs, int totalSetSize, double theta)
        {
            List<int> attributes = new List<int>();

            for (int j = 0; j < inputs[0].Length; j++)
            {
                attributes.Add(j);
            }
            int[] attributesArray = attributes.ToArray<int>();
            int majorityClassOfS = Accord.Statistics.Measures.Mode(outputs);
            NumberOfItemsInOriginalDataset = outputs.Length;
            fullInputs = inputs;
            fullOutputs = outputs;
            Dictionary<int, int> expressionE = new Dictionary<int, int>();
            Root = new Node(inputs, outputs, attributesArray, 0, majorityClassOfS, theta, expressionE);
        }

        public int[] Decide(int[][] inputs)
        {
            int[] outputs = new int[inputs.Length];
            for(int i = 0; i < inputs.Length; i++)
            {
                int[] currentInput = inputs[i];
                int currentOutput = DecideSingleInput(currentInput);
                outputs[i] = currentOutput;
            }
            return outputs;
        }

        public int DecideSingleInput(int[] input)
        {
            int output = 0;
            Node currentNode = Root;
            for(int i = 0; i < input.Length + 1; i++)
            {
                if (currentNode.IsTerminal == true)
                {
                    output = currentNode.Class;
                    return output;
                }
                int currentAttribute = currentNode.Attribute;
                int inputValueAtAttribute = input[currentAttribute];
                try
                {
                    KeyValuePair<int, Node> edgeToTake = currentNode.Edges.Where(x => x.Key == inputValueAtAttribute).FirstOrDefault();
                    currentNode = edgeToTake.Value;
                }
                catch(Exception e)
                {
                    throw e;
                }
            }

            //This should never be reached - should always return an output from the loop.  -1 is used so it can be detected if this is reached for debugging.
            return -1;
        }

        //int[] outputs,
        public static double Entropy(double theta, Dictionary<int, int> expressionE)
        {
            double entropy = 0;
            int[] PStarEIndexes = new int[fullInputs.Length];
            int[] PStarEBarIndexes = new int[fullInputs.Length];

            for (int i = 0; i < fullInputs.Length; i++)
            {
                PStarEIndexes[i] = 0;
                PStarEBarIndexes[i] = 0;
            }
            double PStarE;
            double PStarEBar;

            //Build P*(E) based on E by building iterating through all key pairs of expressionE and whittling down from overall dataset
            for (int i = 0; i < fullInputs.Length; i++)
            {
                int currentInput = 1;
                int oppositeInput = 1;
                foreach (KeyValuePair<int, int> keyPair in expressionE)
                {
                //Get current attribute and its value from E
                    int currentAttribute = keyPair.Key;
                    int currentValue = keyPair.Value;
                    int oppositeValue = (keyPair.Value + 1) % 2;

                    //List<int[]> newInputs = new List<int[]>();
                    //List<int> newOutputs = new List<int>();
                    ////Iterate through all inputs - flag the indices where any key-value pair is false as 0
                    if (fullInputs[i][currentAttribute] != currentValue)
                    {
                        currentInput = 0;
                    }

                    if (fullInputs[i][currentAttribute] != oppositeValue)
                    {
                        oppositeInput = 0;
                    }
                }

                if (oppositeInput== 1)
                {
                    PStarEBarIndexes[i] = 1;
                }

                if (currentInput == 1)
                {
                    PStarEIndexes[i] = 1;
                }
            }

            double numberOfRows = 0;
            double numberOfEBarRows = 0;

            //Count number of rows where trueIndexes is still true (Count P*(E))
            for (int i = 0; i < PStarEIndexes.Length; i++)
            {
                if(PStarEIndexes[i] == 1)
                {
                    numberOfRows++;
                }
                if (PStarEBarIndexes[i] == 1)
                {
                    numberOfEBarRows++;
                }
            }

            //Get P*(E) as a ratio
            PStarE = numberOfRows / (double)NumberOfItemsInOriginalDataset;

            ////Build P*(E) based on E by building iterating through all key pairs of expressionE and whittling down from overall dataset
            //for (int i = 0; i < fullInputs.Length; i++)
            //{
            //    int currentInput = 1;
            //    foreach (KeyValuePair<int, int> keyPair in expressionE)
            //    {
            //        //Get current attribute and its value from E
            //        int currentAttribute = keyPair.Key;
            //        //Add 1 and mod by 2 - this will flip the value (0 + 1 % 2 = 1, 1 + 1 % 2 = 0)
            //        int currentValue = (keyPair.Value + 1) % 2;
            //        //List<int[]> newInputs = new List<int[]>();
            //        //List<int> newOutputs = new List<int>();
            //        ////Iterate through all inputs - flag the indices where any key-value pair is false as 0
            //        if (fullInputs[i][currentAttribute] != currentValue)
            //        {
            //            currentInput = 0;
            //        }
            //    }

            //    if (currentInput == 1)
            //    {
            //        PStarEBarIndexes[i] = 1;
            //    }
            //}


            //Count number of rows where trueIndexes is still true (Count P*(E))
            //for (int i = 0; i < PStarEBarIndexes.Length; i++)
            //{
            //    if (PStarEBarIndexes[i] == 1)
            //    {
            //        numberOfEBarRows++;
            //    }

            //}

            //Get P*(E) as a ratio
            PStarEBar = numberOfEBarRows / (double)NumberOfItemsInOriginalDataset;

            //Calculate P(E) and |S|
            double PE = GetPE(PStarE, PStarEBar, theta);
            double magnitudeOfS = PE * (double)NumberOfItemsInOriginalDataset;


            double PStarEQ0 = 0;

            double PStarEBarQ0 = 0;

            //Get P*(E) for Q0
            //for (int i = 0; i < fullOutputs.Length; i++)
            //{
            //    if(fullOutputs[i] == 0 && trueIndexes[i] == 1)
            //    {
            //        PStarEQ0++;
            //    }
            //}

            // int[] Q0trueIndexes = new int[trueIndexes.Length];

            double Q0numberOfRows = 0;
            double EBarQ0numberOfRows = 0;

            //Count number of rows where trueIndexes is still true (Count P*(E))
            for (int i = 0; i < PStarEIndexes.Length; i++)
            {
                if (PStarEIndexes[i] == 1 && fullOutputs[i] == 0)
                {
                    Q0numberOfRows++;
                }
                if (PStarEBarIndexes[i] == 1 && fullOutputs[i] == 0)
                {
                    EBarQ0numberOfRows++;
                }

            }

            //Get P*(E) for Q0 as a ratio
            PStarEQ0 = Q0numberOfRows / (double)NumberOfItemsInOriginalDataset;


            ////Count number of rows where trueIndexes is still true (Count P*(E))
            //for (int i = 0; i < PStarEBarIndexes.Length; i++)
            //{
            //    if (PStarEBarIndexes[i] == 1 && fullOutputs[i] == 0)
            //    {
            //        EBarQ0numberOfRows++;
            //    }

            //}

            //Get P*(E) for Q0 as a ratio
            PStarEBarQ0 = EBarQ0numberOfRows / (double)NumberOfItemsInOriginalDataset;

            //Get P(E) for Q0, then calculate Q0 and Q1.
            double PEQ0 = GetPE(PStarEQ0, PStarEBarQ0, theta);

            double Q0 = (PEQ0 * (double)NumberOfItemsInOriginalDataset) / magnitudeOfS;
            double Q1 = 1 - Q0;

            //Calculate entropy based on Q0 and Q1
            entropy -= Q0 * Math.Log(Q0, 2);
            entropy -= Q1 * Math.Log(Q1, 2);

            return entropy;

        }

        //Method to get PE given a PStarE and theta
        public static double GetPE(double PStarE, double PStarEBar, double theta)
        {
            //P*(E) = P(E) * theta + P(EBar) * (1 - theta)
            //P*(EBar) = P(EBar) * theta + P(E) * (1 - theta)
            if (PStarE == 1)
            {
                return 1;
            }
            //double PETopHalf = PStarE - (1 - theta);
            //double PEBottomHalf = (2 * theta) - 1;
            //double PE = PETopHalf / PEBottomHalf;

            var A = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {theta, (1-theta) },
                {(1-theta), theta }
            });
            var b = Vector<double>.Build.Dense(new double[] { PStarE, PStarEBar });
            var x = A.Solve(b);

            double PE = x.First();

            return PE;
        }

        //P*(E) is the number of elements in outputs or inputs (as the tree goes down the edges, outputs shrinks; the number of items in outputs/inputs is equivalent 
        //to P*(E) where E is the attributes as given so far).
        public static double CalculateInformationGain(int[][] inputs, int[] outputs, int attributeToCheck, double entropyOfSet, double theta, Dictionary<int, int> expressionE)
        {
            //Information gain for a given attribute is defined as 
            //Gain(S, A) = Entropy(S) - Sum v in a ((|Sv|/|S|)*Entropy(Sv)) where v is all possible values for A (in this case {0, 1}) and Sv is all elements is S where A has value v.

            double magnitudeOfS = getMagnitudeOfS(expressionE, theta);

            Dictionary<int, int> expressionE0 = new Dictionary<int, int>();
            Dictionary<int, int> expressionE1 = new Dictionary<int, int>();

            foreach (KeyValuePair<int, int> pair in expressionE)
            {
                expressionE0.Add(pair.Key, pair.Value);
                expressionE1.Add(pair.Key, pair.Value);
            }

            expressionE0.Add(attributeToCheck, 0);
            expressionE1.Add(attributeToCheck, 1);


            double magnitudeOfS0 = getMagnitudeOfS(expressionE0, theta);
            double magnitudeOfS1 = getMagnitudeOfS(expressionE1, theta);

            double entropyS0 = Entropy(theta, expressionE0);
            double entropyS1 = Entropy(theta, expressionE1);


            double summationPart0 = (magnitudeOfS0 / magnitudeOfS) * entropyS0;
            double summationPart1 = (magnitudeOfS1 / magnitudeOfS) * entropyS1;

            double summation = summationPart0 + summationPart1;

            double infoGain = entropyOfSet - summation;

            return infoGain;

        }

        public static double getMagnitudeOfS(Dictionary<int, int> expressionE, double theta)
        {
            int[] PStarEIndexes = new int[fullInputs.Length];
            int[] PStarEBarIndexes = new int[fullInputs.Length];

            for (int i = 0; i < fullInputs.Length; i++)
            {
                PStarEIndexes[i] = 0;
                PStarEBarIndexes[i] = 0;
            }
            double PStarE;
            double PStarEBar;

            //Build P*(E) based on E by building iterating through all key pairs of expressionE and whittling down from overall dataset
            for (int i = 0; i < fullInputs.Length; i++)
            {
                int currentInput = 1;
                foreach (KeyValuePair<int, int> keyPair in expressionE)
                {
                    //Get current attribute and its value from E
                    int currentAttribute = keyPair.Key;
                    int currentValue = keyPair.Value;
                    //List<int[]> newInputs = new List<int[]>();
                    //List<int> newOutputs = new List<int>();
                    ////Iterate through all inputs - flag the indices where any key-value pair is false as 0
                    if (fullInputs[i][currentAttribute] != currentValue)
                    {
                        currentInput = 0;
                    }
                }

                if (currentInput == 1)
                {
                    PStarEIndexes[i] = 1;
                }
            }

            double numberOfRows = 0;

            //Count number of rows where trueIndexes is still true (Count P*(E))
            for (int i = 0; i < PStarEIndexes.Length; i++)
            {
                if (PStarEIndexes[i] == 1)
                {
                    numberOfRows++;
                }

            }

            //Get P*(E) as a ratio
            PStarE = numberOfRows / (double)NumberOfItemsInOriginalDataset;

            //Build P*(E) based on E by building iterating through all key pairs of expressionE and whittling down from overall dataset
            for (int i = 0; i < fullInputs.Length; i++)
            {
                int currentInput = 1;
                foreach (KeyValuePair<int, int> keyPair in expressionE)
                {
                    //Get current attribute and its value from E
                    int currentAttribute = keyPair.Key;
                    //Add 1 and mod by 2 - this will flip the value (0 + 1 % 2 = 1, 1 + 1 % 2 = 0)
                    int currentValue = (keyPair.Value + 1) % 2;
                    //List<int[]> newInputs = new List<int[]>();
                    //List<int> newOutputs = new List<int>();
                    ////Iterate through all inputs - flag the indices where any key-value pair is false as 0
                    if (fullInputs[i][currentAttribute] != currentValue)
                    {
                        currentInput = 0;
                    }
                }

                if (currentInput == 1)
                {
                    PStarEBarIndexes[i] = 1;
                }
            }

            double numberOfEBarRows = 0;

            //Count number of rows where trueIndexes is still true (Count P*(E))
            for (int i = 0; i < PStarEBarIndexes.Length; i++)
            {
                if (PStarEBarIndexes[i] == 1)
                {
                    numberOfEBarRows++;
                }

            }

            //Get P*(E) as a ratio
            PStarEBar = numberOfEBarRows / (double)NumberOfItemsInOriginalDataset;

            //Calculate P(E) and |S|
            double PE = GetPE(PStarE, PStarEBar, theta);
            double magnitudeOfS = PE * (double)NumberOfItemsInOriginalDataset;

            return magnitudeOfS;
        }
    }
}
