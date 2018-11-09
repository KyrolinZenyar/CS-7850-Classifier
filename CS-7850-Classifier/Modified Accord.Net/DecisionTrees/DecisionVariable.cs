// Accord Machine Learning Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.MachineLearning.DecisionTrees
{
    using Accord.Statistics.Filters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Accord.Math;
    using System.Linq;
    using Accord.Compat;
    using Accord.Collections;

    /// <summary>
    ///   Attribute category.
    /// </summary>
    /// 
    public enum DecisionVariableKind
    {
        /// <summary>
        ///   Attribute is discrete-valued.
        /// </summary>
        /// 
        Discrete,

        /// <summary>
        ///   Attribute is continuous-valued.
        /// </summary>
        /// 
        Continuous
    }


    /// <summary>
    ///   Decision attribute.
    /// </summary>
    /// 
    [Serializable]
    public class ModifiedDecisionVariable
    {
        /// <summary>
        ///   Gets the name of the attribute.
        /// </summary>
        /// 
        public string Name { get; set; }

        /// <summary>
        ///   Gets the nature of the attribute (i.e. real-valued or discrete-valued).
        /// </summary>
        /// 
        public DecisionVariableKind Nature { get; set; }

        /// <summary>
        ///   Gets the valid range of the attribute.
        /// </summary>
        /// 
        public DoubleRange Range { get; set; }


        /// <summary>
        ///   Creates a new <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="range">The range of valid values for this attribute. Default is [0;1].</param>
        /// 
        public ModifiedDecisionVariable(string name, DoubleRange range)
        {
            this.Name = name;
            this.Nature = DecisionVariableKind.Continuous;
            this.Range = range;
        }

        /// <summary>
        ///   Creates a new <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="nature">The attribute's nature (i.e. real-valued or discrete-valued).</param>
        /// 
        public ModifiedDecisionVariable(string name, DecisionVariableKind nature)
        {
            this.Name = name;
            this.Nature = nature;
            this.Range = new DoubleRange(0, 1);
        }

        /// <summary>
        ///   Creates a new <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="range">The range of valid values for this attribute.</param>
        /// 
        public ModifiedDecisionVariable(string name, IntRange range)
        {
            this.Name = name;
            this.Nature = DecisionVariableKind.Discrete;
            this.Range = new DoubleRange(range.Min, range.Max);
        }

        /// <summary>
        ///   Creates a new discrete-valued <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="symbols">The number of possible values for this attribute.</param>
        /// 
        public ModifiedDecisionVariable(string name, int symbols)
            : this(name, new IntRange(0, symbols - 1))
        {
        }


        /// <summary>
        ///   Creates a new continuous <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// 
        public static ModifiedDecisionVariable Continuous(string name)
        {
            return new ModifiedDecisionVariable(name, DecisionVariableKind.Continuous);
        }

        /// <summary>
        ///   Creates a new continuous <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="range">The range of valid values for this attribute. Default is [0;1].</param>
        /// 
        public static ModifiedDecisionVariable Continuous(string name, DoubleRange range)
        {
            return new ModifiedDecisionVariable(name, range);
        }

        /// <summary>
        ///   Creates a new discrete <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="range">The range of valid values for this attribute.</param>
        /// 
        public static ModifiedDecisionVariable Discrete(string name, IntRange range)
        {
            return new ModifiedDecisionVariable(name, range);
        }

        /// <summary>
        ///   Creates a new discrete-valued <see cref="ModifiedDecisionVariable"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of the attribute.</param>
        /// <param name="symbols">The number of possible values for this attribute.</param>
        /// 
        public static ModifiedDecisionVariable Discrete(string name, int symbols)
        {
            return new ModifiedDecisionVariable(name, symbols);
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return String.Format("{0} : {1} ({2})", Name, Nature, Range);
        }

        /// <summary>
        ///   Creates a set of decision variables from a <see cref="OrderedDictionary{TKey, TValue}"/> codebook.
        /// </summary>
        /// 
        /// <param name="columns">The ordered dictionary containing information about the variables.</param>
        /// 
        /// <returns>An array of <see cref="ModifiedDecisionVariable"/> objects 
        ///   initialized with the values from the codebook.</returns>
        /// 
        public static ModifiedDecisionVariable[] FromDictionary(OrderedDictionary<string, string[]> columns)
        {
            if (columns.Count == 0)
                throw new ArgumentException("List of columns is empty.");

            var variables = new ModifiedDecisionVariable[columns.Count];

            for (int i = 0; i < variables.Length; i++)
            {
                string name = columns.GetKeyByIndex(i);
                variables[i] = new ModifiedDecisionVariable(name, columns[name].Length);
            }

            return variables;
        }

        /// <summary>
        ///   Creates a set of decision variables from a <see cref="Codification"/> codebook.
        /// </summary>
        /// 
        /// <param name="codebook">The codebook containing information about the variables.</param>
        /// <param name="columns">The columns to consider as decision variables.</param>
        /// 
        /// <returns>An array of <see cref="ModifiedDecisionVariable"/> objects 
        /// initialized with the values from the codebook.</returns>
        /// 
        public static ModifiedDecisionVariable[] FromCodebook(Codification<string> codebook, params string[] columns)
        {
            if (columns.Length == 0)
                throw new ArgumentException("List of columns is empty.");

            var variables = new ModifiedDecisionVariable[columns.Length];

            for (int i = 0; i < variables.Length; i++)
            {
                string name = columns[i];

                Codification.Options col;

                if (codebook.Columns.TryGetValue(name, out col))
                    variables[i] = new ModifiedDecisionVariable(name, col.NumberOfSymbols);
                else
                    variables[i] = new ModifiedDecisionVariable(name, DecisionVariableKind.Continuous);
            }

            return variables;
        }

        /// <summary>
        ///   Creates a set of decision variables from input data.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// 
        /// <returns>An array of <see cref="ModifiedDecisionVariable"/> objects 
        /// initialized with the values from the codebook.</returns>
        /// 
        public static ModifiedDecisionVariable[] FromData(double[][] inputs)
        {
            int cols = inputs.Columns();
            var variables = new ModifiedDecisionVariable[cols];
            for (int i = 0; i < variables.Length; i++)
                variables[i] = new ModifiedDecisionVariable(i.ToString(), inputs.GetColumn(i)
                    .Where(x => !Double.IsNaN(x)).ToArray().GetRange());
            return variables;
        }

        /// <summary>
        ///   Creates a set of decision variables from input data.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// 
        /// <returns>An array of <see cref="ModifiedDecisionVariable"/> objects 
        /// initialized with the values from the codebook.</returns>
        /// 
        public static ModifiedDecisionVariable[] FromData(int[][] inputs)
        {
            int cols = inputs.Columns();
            var variables = new ModifiedDecisionVariable[cols];
            for (int i = 0; i < variables.Length; i++)
                variables[i] = new ModifiedDecisionVariable(i.ToString(), inputs.GetColumn(i).GetRange());
            return variables;
        }

        /// <summary>
        ///   Creates a set of decision variables from input data.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// 
        /// <returns>An array of <see cref="ModifiedDecisionVariable"/> objects 
        /// initialized with the values from the codebook.</returns>
        /// 
        public static ModifiedDecisionVariable[] FromData(int?[][] inputs)
        {
            int cols = inputs.Columns();
            var variables = new ModifiedDecisionVariable[cols];
            for (int i = 0; i < variables.Length; i++)
                variables[i] = new ModifiedDecisionVariable(i.ToString(), inputs.GetColumn(i)
                    .Where(x => x.HasValue).Select(x => x.Value).ToArray().GetRange());
            return variables;
        }
    }


    /// <summary>
    ///   Collection of decision attributes.
    /// </summary>
    /// 
    [Serializable]
    public class DecisionVariableCollection : ReadOnlyCollection<ModifiedDecisionVariable>
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="DecisionVariableCollection"/> class.
        /// </summary>
        /// 
        /// <param name="list">The list to initialize the collection.</param>
        /// 
        public DecisionVariableCollection(IList<ModifiedDecisionVariable> list)
            : base(list) { }
    }
}
