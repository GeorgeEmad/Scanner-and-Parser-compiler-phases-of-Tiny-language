using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class scanner
    {
        static string scanerrors = "";
        static public string GetScanerrors() {return scanerrors;}
        static public bool hasAlphabet(string input)// this function checks if there is an alphabetical letter (for checking for identifiers)
        {
            string alpha = "abcdefghijkmnlopqrstuvwxyz";
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    if (input[i] == alpha[j]) { return true; }
                }

            }
            return false;
        }
        public static String[] toknize(String text, String delimeters)
        {
            String[] tokens = new String[text.Length];
            for (int i = 0; i < tokens.Length; i++)//initialize the tokens string
                tokens[i] = "";


            int index = 0;// the position in the string array
            for (int i = 0; i < text.Length; i++)//checking every character in the text string
            {
                Boolean del = false;
                for (int j = 0; j < delimeters.Length; j++)
                {
                    if (text[i] == delimeters[j])
                    {
                        if (!(String.IsNullOrEmpty(tokens[index])))
                            index++;

                        tokens[index] = "" + delimeters[j];
                        index++;
                        del = true;
                    }

                }
                if (!del)
                    tokens[index] += "" + text[i];



            }
            return tokens;
        }
        static public string[,] Lexer(string input)
        {

            String delimeters = " <>+-*/=:,;{}[]()\"" + "\r\n"; //delimiters 
            String[] splitted = toknize(input, delimeters); //calls the tokenize function which tokenises the text
            string[,] print = new string[splitted.Length, 2]; // the 2D array to be returned from this function

            bool comment = false;
            string commentt = "";
            bool str = false;
            string strr = "";
            int index = 0;
            for (int i = 0; i < splitted.Length; i++)
            {
                //--------------if there is a comment------------------------
                if (comment == true)
                {
                    if (splitted[i] == "*" && splitted[i + 1] == "/")
                    {
                        comment = false;
                        commentt += "*/";
                        print[index, 0] = commentt;
                        print[index, 1] = "comment";
                        commentt = "";
                        i++;
                        index++;
                        continue;
                    }
                    else
                    {
                        commentt += splitted[i];
                        continue;
                    }
                }
                //---------------------------------------------
                //------------------- if there is a string--------------
                if (str == true)
                {
                    if (splitted[i] == "\"")
                    {
                        str = false;
                        strr += "\"";
                        print[index, 0] = strr;
                        print[index, 1] = "string";
                        strr = "";
                        index++;
                        continue;
                    }
                    else
                    {
                        strr += splitted[i];
                        continue;
                    }
                }
                //---------------------------------------------------
                if (splitted[i] == "/") //checks for /* (start of a comment)
                {
                    if (splitted[i + 1] == "*")
                    {
                        commentt += splitted[i];
                        comment = true;
                        continue;
                    }
                }
                if (splitted[i] == "\"") //checks for a string ""
                {
                    strr += splitted[i]; str = true; continue;
                }
                if (splitted[i] == "int" || splitted[i] == "float" || splitted[i] == "string" || splitted[i] == "read" || splitted[i] == "write" ||
                     splitted[i] == "repeat" || splitted[i] == "until" || splitted[i] == "if" || splitted[i] == "endl" || splitted[i] == "return"
                     || splitted[i] == "then" || splitted[i] == "endl" || splitted[i] == "end" || splitted[i] == "elseif" || splitted[i] == "else"
                     || splitted[i] == "real" || splitted[i] == "begin") //check for reserved words
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "RESERVED_WORD_" + splitted[i];
                }
                else if (float.TryParse(splitted[i], out _)) //check for numbers
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "Number_" + splitted[i];

                }
                // check for operators----------------------
                else if (splitted[i] == ":")
                {
                    if (splitted[i + 1] == "=") {
                        print[index, 0] = splitted[i] + splitted[i + 1];
                        print[index, 1] = "Assign_Operator";
                        i++;
                        index++;
                        continue;
                          }
                    else { scanerrors += "assign operator's " + splitted[i] + " must be followed by an =" + "\n" + "\n"; }
                }
                else if (splitted[i] == "+" || splitted[i] == "*" || splitted[i] == "/" || splitted[i] == "-")
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "Arithmatic_Operator_" + splitted[i];
                }
                else if (splitted[i] == ">=" || splitted[i] == "<=" || splitted[i] == ">" || splitted[i] == "<" || splitted[i] == "==" ||
                  splitted[i] == "<>")
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "condition_operator" + splitted[i];
                }
                else if (splitted[i] == "&&" || splitted[i] == "||" || splitted[i] == "!")
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "Boolean_operator" + splitted[i];
                }
                else if (splitted[i] == "%" ||
                 splitted[i] == "=" || splitted[i] == "{" || splitted[i] == "}" ||
                  splitted[i] == ":=")
                {
                    print[index, 0] = splitted[i];
                    print[index, 1] = "Operator_" + splitted[i];
                }
                //----------------------------------------------------------------
                else if (splitted[i] == ";" || splitted[i] == "(" || splitted[i] == ")" || splitted[i] == ",")
                { //check for separators 
                    print[index, 0] = splitted[i];
                    print[index, 1] = "Separator_" + splitted[i];
                }

                else if (hasAlphabet(splitted[i].ToString())) // check for identifiers
                {
                    if (Char.IsDigit(splitted[i][0]))
                    {
                        // prints to the console than an identifier cant start with number
                        scanerrors += "identifier " + splitted[i] + " can't begin with number ,Identifier must begin with letter only." + "\n" +"\n";
                    }
                    else
                    {
                        print[index, 0] = splitted[i];
                        print[index, 1] = "Identifier_" + splitted[i];
                    }

                }
                else
                {
                    continue;
                }
                index++;
            }
            return print; //returns the print 2D array which contains the character on the first column and its lexical analysis on the right column
        }
    }
}
