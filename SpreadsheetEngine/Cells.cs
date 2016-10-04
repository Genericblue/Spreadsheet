//Mark Eads 11250124
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ExpressionTree;
using System.Xml;

namespace SpreadsheetEngine
{
    //the abstract base class, inherits to utilize events
    public abstract class AbstractCell : INotifyPropertyChanged
    {
        //RowIndex and its property
        private readonly int rowIndex;
        public int RowIndex { get { return rowIndex; } }

        //columnIndex and its property
        private readonly int columnIndex;
        public int ColumnIndex { get { return columnIndex; } }

        private int bgc;
        public int BGC
        {
            get { return bgc; }
            set
            {
                bgc = value;
                PropertyChanged(this, new PropertyChangedEventArgs("bgc"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected string text;//text and its properties
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("textundo"));
                    text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("text"));//notify subscribers that text has changed
                }
            }
        }
        
        //evaluated is text after it has been evaluated
        protected string evaluated;
        public virtual string Evaluated
        {
            get { return evaluated; }
            set { }
        }

        //constructor
        public AbstractCell(int newColumnIndex, int newRowIndex)
        {
            text = "";
            bgc = -1;
            evaluated = "";
            columnIndex = newColumnIndex;
            rowIndex = newRowIndex;
            subs = new List<AbstractCell>();
        }

        //sets all the values in the cell to the defualt
        public void Clear()
        {
            unsubscribeAll();
            subs.Clear();
            Text = "";
            BGC = -1;
        }

        private List<AbstractCell> subs;//all the cells this cell is subscribed to
        //which are the ones it is referencing

        public List<AbstractCell> Subs
        {
            get { return subs; }
        }

        public void unsubscribeAll()
        {
            foreach (AbstractCell item in subs)
            {
                item.PropertyChanged -= subChanged;
            }
            subs.Clear();//clear the subs
        }

        public void unsubscribeTo(AbstractCell tounSub)
        {
            tounSub.PropertyChanged -= subChanged;
            subs.Remove(tounSub);
        }

        public void subscribeTo(AbstractCell toSub)
        {
            toSub.PropertyChanged += subChanged;//sub to cell property passed in
            subs.Add(toSub);
        }

