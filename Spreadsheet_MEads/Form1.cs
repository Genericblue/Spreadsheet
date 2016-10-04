//Mark Eads 12150124
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using ExpressionTree;

namespace Spreadsheet_MEads
{
    public partial class Form1 : Form
    {
        Spreadsheet spreadsheet;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //initialize the columns
            DataGridViewColumn column;
            for(char c = 'A'; c <= 'Z'; c++)
            {
                column = new DataGridViewTextBoxColumn();
                column.HeaderText = c.ToString();
                dataGridView1.Columns.Add(column);
            }

            //initialize the rows
            DataGridViewRow row;
            for (int i = 1; i <= 50; i++)
            {
                row = new DataGridViewRow();
                row.HeaderCell.Value = i.ToString();
                dataGridView1.Rows.Add(row);
            }

            //initialize the spreadsheet
            spreadsheet = new Spreadsheet(26, 50);
            //subscribe to CellPropertyChanged
            spreadsheet.CellPropertyChanged += CellChanged;
        }

        public void CellChanged(object sender, PropertyChangedEventArgs e)
        {
            AbstractCell eventCell = (AbstractCell)sender; //set sender casted as a Cell to a new Cell

            if (e.PropertyName == "text")
            {
                //if the text in cell was changed, change the corresponding cell in the datagrid
                dataGridView1.Rows[eventCell.RowIndex].Cells[eventCell.ColumnIndex].Value = eventCell.Evaluated;
            }
            if (e.PropertyName == "bgc")
            {
                dataGridView1.Rows[eventCell.RowIndex].Cells[eventCell.ColumnIndex].Style.BackColor = Color.FromArgb(eventCell.BGC);
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView temp = (DataGridView) sender;
            temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = spreadsheet.GetCell(e.ColumnIndex, e.RowIndex).Text;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
            DataGridView temp = (DataGridView)sender;

            if (temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                spreadsheet.GetCell(e.ColumnIndex, e.RowIndex).Text = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
            else
            {
                spreadsheet.GetCell(e.ColumnIndex, e.RowIndex).Text = "";
            }
            temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = spreadsheet.GetCell(e.ColumnIndex, e.RowIndex).Evaluated; 
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<command> bgcCmds = new List<command>();
            AbstractCell temp = null;
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)//for each of the highlighted cells
                {
                    temp = spreadsheet.GetCell(cell.ColumnIndex, cell.RowIndex);//get the spreadsheet cell of it
                    bgcCmds.Add(new command(temp, temp.BGC));//add it to a command list
                    temp.BGC = cd.Color.ToArgb();//set its color
                }
                spreadsheet.UndoPush(bgcCmds);//push the command list onto the undo stack
            }
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            //set each button to disabled
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Text = "Redo";
            undoToolStripMenuItem.Text = "Undo";

            //if there are redos, enable the button
            if (spreadsheet.RedoCount() != 0)
            {
                redoToolStripMenuItem.Text = "Redo " + spreadsheet.RedoPeek()[0].Type;
                redoToolStripMenuItem.Enabled = true;
            }
            //if there are undos enable the button
            if (spreadsheet.UndoCount() != 0)
            {
                undoToolStripMenuItem.Text = "Undo " + spreadsheet.UndoPeek()[0].Type;
                undoToolStripMenuItem.Enabled = true;
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //call the spreadsheet undo and redo function
            spreadsheet.UnandRedo(true);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //call the spreadsheet undo and redo function
            spreadsheet.UnandRedo(false);
        }

        private void toolStripDropDownButton2_Click(object sender, EventArgs e)
        {

        }

        //procs if the user the clicks on save
        //opens a save file dialog
        //passes the filename the user provides to the spreadsheets save function
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                spreadsheet.save(file.FileName);
            }
        }

        //procs if the user clicks on load
        //opens an open file dialog
        //passes the filename of the file the user chose to the spreadsheet load function
        //also ends datagridview editing before entering the spreadsheet scope
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK)
            {
                dataGridView1.EndEdit();
                spreadsheet.load(file.FileName);
            }
        }
    }
}
