using System;
using System.Collections.Generic;
using System.Text;

namespace Expression_ator
{
    class Program
    {
    
        public class result{
            public int data;
            public string error;
            public bool isErrored;

            public result(int data,string error){
                this.data = data;
                this.error = error;
                isErrored = false;
            }
            public void setError(string error){
                this.error = error;
                isErrored = true;
            }
            public string getError(){
                return this.error;
            }
            public void setData(int data){
                this.data= data;
            }
            public int getData(){
                return this.data;
            }
            public bool isError(){
                return isErrored;
            }

        }


        public class expression
        {
            private string  text;
            private int     position;
            private int     depth;
            private string  error;
            private bool    isErrored   =false;
            private bool    isEvaluated =false;
            private bool    debug       =false;

            public expression(){
                
            }
            public expression(string expression, int depth, int position,bool debug){
                setData(expression,depth,position);
                this.debug = debug;
            }
            public void setError(string error){
                this.error = error;
                isErrored = true;
            }
            public string getError(){
                return this.error;
            }
            public void setDebug(bool debug){
                this.debug = debug;
            }
            public bool getDebug(){
                return this.debug;
            }
            public void setData(string expression,int depth,int position){
                this.text = expression;
                this.depth = depth;
                this.position = position;
            }
            public void setData(string expression){
                this.text = expression;
            }   
            public string getData(){
                return this.text;
             
            }
            public int getPosition(){
                return this.position;
            }
            public int getDepth(){
                return this.depth;
            }
            public string getExpression(){
                return this.text;
            }
            public bool isError(){
                return isErrored;
            }
            static List<string> splitWithDelimiters(string data, string[] delimiters)
            {
                List<string> tokens = new List<string>();
                int len = delimiters.Length;
                int index, position = 0, delimiter = 0, maxDepth;

                while (delimiter > -1)
                {                                                           //loop while there is an occurance of the delimiter
                    maxDepth = data.Length;
                    delimiter = -1;
                    for (int a = 0; a < len; a++){
                        index = data.IndexOf(delimiters[a], position);
                        if (-1 != index && index < maxDepth){                                   //if an occurance is found is it closer to your position than the last?
                            maxDepth = index;                                                   //if so update delimiter
                            delimiter = a;
                        }
                    }
                    if (delimiter > -1){                                                           //if delimiter found add string
                        int tLen = maxDepth - position;
                        if (tLen > 0){
                            tokens.Add(data.Substring(position, tLen));
                        }
                        tokens.Add(data.Substring(maxDepth, delimiters[delimiter].Length));
                        position = maxDepth + delimiters[delimiter].Length;

                    }else{
                        if (position < data.Length){
                            tokens.Add(data.Substring(position));                               //catch last piece of string
                        }
                    }
                }

                return tokens;
            }