        public void subChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "textundo")
            {
                PropertyChanged(this, new PropertyChangedEventArgs("text"));//if one of the cells this is subbed to changes say that this one has changed to and let the program do the work
            }
        }
    }

    //class to put on the undo and redo stack
    public class command
    {
        private int bgc;
        public int BGC { get { return bgc; } }
        private string text;
        public string Text { get { return text; } }
        private string type;
        public string Type { get { return type; } }
        private AbstractCell cell;
        public AbstractCell Cell { get { return cell; } }

        public command(AbstractCell newCell, int newbgc)//background color constructor
        {
            cell = newCell;
            bgc = newbgc;
            type = "Background Color";
        }
        public command(AbstractCell newCell, string newtext)//text constructor
        {
            cell = newCell;
            text = newtext;
            type = "Text";
        }
    }

    public class Spreadsheet
    {
        //saves the spreadsheet to an xml file
        //iterates through all the cells in the spreadsheet
        //continues if those cells are changed in some way
        //writes the proper node names in the xml file as well as the cell values
        public void save(string filename)
        {
            XmlTextWriter xml = new XmlTextWriter(filename,Encoding.UTF8);
            xml.Formatting = Formatting.Indented;

            xml.WriteStartElement("spreadsheet");
            foreach (Cell cell in cells)
            {
                if (cell.Text != "" || cell.BGC != -1)
                {
                    xml.WriteStartElement("cell");
                    xml.WriteStartElement("column");
                    xml.WriteString(cell.ColumnIndex.ToString());
                    xml.WriteEndElement();
                    xml.WriteStartElement("row");
                    xml.WriteString(cell.RowIndex.ToString());
                    xml.WriteEndElement();
                    xml.WriteStartElement("text");
                    xml.WriteString(cell.Text);
                    xml.WriteEndElement();
                    xml.WriteStartElement("bgc");
                    xml.WriteString(cell.BGC.ToString());
                    xml.WriteEndElement();
                    xml.WriteEndElement();
                }
            }
            xml.WriteEndElement();
            xml.Close();
        }

        //loads a spread sheet from an xml file using xml document
        //First loads the filename passed into it into the xml document
        //then clears all the current cells
        //then iterates through the xml file and sets the cells to their correspoding values in the xml file
        //then clears the undo and redo stacks
        public void load(string filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);

            foreach (Cell cell in cells)
            {
                cell.Clear();
            }

            foreach (XmlNode node in xml.SelectNodes("spreadsheet/cell"))
            {
                AbstractCell cell = GetCell(Convert.ToInt32(node.SelectSingleNode("column").InnerText), Convert.ToInt32(node.SelectSingleNode("row").InnerText));
                cell.Text = node.SelectSingleNode("text").InnerText;
                cell.BGC = Convert.ToInt32(node.SelectSingleNode("bgc").InnerText);
            }

            undos.Clear();
            redos.Clear();
        }

        public event PropertyChangedEventHandler CellPropertyChanged;

        Cell[,] cells;//2D array of cells

        private bool UnorRedo;//to tell if we are in an undo or redo call

        private Stack<List<command>> undos = new Stack<List<command>>();

        public void UndoPush(List<command> cmd)
        {
            undos.Push(cmd);
        }

        public int UndoCount()
        {
            return undos.Count;
        }

        public List<command> UndoPeek()
        {
            return undos.Peek();
        }

        private Stack<List<command>> redos = new Stack<List<command>>();

        public int RedoCount()
        {
            return redos.Count;
        }

        public List<command> RedoPeek()
        {
            return redos.Peek();
        }

        //the undo and redo function
        public void UnandRedo(bool isUndo)
        {
            UnorRedo = true;
            List<command> List1 = new List<command>();
            List<command> List2 = new List<command>();
            if(isUndo)
                List1 = undos.Pop();//pop off the undo stack if it is an undo call
            else
                List1 = redos.Pop();
            

            if (List1[0].Type == "Text")//if it is a text undo or redo
            {
                string tempText = List1[0].Cell.Text;
                List1[0].Cell.Text = List1[0].Text;
                List2.Add(new command(List1[0].Cell, tempText));
            }
            else if (List1[0].Type == "Background Color")//if it is a background color undo or redo
            {
                int tempBGC = 0;
                for (int i = 0; i < List1.Count; i++)//go through the list since background color can be a lot of cells
                {
                    tempBGC = List1[i].Cell.BGC;
                    List1[i].Cell.BGC = List1[i].BGC;
                    List2.Add(new command(List1[i].Cell, tempBGC));
                }
            }

            if (isUndo)
                redos.Push(List2);//add something to the redo stack if it is an undo call
            else
                undos.Push(List2);
            UnorRedo = false;
        }

        //Columncount and its property
        private int columnCount;
        public int ColumnCount { get { return columnCount; } }

        //rowcount and its property
        private int rowCount;
        public int RowCount { get { return rowCount; } }

        ExpTree exp = new ExpTree("0");

        //constructor
        public Spreadsheet(int columns, int rows)
        {
            //set columncount and rowcount
            columnCount = columns;
            rowCount = rows;
            UnorRedo = false;
            //initialize cells array
            cells = new Cell[columns, rows];
            for (; columns > 0; columns--)
            {
                for (rows = rowCount; rows > 0; rows--)
                {
                    cells[columns-1, rows-1] = new Cell(columns-1, rows-1);
                    //subscribe to propertychange
                    cells[columns-1, rows-1].PropertyChanged += CellChange;
                }
            }
        }

        public AbstractCell GetCell(int column, int row)
        {
            return cells[column, row];
        }
        public AbstractCell GetCell(string cellname)
        {
            return GetCell((int)(cellname[0] - 65), (Convert.ToInt32(cellname.Substring(1))) - 1);
        }
        public void CellChange(object sender, PropertyChangedEventArgs e)
        {
            //if a cell changes
            Cell eventCell = (Cell)sender;
            if (e.PropertyName == "text")//if the property that changed was text
            {
                if (eventCell.Text == "" || eventCell.Text[0] != '=')//if the first character is not =
                {
                    eventCell.Evaluated = eventCell.Text;
                }
                else
                {
                    exp.Expression = eventCell.Text.Substring(1);

                    List<string> variables = exp.getVariableNames();//get a list of all the variables in the expression
                    bool noEval = false;

                    eventCell.unsubscribeAll();

                    if (variables != null)//for each variable get its value and set the variable in the expression tree
                    {
                        AbstractCell cellRef = null;
                        double eval = 0;
                        int temp = 0;
                        foreach (string item in variables)
                        {
                            if (!Int32.TryParse(item.Substring(1), out temp))////////////////////ERROR CHECKING
                            {//check if the user entered something that isnt a cell as a variable
                                noEval = true;
                                eventCell.Evaluated = "ERROR: Bad Reference";
                                break;
                            }
                            if ((Convert.ToInt32(item.Substring(1))) > 50 || (Convert.ToInt32(item.Substring(1))) <= 0)
                            {//checks if the user entered a cell name that is out of bounds
                                noEval = true;
                                eventCell.Evaluated = "ERROR: Reference Out of Bounds";
                                break;
                            }
                            cellRef = GetCell((int)(Char.ToUpper(item[0]) -'A'), (Convert.ToInt32(item.Substring(1))) - 1);
                            if (cellRef == eventCell)
                            {//check to see if the cell the user entered is self referencing
                                noEval = true;
                                eventCell.Evaluated = "ERROR: Self Reference";
                                break;
                            }////////////////////////////////////////////////////////////////////
                            //SUBS
                            eventCell.subscribeTo(cellRef);//have the eventcell subscribe to its property changed event
                            //
                            if (Double.TryParse(cellRef.Evaluated, out eval))
                                exp.SetVar(item, eval);
                            else if (exp.NodeCount == 1)
                            {
                                if (circularReference(eventCell, new List<AbstractCell>()))//checks to see if the user entered a circular reference if so break the loop by unsubbing
                                {
                                    eventCell.Evaluated = "ERROR: Circular Reference";
                                    eventCell.unsubscribeAll();
                                }
                                else
                                {
                                    eventCell.Evaluated = cellRef.Evaluated;
                                    if (eventCell.Evaluated == "")
                                        eventCell.Evaluated = "0";
                                }

                                noEval = true;
                            }
                            else
                                exp.SetVar(item, 0);
                        }
                    }
                    if (!noEval)
                    {
                        if (circularReference(eventCell, new List<AbstractCell>()))//checks to see if the user entered a circular reference if so break the loop by unsubbing
                        {
                            eventCell.Evaluated = "ERROR: Circular Reference";
                            eventCell.unsubscribeAll();
                        }
                        else
                            eventCell.Evaluated = exp.Eval().ToString();
                    }
                }
                CellPropertyChanged(sender, e);//notify subscribers of cellpropertychanged
                if (!UnorRedo)
                    redos.Clear();
            }

            if(e.PropertyName == "bgc")
            {
                CellPropertyChanged(sender, e);
                if(!UnorRedo)
                    redos.Clear();
            }

            if (e.PropertyName == "textundo")
            {
                if (!UnorRedo)// if we did not come from an undo or redo call
                {
                    List<command> temp = new List<command>();
                    temp.Add(new command(eventCell, eventCell.Text));
                    undos.Push(temp);
                }
            }
        }

        //gets passed in a cell and originally an empty list of cells
        //checks to see if the cell is in the list, if it is then there is a circular reference
        //if it isnt it adds that cell to the list
        //then it recursively goes through all the cells it is referencing, passing in that cell and the current list
        private bool circularReference(AbstractCell cell, List<AbstractCell> used)
        {
            bool success = false;
            if (used.Contains(cell))
                return true;

            used.Add(cell);
            foreach (AbstractCell cell2 in cell.Subs)
            {
               success = circularReference(cell2, new List<AbstractCell>(used));
               if (success == true)
               {
                   break;
               }
            }
            return success;
        }
        
        //cell class inheriting from the abstract base class
        private class Cell : AbstractCell
        {
            public Cell(int newColumnIndex, int newRowIndex) : base(newColumnIndex, newRowIndex){}

            public override string Evaluated
            {
                get { return evaluated; }
                set 
                { 
                    evaluated = value;
                }
            }
        }
    }
}
