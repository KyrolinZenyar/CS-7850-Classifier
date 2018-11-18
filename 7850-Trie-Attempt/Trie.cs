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

    public class Trie
    {
        public class Node
        {
            public int Class;
            public int Attribute;
            public bool IsTerminal { get { return Class != -1; } }
            //the int is the value of the attribute (0 or 1), the Node is the next node.
            public Dictionary<int, Node> Edges = new Dictionary<int, Node>();


            public Node(int[][] inputs, int[] outputs, int[] attributes, int height, int majorityClassOfParentNode)
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
                    }
                    else
                    {


                        //3. If attribute list is empty, return node as leaf labeled with majority class in S
                        if (attributes.Length == 0)
                        {
                            Class = Accord.Statistics.Measures.Mode(outputs);
                            Attribute = -1;
                        }
                        else
                        {
                            double maxInfoGain = 0;
                            int maxInfoGainAttribute = 0;
                            double entropyOfSet = Entropy(outputs);

                            //4. Select test attribute with highest information gain.

                            //Iterate through all attributes
                            for (int i = 0; i < attributes.Length; i++)
                            {
                                //Calculate information gain on each attribute
                                double attributeInfoGain = CalculateInformationGain(inputs, outputs, attributes[i], entropyOfSet);
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
                            Node attribute0Node = new Node(inputSubsetAttribute0Array, outputSubsetAttribute0Array, newAttributes, height++, majorityClassOfS);
                            Node attribute1Node = new Node(inputSubsetAttribute1Array, outputSubsetAttribute1Array, newAttributes, height++, majorityClassOfS);

                            //Connect nodes by an edge
                            Edges.Add(0, attribute0Node);
                            Edges.Add(1, attribute1Node);
                        }
                    }
                }
            }
        }

        public Node Root;

        public Trie(int[][] inputs, int[] outputs, int totalSetSize)
        {
            List<int> attributes = new List<int>();

            //number of attributes == length of any input[]
            for (int j = 0; j < inputs[0].Length; j++)
            {
                attributes.Add(j);
            }
            int[] attributesArray = attributes.ToArray<int>();
            int majorityClassOfS = Accord.Statistics.Measures.Mode(outputs);

            Root = new Node(inputs, outputs, attributesArray, 0, majorityClassOfS);
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

        public static double Entropy(int[] outputs)
        {
            int numberOfElements0 = 0;
            int numberOfElements1 = 0;
            double entropy = 0;

            //Count number of instances of 0 in dataset
            for (int i = 0; i < outputs.Length; i++)
            {
                if(outputs[i] == 0)
                {
                    numberOfElements0++;
                }
            }
            //All class labels that aren't 0 are 1
            numberOfElements1 = outputs.Length - numberOfElements0;

            //If elements in set are perfectly classified, entropy is 0
            if (numberOfElements0 == 0 || numberOfElements1 == 0)
            {
                entropy = 0;
                return entropy;
            }

            double Q0 = (double)numberOfElements0 / (double)outputs.Length;
            double Q1 = (double)numberOfElements1 / (double)outputs.Length;


            //Calculate entropy based on Q0 and Q1
            entropy -= Q0 * Math.Log(Q0, 2);
            entropy -= Q1 * Math.Log(Q1, 2);

            return entropy;
        }

        public static double CalculateInformationGain(int[][] inputs, int[] outputs, int attributeToCheck, double entropyOfSet)
        {
            //Magnitude of S = inputs.length
            //Magnitude of Sv = either elementsWith0 or elementsWith1, dependent on which is v

            List<int> outputSubsetAttribute0 = new List<int>();
            List<int> outputSubsetAttribute1 = new List<int>();
            int elementsWithAttribute0 = 0;
            int elementsWithAttribute1 = 0;
            double entropyOfAttributes0 = 0;
            double entropyOfAttributes1 = 0;
            
            //Count elements with attribute values of 0 and 1
            for (int j = 0; j < inputs.Length; j++)
            {
                
                if (inputs[j][attributeToCheck] == 0)
                {
                    elementsWithAttribute0++;
                    outputSubsetAttribute0.Add(outputs[j]);
                }
                else
                {
                    elementsWithAttribute1++;
                    outputSubsetAttribute1.Add(outputs[j]);
                }
            }

            //Make array of outputs for which attribute value is one or the other
            int[] outputSubsetAttribute0Array = outputSubsetAttribute0.ToArray<int>();
            int[] outputSubsetAttribute1Array = outputSubsetAttribute1.ToArray<int>();
            //Get Entropy(Sv) for v = 0, 1
            entropyOfAttributes0 = Entropy(outputSubsetAttribute0Array);
            entropyOfAttributes1 = Entropy(outputSubsetAttribute1Array);
            
            //Build out elements of summation for infoGain calculation
            double attribute0SumElement = ((double)elementsWithAttribute0 / (double)inputs.Length) * entropyOfAttributes0;
            double attribute1SumElement = ((double)elementsWithAttribute1 / (double)inputs.Length) * entropyOfAttributes1;

            //Get value of summation for infoGain
            double summation = attribute0SumElement + attribute1SumElement;

            //Get and return infoGain
            double infoGain = entropyOfSet - summation;
            return infoGain;
        }

    }
}