            public void evaluate(){
                string data= this.text;

                if (isEvaluated){
                    return;
                }
                if (data == null || data == ""){
                    setError("No string to evaluate");
                    return;
                }

                string[] operations = { "(", ")", "+", "-", "*", "/","%","^" };
                List<string> tokens = splitWithDelimiters(data, operations);
                List<string> tempList = new List<string>();

                for (int e = 0; e < tokens.Count; e++){                                     //After split recombine negative and positive numbers after Multiplication or division
                   // Console.WriteLine(tokens[e]);
                    if ((e < tokens.Count - 1) && (tokens[e] == "(" || tokens[e] == "-" || tokens[e] == "+" || tokens[e] == "*" || tokens[e] == "/" || tokens[e] == "%" || tokens[e] == "^"))
                    {        //if not the lastitem
                        if((e<tokens.Count-2) && (tokens[e + 1] == "-" || tokens[e + 1] == "+")){
                            tokens[e + 1] = tokens[e + 1] + tokens[e + 2];
                            tokens.RemoveAt(e + 2);
                        }
                    }
                }


                string nextOperation;
                result tRes,left,right;
                int a = 0;
                int tStart = 0, tEnd = 0;
                bool doOp = false;

                string spacer="";
                for (a = 0; a < this.position; a++) spacer += " ";

                for (int t = 0; t < 3; t++){
                    a = 0;
                    while (a < tokens.Count){
                        if (tokens[a] == ")" || tokens[a] == "("){
                            a++;
                            continue;
                        }

                        nextOperation = isOperation(tokens[a]);         //is this an operation?
                        if (null == nextOperation){
                            a++;
                            continue;                                   //no? then continue
                        }else{
                            if (a == 0 || (a == 1 && tokens[a - 1] == "(")){
                                if (nextOperation == "-" || nextOperation == "+"){
                                    left = getValue("0");
                                    tStart = a;
                                }else{
                                    setError("Invalid Operation. No left hand side of Operand.");
                                    return;
                                }
                            }else{
                                tStart = a - 1;
                                left = getValue(tokens[a - 1]);
                            }
                            if (a + 1 == tokens.Count){
                                setError("Hanging Operand at the end of evaluated string.");
                                return;
                            }else{
                                tEnd = a;
                                right = getValue(tokens[a + 1]);
                            }

                            doOp = false;
                            if (t == 0 && (nextOperation == "*" || nextOperation == "/" || nextOperation == "%")) doOp = true;
                            if (t == 1 && (nextOperation == "^"                         )) doOp = true;
                            if (t == 2 && (nextOperation == "-" || nextOperation == "+" )) doOp = true;
                            
                            a++;
                            if (doOp){
                                if (debug){
                                    StringBuilder newLine = new StringBuilder();
                                    for (int z = 0; z < tokens.Count; z++){
                                        newLine.Append(tokens[z]);
                                    }
                                    Console.ForegroundColor=ConsoleColor.Blue;
                                    Console.WriteLine(spacer + newLine.ToString());
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                }
                                tRes = doOperation(left, right, nextOperation);
                                if (tRes.isErrored){
                                    this.isErrored = true;
                                    setError("Loop: "+t+" :" + tRes.getError());
                                    return;
                                }
                                tokens[a] = tRes.data.ToString();
                                tokens.RemoveRange(tStart, tEnd - tStart+1);
                                a = 0;
                            }
                        
                        }
                    }//end loop
                }
               
                if (tokens.Count == 3 && tokens[0]=="(" && tokens[2]==")"){
                    this.text = tokens[1];
                    if (this.depth > 0) this.depth--;
                }
                
           
                isEvaluated = true;
            }

            static result doOperation(result left, result right, string operation){
                int masterValue = 0;
                
                
                if (left.isError()){
                    return new result(left.getData(),"left of operation is errored:" + left.getError());
                }
                if (right.isError()){
                    return new result(right.getData(), "right of operation is errored:" + right.getError());
                }
                
                switch (operation){
                    case "^": masterValue = (int)Math.Pow((double)left.data ,(double) right.data); break;
                    case "%": masterValue = left.data % right.data; break;
                    case "+": masterValue = left.data + right.data; break;
                    case "-": masterValue = left.data - right.data; break;
                    case "*": masterValue = left.data * right.data; break;
                    case "/": if (left.data == 0) return new result(0,"Divide By 0"); else masterValue = left.data / right.data; break;
                    default: return new result(0,"Undefined operation"); 
                }

                return new result(masterValue, "");
            }

            //checks a string to see if it is a valid operation
            // returns the operation if true, null if false
            static string isOperation(string operation){
                string[] operations = { "(", ")", "+", "-", "*", "/","%","^" };
                int len = operations.Length;
                for (int b = 0; b < len; b++){
                    if (operation == operations[b]) return operations[b];
                }
                return null;
            }

            //checks a string to see if it is parsable number
            // returns the number if true, null if false
            static result getValue(string variable){
                
                int value;
                if (!int.TryParse(variable, out value)){
                    return new result(0,"Failed to convert to int");
                }
                return new result(value, "");
            }
        }

        public class expresisonGroup
        {
            public List<expression> expressions = new List<expression>();
            private string error;
            private bool isErrored;
            private string text;
            private int maxDepth=0;
            public bool debug = false;
            public expresisonGroup(string data){
                preErrorCheck(data); 
                this.text = "(" + data + ")";
                this.splitExpression();
                
            }
            //checks a string to see if it is a valid operation
            // returns the operation if true, null if false
            static bool isOperation(char operation)
            {
                char[] operations = { '(', ')', '+', '-', '*', '/', '%', '^' };
                int len = operations.Length;
                for (int b = 0; b < len; b++)
                {
                    if (operation == operations[b]) return true;
                }
                return false;
            }


            public void preErrorCheck(string data){
                for (int a = 0; a < data.Length; a++){
                    if (a!=0 && data[a] == '('){
                        if (!isOperation(data[a - 1])){
                            setError("Invalid join on left Parenthetical at Position:" + a);
                            return;
                        }
                    }
                    if (a!=data.Length-1 && data[a] == ')'){
                        if (!isOperation(data[a + 1])){
                            setError("Invalid join on right Parenthetical at Position:" + a);
                            return;
                        }
                    }
                }
            }

