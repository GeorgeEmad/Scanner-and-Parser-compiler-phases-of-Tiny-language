using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parser
{
    public partial class Form1 : Form
    {
        /************************************************ PARSER *****************************************************/
        string[,] token ;
        
        int current_Token = 0;
        bool match_output;

        List<string> error_list;  
        void token_getter()
        {
            token = scanner.Lexer(richTextBox1.Text);
            int m = token.GetUpperBound(0);
            int n = token.GetUpperBound(1) + 1;
            object[] temp = new object[token.GetUpperBound(0)];
            for (int x = 0; x < m; x++)
                temp[x] = token[x, 0];
            temp = temp.Where(s => !object.Equals(s, null)).ToArray();
            string[,] output = new string[temp.Length, n];
            Array.Copy(token, output, temp.Length * n);
            token = output;
        }
        void error_list_getter()
        {
            error_list = new List<string>();
            error_list.Add("");
        }
        public void syntax_analyser(string[,] token)
        {
            this.token = token;
        }

        bool match(int current_tok, string inpt)
        {
            if (current_tok < token.GetLength(0) && token[current_tok,1] != null && token[current_tok, 1].Contains(inpt))
            {
                current_Token++;
                return true;
            }
            current_Token++;
            return false;
            
        }

        //statement -> if-stmt | repeat-stmt | assign-stmt | read-stmt | write-stmt
        void statement()
        {
            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "RESERVED_WORD_if")
            {
                TreeNode IF = treeView1.SelectedNode.Nodes.Add("IF");
                treeView1.SelectedNode = IF;
                if_stmt();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "RESERVED_WORD_repeat")
            {
                TreeNode repeat = treeView1.SelectedNode.Nodes.Add("REPEAT");
                treeView1.SelectedNode = repeat;
                repeat_stmt();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;

            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "RESERVED_WORD_read")
            {
                TreeNode Read = treeView1.SelectedNode.Nodes.Add("READ " + token[current_Token + 1, 0]);
                treeView1.SelectedNode = Read;

                read_stmt();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;

            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "RESERVED_WORD_write")
            {
                TreeNode Write = treeView1.SelectedNode.Nodes.Add("Write");
                treeView1.SelectedNode = Write;
                TreeNode Write2 = treeView1.SelectedNode.Nodes.Add(token[current_Token + 1, 0]);

                write_stmt();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] != null && token[current_Token, 1].Contains("Identifier"))
            {
                TreeNode ID = treeView1.SelectedNode.Nodes.Add("Assign " + token[current_Token, 0]);
                treeView1.SelectedNode = ID;
                if (current_Token +3 < token.GetLength(0)) { 
                    if (token[current_Token + 1, 0] == ":=" && (float.TryParse(token[current_Token + 2, 0], out _) || token[current_Token + 2, 1].Contains("Identifier")) && token[current_Token + 3, 0] == ";")
                    {
                        treeView1.SelectedNode.Nodes.Add(token[current_Token + 2, 0]);
                    }
                   }
                assign_stmt();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
            }
            else if( current_Token < token.GetLength(0)) 
                 { 

                 } 
        }

        //stmt-seq -> statement {; statement}
        void stmt_seq()
        {
            statement();
            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Separator_;")
            {
                while (current_Token < token.GetLength(0) && token[current_Token, 1] == "Separator_;")
                {
                    match_output = match(current_Token, "Separator_;");
                    if (match_output == false) { error_list.Add(" Semicolon missing"); }
                    statement();
                }
            }
            else if (current_Token < token.GetLength(0) - 1) { 
                error_list.Add(" Semicolon missing");////////////////////////////////////////////////////////
                statement();
            }
        }

        //if-stmt -> if ( exp ) then stmt-seq [else stmt-seq] end
        void if_stmt()
        {
            match_output = match(current_Token, "RESERVED_WORD_if");
            if (match_output == false) { error_list.Add(" IF missing in if statement"); }
            exp();
            match_output = match(current_Token, "RESERVED_WORD_then");
            if (match_output == false) { error_list.Add(" Then missing in if statement"); }
            stmt_seq();
            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "RESERVED_WORD_else")
            {
                match_output = match(current_Token, "RESERVED_WORD_else");
                if (match_output == false) { error_list.Add("Else missing in if statement"); }
                stmt_seq();
            }
            match_output = match(current_Token, "RESERVED_WORD_end");
            if (match_output == false) { error_list.Add(" End missing in if statement "); }
        }

        //repeat-stmt -> repeat stmt-seq until exp
        void repeat_stmt()
        {
            match_output = match(current_Token, "RESERVED_WORD_repeat");
            if (match_output == false) { error_list.Add("repeat missing in repeat statement"); }
            stmt_seq();
            match_output = match(current_Token, "RESERVED_WORD_until");
            if (match_output == false) { error_list.Add("until missing in repeat statement"); }
            exp();
        }

        //read-stmt -> read identifier
        void read_stmt()
        {
            match_output = match(current_Token, "RESERVED_WORD_read");
            if (match_output == false) { error_list.Add("read missing in read statement"); }
            match_output = match(current_Token, "Identifier");
            if (match_output == false) { error_list.Add("Identifier missing in read statement"); }
        }

        //write-stmt -> write exp
        void write_stmt()
        {
            match_output = match(current_Token, "RESERVED_WORD_write");
            if (match_output == false) { error_list.Add("write missing in write statement"); }
            exp();
          
        }

        //assign-stmt -> identifier := exp
        void assign_stmt()
        {
            match_output = match(current_Token, "Identifier");
            if (match_output == false) { error_list.Add("Identifier missing in assignment statement"); }
            match_output = match(current_Token, "Assign_Operator");
            if (match_output == false) { error_list.Add("Assign operator missing in assignment statement"); }
            exp();
        }

        //exp -> simple-exp [comparison-op simple exp]
        void exp()
        {
            
            simple_exp();
            if (current_Token < token.GetLength(0) && (token[current_Token, 1] == "condition_operator<" || token[current_Token, 1] == "Operator_="))
            {
                comparison_op();
                simple_exp();
            }
        }

        //simple-exp -> term {addop term}
        void simple_exp()
        {
            term();
            while (current_Token < token.GetLength(0) && (token[current_Token, 1] == "Arithmatic_Operator_+" || token[current_Token, 1] == "Arithmatic_Operator_-"))
            {
                addop();
                term();
            }
        }

        //factor -> number | identifier | ( exp )
        void factor()
        {
            

            if (current_Token < token.GetLength(0) && (token[current_Token, 1] != null && token[current_Token, 1].Contains("Number")))
            {
                
                current_Token++; //////////////// tree ??????????????????????????? 

            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Separator_(")
            {
                current_Token++;
                exp();
                if (current_Token < token.GetLength(0) && (!match(current_Token, "Separator_)")))
                {
                   error_list.Add("righ bracket not found "); 
                }                
            }
            else if (current_Token < token.GetLength(0) && (token[current_Token, 1] != null && token[current_Token, 1].Contains("Identifier")))
            {
                match(current_Token, "Identifier");
                
            }
            else if(current_Token < token.GetLength(0))
            {
               error_list.Add("missing number or identifier or Left bracket");
               stmt_seq();
            }
        }

        //term -> factor {mulop factor}
        void term()
        {
            factor();
            while (current_Token < token.GetLength(0) && (token[current_Token, 1] == "Arithmatic_Operator_*" || token[current_Token, 1] == "Arithmatic_Operator_/"))
            {
                TreeNode expresimp = treeView1.SelectedNode.Nodes.Add(token[current_Token, 0]);
                treeView1.SelectedNode = expresimp;
                TreeNode expresimp3 = treeView1.SelectedNode.Nodes.Add(token[current_Token - 1, 0]);
                TreeNode expresimp2 = treeView1.SelectedNode.Nodes.Add(token[current_Token + 1, 0]);
                
                current_Token++;
                factor();
                treeView1.SelectedNode = treeView1.SelectedNode.Parent; //////////////////////////////////////// FY FACTOR IN ERROR
            }
        }

        //mulop -> * | /
        void mulop()
        {

            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Arithmatic_Operator_*")
            {
                current_Token++;
            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Arithmatic_Operator_/")
            {
                current_Token++;
            }
            else if (current_Token < token.GetLength(0))
            {
                error_list.Add("missing * or / in multiplication or divide operation ");
                stmt_seq();
            }
        }

        //addop -> + | -
        void addop()
        {
            TreeNode expresimp = treeView1.SelectedNode.Nodes.Add(token[current_Token, 0]);
            treeView1.SelectedNode = expresimp;
            TreeNode expresimp3 = treeView1.SelectedNode.Nodes.Add(token[current_Token - 1, 0]);
            TreeNode expresimp2 = treeView1.SelectedNode.Nodes.Add(token[current_Token+1, 0]);
            
            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Arithmatic_Operator_+")
            {
                
                current_Token++;
            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Arithmatic_Operator_-")
            {
                current_Token++;
            }
            else if (current_Token < token.GetLength(0))
            {
                error_list.Add("missing + or - in addition or subtraction operation");
                stmt_seq();
            }
            treeView1.SelectedNode = treeView1.SelectedNode.Parent; 

        }

        //comparison-op -> < | =
        void comparison_op()
        {
            TreeNode expre = treeView1.SelectedNode.Nodes.Add(token[current_Token, 0]);
            treeView1.SelectedNode = expre;
            TreeNode expre1 = treeView1.SelectedNode.Nodes.Add(token[current_Token-1, 0]);
            TreeNode expre2 = treeView1.SelectedNode.Nodes.Add(token[current_Token +1, 0]);

            if (current_Token < token.GetLength(0) && token[current_Token, 1] == "Operator_=")
            {
                
                current_Token++;
            }
            else if (current_Token < token.GetLength(0) && token[current_Token, 1] == "condition_operator<")
            {
               
                current_Token++;
            }
            else if (current_Token < token.GetLength(0))
            {
                error_list.Add("missing < or = in comparison operation"); 
            }
            treeView1.SelectedNode = treeView1.SelectedNode.Parent;

        }

        void parsing()
        {
            stmt_seq();
            
        }
        
        /**************************************************** GUI **********************************************************/

        public Form1()
        {

            InitializeComponent();
            
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            current_Token = 0;
            treeView1.Nodes.Clear();
            TreeNode Program = treeView1.Nodes.Add("Program");
            treeView1.SelectedNode = Program;
            token_getter();
            error_list_getter();
            parsing();
            string error_output = "";
            foreach (string error_type in error_list) // get all errors from error List 
            {
                error_output = error_output + error_type + "\n";
            }

            richTextBox3.Text = error_output;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void SCAN_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            string output = "";
            string[,] print = scanner.Lexer(richTextBox1.Text);
            for (int i = 0; i < print.GetLength(0); i++)
            {
                if (print[i, 0] == null) { continue; }
                output += print[i, 0] + ' ' + print[i, 1] + "\n";

            }

            richTextBox2.Text = output;
            richTextBox4.Text = scanner.GetScanerrors();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {

        }
        private void richTextBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
