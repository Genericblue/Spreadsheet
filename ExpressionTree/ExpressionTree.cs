//Mark Eads 11250124
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTree
{
    public class ExpTree
    {
        //root node
        private BaseNode m_root;
        private int nodeCount;
        public int NodeCount
        {
            get { return nodeCount; }
        }
        //expression the user inputs
        private string m_expression;
        public string Expression
        {
            get { return m_expression; }
            set
            {
                m_expression = value;
                m_variables.Clear();
                nodeCount = 0;
                int c = 0;
                m_root = Build(ref c);
  
            }
        }

        public char[] operators = { '+', '-', '*', '/' };
        //dictionary for the variable names and values, default to 0
        private Dictionary<string, VariableNode> m_variables = new Dictionary<string, VariableNode>();
        public List<string> getVariableNames()
        {
            List<string> returnList = new List<string>();
            foreach (KeyValuePair<string, VariableNode> item in m_variables)
            {
                returnList.Add(item.Key);
            }
            return returnList;
        }
        //constustor, defualts to "0+0"
        public ExpTree(string expression)
        {
            m_expression = expression;
            nodeCount = 0;
            int c = 0;
            m_root = Build(ref c);
            //call build to build the expression tree around the expression
        }

        //sets the key in the dictionary to the value
        public void SetVar(string varName, double varValue)
        {
            m_variables[varName].Number = varValue;

        }

        //calls the private eval that is passed in the root node
        public double Eval()
        {
            return EvalReal(m_root);
        }

        //evaluates the expression tree and returns its value using recursion
        private double EvalReal(BaseNode temp)
        {
            return m_root.EvalThis();
        }

        //build the expression tree
        private BaseNode Build(ref int c)
        {
            BaseNode returnNode = null;
            string tempString = "";
            BaseNode tempNode = null;
            BinaryNode tempBinary = null;
            BinaryNode lastOp = null;
            BinaryNode tempReturn = null;

            for (; c < m_expression.Length && m_expression[c] != ')'; nodeCount++)
            {
                if (Char.IsLetter(m_expression[c]))// if it is a letter continue until you reach an operation or the end of the expression
                {////////////////////////
                    do
                    {
                        tempString += m_expression[c];
                        c++;
                    } while (c < m_expression.Length &&
                          !operators.Contains(m_expression[c]) &&
                            m_expression[c] != ')');

                    if (!m_variables.ContainsKey(tempString))
                    {
                        tempNode = new VariableNode(tempString, 0);
                        m_variables.Add(tempString, (VariableNode)tempNode);//add it to the dictionary
                    }
                    tempString = "";

                    if (returnNode == null)
                        returnNode = tempNode;
                    else
                        lastOp.setAsChild(tempNode);
                }////////////////////////
                else if (Char.IsDigit(m_expression[c]))// if it is a number continue until the end of the number
                {///////////////////////
                    do
                    {
                        tempString += m_expression[c];
                        c++;
                    } while (c < m_expression.Length &&
                          !operators.Contains(m_expression[c]) &&
                            m_expression[c] != ')');

                    tempNode = new ConstantNode(Convert.ToDouble(tempString));
                    tempString = "";

                    if (returnNode == null)
                        returnNode = tempNode;
                    else
                        lastOp.setAsChild(tempNode);

                }//////////////////////
                else if (m_expression[c] == '(')//if it is a paranthesis recusivley call build on its contents
                {/////////////////////
                    c++;
                    tempNode = Build(ref c);
                    if (returnNode == null)
                        returnNode = tempNode;
                    else
                        lastOp.setAsChild(tempNode);
                    c++;
                }////////////////////
                else if (operators.Contains(m_expression[c])) // if it is an operator position it in the tree correctly
                {/////////////////////
                    tempBinary = new BinaryNode(m_expression[c]);

                    if (lastOp == null)//if there was no previous operator
                    {
                        returnNode = tempBinary;
                        tempBinary.setAsChild(tempNode);
                    }
                    else
                    {
                        tempReturn = (BinaryNode)returnNode;
                        if (PriorityCheck(tempBinary.Operator, tempReturn.Operator) <= 0) // if the current operator is less than or equal to the root operator in priority
                        {
                            tempBinary.setAsChild(returnNode);
                            returnNode = tempBinary;
                        }
                        else if (PriorityCheck(tempBinary.Operator, lastOp.Operator) <= 0)//if it is <= to the last operator in priority
                        {
                            tempReturn.Right = tempBinary;
                            tempBinary.setAsChild(lastOp);
                        }
                        else if (PriorityCheck(tempBinary.Operator, lastOp.Operator) == 1)//if it is > than the last operator in priority
                        {
                            if (lastOp.Right != null)
                            {
                                tempBinary.Left = lastOp.Right;
                            }
                            lastOp.setAsChild(tempBinary);
                        }
                    }

                    lastOp = tempBinary;//make this the last operator
                    c++;
                }/////////////////////
            }

            return returnNode;
        }

        private int PriorityCheck(char a, char b)
        {
            char[] Low = { '+', '-' };
            char[] High = { '*', '/' };
            if (Low.Contains(a) && Low.Contains(b))
            {
                return 0;
            }
            else if (Low.Contains(a) && High.Contains(b))
            {
                return -1;
            }
            else if (High.Contains(a) && Low.Contains(b))
            {
                return 1;
            }
            else if (High.Contains(a) && High.Contains(b))
            {
                return 0;
            }
            return 0;
        }

        //constant node that has a value
        private class ConstantNode : BaseNode
        {
            private double number;
            public double Number
            {
                get { return number; }
                set { number = value; }
            }

            public ConstantNode(double newNumber)
            {
                number = newNumber;
            }

            public override double EvalThis()
            {
                return number;
            }
        }

        //variable node containing a name
        private class VariableNode : BaseNode
        {
            private string name;
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            private double number;
            public double Number
            {
                get { return number; }
                set { number = value; }
            }

            public VariableNode(string newName, double newNumber)
            {
                name = newName;
                number = newNumber;
            }

            public override double EvalThis()
            {
                return number;
            }
        }

        //binary node containing a char of its operator
        private class BinaryNode : BaseNode
        {
            private BaseNode left;
            public BaseNode Left
            {
                get { return left; }
                set { left = value; }
            }
            private BaseNode right;
            public BaseNode Right
            {
                get { return right; }
                set { right = value; }
            }

            private char op;
            public char Operator
            {
                get { return op; }
                set { op = value; }
            }

            public BinaryNode(char oper)
            {
                op = oper;
                left = null;
                right = null;
            }

            public override double EvalThis()
            {
                if (op == '+')
                    return left.EvalThis() + right.EvalThis();
                else if (op == '-')
                    return left.EvalThis() - right.EvalThis();
                else if (op == '*')
                    return left.EvalThis() * right.EvalThis();
                else
                    return left.EvalThis() / right.EvalThis();
            }

            public void setAsChild(BaseNode node)
            {
                if (left == null)
                    left = node;
                else
                    right = node;
            }
        }

        //the abstract base class for all the other nodes, it contains the node type
        public abstract class BaseNode
        {
            virtual public double EvalThis()
            {
                return 0;
            }
        }
    }
}