            public void add(string expression, int depth, int position)
            {            //add expression reset maxdepth
                expressions.Add(new expression(expression,depth,position,debug));
               
            }
            public int  getMaxDepth(){
                return this.maxDepth;
            }
            
            public void adjustMaxDepth(){
                maxDepth = 0;
                for (int a = 0; a < expressions.Count; a++){
                    if (expressions[a].getDepth() > maxDepth){
                        maxDepth = expressions[a].getDepth();
                    }
                }
            }

            public void evaluate(){
                if (debug){
                    while (!isError() && getMaxDepth() > 0){
                        display();
                        compressLevel();
                    }
                    display();
                }else{
                    while (!isError() && getMaxDepth() > 0){
                        compressLevel();
                    }
                }
            }

            public void compressLevel()
            {
                for (int a = 0; a < expressions.Count; a++)
                {
                    if (expressions[a].getDepth() == maxDepth)
                    {
                        expressions[a].setDebug(debug);
                        expressions[a].evaluate();
                        if (expressions[a].isError()) setError(expressions[a].getError());
                        mergeExpressions();
                    }
                }
                adjustMaxDepth();
            }

            public void mergeExpressions()
            {
                if (expressions.Count < 2) return;
                int a = 0;
                while (a < expressions.Count - 1)
                {
                    if (expressions[a].getDepth() == expressions[a + 1].getDepth())
                    {
                        expressions[a].setData(expressions[a].getData() + expressions[a + 1].getData());
                        expressions.RemoveAt(a + 1);
                    }
                    else
                    {
                        a++;
                    }

                }
            }

            public void setError(string error)
            {
                this.error = error;
                isErrored = true;
            }

            public string getError()
            {
                return this.error;
            }

            public bool isError()
            {
                return isErrored;
            }

            public void display(){
                if (!debug) return;
                StringBuilder o = new StringBuilder();
                string spacer = "";
                Console.WriteLine("***********************");
                for (int b = 0; b <= maxDepth; b++){
                    for (int a = 0; a < expressions.Count; a++) {
                        if (expressions[a].getDepth() == b) {
                            if (expressions[a].isError()){
                                Console.ForegroundColor = ConsoleColor.Red;
                            } else {
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            Console.Write(expressions[a].getExpression());
                        } else {
                            spacer = "";
                            for (int c = 0; c < expressions[a].getExpression().Length; c++) spacer += " ";
                            Console.Write(spacer);
                        }
                    }
                    Console.Write(": Depth " + b + "\r\n");
                }
                for (int a = 0; a < expressions.Count; a++){
                    if (expressions[a].isError()){
                        Console.WriteLine(a+":"+expressions[a].getError() + "\r\n");
                    }
                }
                if (isError()) Console.WriteLine(getError());
                //Console.WriteLine(o.ToString());
            }
            private void splitExpression(){
                string data= text;
                maxDepth = 0;
                int position = 0, depth = 0, len = data.Length;

                for (int a = 0; a < len; a++){
                    if (data[a] == '('){
                        this.add(data.Substring(position, a - position),depth,position);
                        position = a;
                        depth++;
                        if (depth > maxDepth) maxDepth = depth;
                    }
                    if (data[a] == ')'){
                        this.add(data.Substring(position, a - position + 1), depth, position);
                        depth--;
                        if (depth < 0){
                            int ePos = a - 4, eLen = 0;
                            if (ePos < 0) ePos = 0;
                            if (len - ePos > 10) eLen = 10; else eLen = len - ePos;
                            this.setError("Error in Expression: Negative depth. Check Parenthetical near position:" + (a - ePos) + " ->" + data.Substring(ePos, eLen));
                        }
                        position = a + 1;
                    }//if start tag
                }//loop

                if (position != (len - 1)){
                    this.add(data.Substring(position), depth, position);
                }
            }

        }//end expression group class

        
        static void Main(string[] args) {

           string cmd = "(1+2+3+5+5+10/2*2+51+(((4+1)+3+(3*(1-3))+(((41-2+((1+(3+(3+(8+4)+2)))+1)))+1)*1*1))-(1)+(1+(2+(3+(+5+(5+(5)))))*3))";
           // string cmd = "(4^4)%100+2*10";
            int loops = 1;
            
			//long start2,end2;
			DateTime start2 = System.DateTime.Now;
            expresisonGroup g1;


            for (int l = 0; l < loops; l++){
                g1 = new expresisonGroup(cmd);
                g1.debug = true;
                g1.evaluate();
                
            }
            
            DateTime end2 = System.DateTime.Now;
			Console.WriteLine((end2.Ticks - start2.Ticks) / loops);
			Console.ReadKey();
		}//end main
        
	}//end class
}//end namespace
