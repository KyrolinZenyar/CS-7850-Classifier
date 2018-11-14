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

            public Node(int[][] inputs, int[] outputs, int[] attributes, int height, int majorityClassOfParentNode, double theta)
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
                            double entropyOfSet = Entropy(outputs, theta);

                            //4. Select test attribute with highest information gain.

                            //Iterate through all attributes
                            for (int i = 0; i < attributes.Length; i++)
                            {
                                //Calculate information gain on each attribute
                                double attributeInfoGain = CalculateInformationGain(inputs, outputs, attributes[i], entropyOfSet, theta);
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

                            //Create node for attribute 0
                            Node attribute0Node = new Node(inputSubsetAttribute0Array, outputSubsetAttribute0Array, newAttributes, height++, majorityClassOfS, theta);
                            Node attribute1Node = new Node(inputSubsetAttribute1Array, outputSubsetAttribute1Array, newAttributes, height++, majorityClassOfS, theta);

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
            Root = new Node(inputs, outputs, attributesArray, 0, majorityClassOfS, theta);
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

        public static double Entropy(int[] outputs, double theta)
        {
            double bottomHalf = (2 * theta) - 1;
            double magnitudeOfS, PStarE, topHalfOfPE, PE;
            PStarE = outputs.Length / (double)NumberOfItemsInOriginalDataset;
            topHalfOfPE = PStarE - (1 - theta);
            PE = topHalfOfPE / bottomHalf;
            //Magnitude of S = P(E) * n where n is the total number of items in the dataset and P(E) is calculated based on P*(E) and theta.
            if (theta > 0.5)
            {
                if (PStarE >= theta)
                {
                    PE = 1;
                }
                if (PStarE >= (1 - theta))
                {
                    PE = 0;
                }
            }
            else if (theta < 0.5)
            {
                if (PStarE <= (1 - theta))
                {
                    PE = 0;
                }
                if (PStarE <= theta)
                {
                    PE = 1;
                }
            }
            magnitudeOfS = PE * (double)NumberOfItemsInOriginalDataset;

            //If full set, then no need to break it down
            if(outputs.Length == (double)NumberOfItemsInOriginalDataset)
            {
                magnitudeOfS = (double)NumberOfItemsInOriginalDataset;
            }

            int numberOfElementsWithAttribute0 = 0;
            int numberOfElementsWithAttribute1 = 0;
            double entropy = 0;

            //Count number of instances of 0 in dataset
            for (int i = 0; i < outputs.Length; i++)
            {
                if(outputs[i] == 0)
                {
                    numberOfElementsWithAttribute0++;
                }
            }
            //All class labels that aren't 0 are 1
            numberOfElementsWithAttribute1 = outputs.Length - numberOfElementsWithAttribute0;

            double PStarE0 = numberOfElementsWithAttribute0 / (double)NumberOfItemsInOriginalDataset;
            double PStarE1 = numberOfElementsWithAttribute1 / (double)NumberOfItemsInOriginalDataset;

            double topHalfOfPE0 = PStarE0 - (1 - theta);
            double topHalfOfPE1 = PStarE1 - (1 - theta);

            double PE0 = topHalfOfPE0 / bottomHalf;
            double PE1 = topHalfOfPE1 / bottomHalf;

            //Limit P*E(0)
            if (theta > 0.5)
            {
                if (PStarE0 >= theta)
                {
                    PE0 = 1;
                }
                if (PStarE0 >= (1 - theta))
                {
                    PE0 = 0;
                }
            }
            else if (theta < 0.5)
            {
                if (PStarE0 <= (1 - theta))
                {
                    PE0 = 0;
                }
                if (PStarE0 <= theta)
                {
                    PE0 = 1;
                }
            }

            //Limit P*E(1)
            if (theta > 0.5)
            {
                if (PStarE1 >= theta)
                {
                    PE1 = 1;
                }
                if (PStarE1 >= (1 - theta))
                {
                    PE1 = 0;
                }
            }
            else if (theta < 0.5)
            {
                if (PStarE1 <= (1 - theta))
                {
                    PE1 = 0;
                }
                if (PStarE1 <= theta)
                {
                    PE1 = 1;
                }
            }

            //If elements in set are perfectly classified, entropy is 0
            if (PStarE0 == 0 || PStarE1 == 0)
            {
                entropy = 0;
                return entropy;
            }

            double Q0 = ((double)PStarE0 * (double)NumberOfItemsInOriginalDataset) / (double)magnitudeOfS;
            double Q1 = ((double)PStarE1 * (double)NumberOfItemsInOriginalDataset) / (double)magnitudeOfS;


            //Calculate entropy based on Q0 and Q1
            entropy -= Q0 * Math.Log(Q0, 2);
            entropy -= Q1 * Math.Log(Q1, 2);

            return entropy;
        }

        //P*(E) is the number of elements in outputs or inputs (as the tree goes down the edges, outputs shrinks; the number of items in outputs/inputs is equivalent 
        //to P*(E) where E is the attributes as given so far).
        public static double CalculateInformationGain(int[][] inputs, int[] outputs, int attributeToCheck, double entropyOfSet, double theta)
        {
            double bottomHalf = (2 * theta) - 1;
            double magnitudeOfS, PStarE, topHalfOfPE, PE;
            PStarE = outputs.Length / (double)NumberOfItemsInOriginalDataset;
            topHalfOfPE = PStarE - (1 - theta);
            PE = topHalfOfPE / bottomHalf;
            //Magnitude of S = P(E) * n where n is the total number of items in the dataset and P(E) is calculated based on P*(E) and theta.
            if (theta > 0.5)
            {
                if(PStarE >= theta)
                {
                    PE = 1;
                }
                if(PStarE >= (1 - theta))
                {
                    PE = 0;
                }
            }
            else if(theta < 0.5)
            {
                if (PStarE <= (1 - theta))
                {
                    PE = 0;
                }
                if (PStarE <= theta)
                {
                    PE = 1;
                }
            }
            magnitudeOfS = PE * (double)NumberOfItemsInOriginalDataset;

            //If full set, then no need to break it down
            if (outputs.Length == (double)NumberOfItemsInOriginalDataset)
            {
                magnitudeOfS = (double)NumberOfItemsInOriginalDataset;
            }

            //Magnitude of Sv = P(E)*n where E also divides based on the test attirbute (either elementsWith0 or elementsWith1, dependent on which is v)

            List<int> outputSubsetAttribute0 = new List<int>();
            List<int> outputSubsetAttribute1 = new List<int>();
            int numberOfElementsWithAttribute0 = 0;
            int numberOfElementsWithAttribute1 = 0;
            double entropyOfAttributes0 = 0;
            double entropyOfAttributes1 = 0;
            
            //Count elements with attribute values of 0 and 1
            for (int j = 0; j < inputs.Length; j++)
            {
                
                if (inputs[j][attributeToCheck] == 0)
                {
                    numberOfElementsWithAttribute0++;
                    outputSubsetAttribute0.Add(outputs[j]);
                }
                else
                {
                    numberOfElementsWithAttribute1++;
                    outputSubsetAttribute1.Add(outputs[j]);
                }
            }

            //Make array of outputs for which attribute value is one or the other
            int[] outputSubsetAttribute0Array = outputSubsetAttribute0.ToArray<int>();
            int[] outputSubsetAttribute1Array = outputSubsetAttribute1.ToArray<int>();

            double PStarE0 = numberOfElementsWithAttribute0 / (double)NumberOfItemsInOriginalDataset;
            double PStarE1 = numberOfElementsWithAttribute1 / (double)NumberOfItemsInOriginalDataset;

            double topHalfOfPE0 = PStarE0 - (1 - theta);
            double topHalfOfPE1 = PStarE1 - (1 - theta);

            double PE0 = topHalfOfPE0 / bottomHalf;
            double PE1 = topHalfOfPE1 / bottomHalf;

            //Limit P*E(0)
            if (theta > 0.5)
            {
                if (PStarE0 >= theta)
                {
                    PE0 = 1;
                }
                if (PStarE0 >= (1 - theta))
                {
                    PE0 = 0;
                }
            }
            else if (theta < 0.5)
            {
                if (PStarE0 <= (1 - theta))
                {
                    PE0 = 0;
                }
                if (PStarE0 <= theta)
                {
                    PE0 = 1;
                }
            }

            //Limit P*E(1)
            if (theta > 0.5)
            {
                if (PStarE1 >= theta)
                {
                    PE1 = 1;
                }
                if (PStarE1 >= (1 - theta))
                {
                    PE1 = 0;
                }
            }
            else if (theta < 0.5)
            {
                if (PStarE1 <= (1 - theta))
                {
                    PE1 = 0;
                }
                if (PStarE1 <= theta)
                {
                    PE1 = 1;
                }
            }

            double magnitudeOfS0 = PE0 * (double)NumberOfItemsInOriginalDataset;
            double magnitudeOfS1 = PE1 * (double)NumberOfItemsInOriginalDataset;


            //Get Entropy(Sv) for v = 0, 1
            entropyOfAttributes0 = Entropy(outputSubsetAttribute0Array, theta);
            entropyOfAttributes1 = Entropy(outputSubsetAttribute1Array, theta);
            
            //Build out elements of summation for infoGain calculation
            double attribute0SumElement = ((double)magnitudeOfS0 / (double)magnitudeOfS) * entropyOfAttributes0;
            double attribute1SumElement = ((double)magnitudeOfS1 / (double)magnitudeOfS) * entropyOfAttributes1;

            //Get value of summation for infoGain
            double summation = attribute0SumElement + attribute1SumElement;

            //Get and return infoGain
            double infoGain = entropyOfSet - summation;
            return infoGain;
        }
    }
}
