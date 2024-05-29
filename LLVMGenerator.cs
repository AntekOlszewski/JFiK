using Antlr4.Runtime.Misc;
using LLVMSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JFiK
{
    class LLVMGenerator
    {
        static string header = "";
        static string mainText = "";
        public static int reg = 1;
        static int br = 0;
        static string buffer = "";

        static Stack<int> brstack = new Stack<int>();

        public static void Assign(string id, string value, string type, bool global)
        {
            buffer += "store " + type + " " + value + ", " + type + "* %" + id + "\n";
        }

        public static void Declare(string id, string type)
        {
        //if (global)
        //{
        //    header += "store " + type + " " + value + ", " + type + "* @" + id + "\n";
        //}
        //else
        //{
            buffer += "%" + id + " = alloca " + type + "\n";
        }

        public static void PrintInteger(string id)
        {
            buffer += "%" + reg + " = load i32, i32* %" + id + "\n";
            reg++;
            buffer += "%" + reg + " = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strp, i32 0, i32 0), i32 %" + (reg - 1) + ")\n";
            reg++;
        }

        public static void PrintDouble(string id)
        {
            buffer += "%" + reg + " = load double, double* %" + id + "\n";
            reg++;
            buffer += "%" + reg + " = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strpd, i32 0, i32 0), double %" + (reg - 1) + ")\n";
            reg++;
        }

        public static void PrintBoolean(string id)
        {
            buffer += "%" + reg + " = load i1, i1* %" + id + "\n";
            reg++;
            buffer += "%" + reg + " = getelementptr inbounds [5 x i8], [5 x i8]* @str_true, i32 0, i32 0\n";
            reg++;
            buffer += "%" + reg + " = getelementptr inbounds [6 x i8], [6 x i8]* @str_false, i32 0, i32 0\n";
            reg++;
            buffer += "%" + reg + " = select i1 %" + (reg - 3) + ", i8* %" + (reg - 2) + ", i8* %" + (reg - 1) + "\n";
            reg++;
            buffer += "%" + reg + " = getelementptr inbounds [3 x i8], [3 x i8]* @str_fmt, i32 0, i32 0\n";
            reg++;
            buffer += "call i32 (i8*, ...) @printf(i8* %" + (reg - 1) + ", i8* %" + (reg - 2) + ")\n";
            reg++;

        }

        public static void CloseMain()
        {
            mainText += buffer;
        }


        public static void ScanInteger(string id)
        {
            buffer += "%" + reg + " = call i32 (i8*, ...) @__isoc99_scanf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @strs, i32 0, i32 0), i32* %" + id + ")\n";
            reg++;
        }

        public static void ScanDouble(string id)
        {
            buffer += "%" + reg + " = call i32 (i8*, ...) @__isoc99_scanf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strsd, i32 0, i32 0), double* %" + id + ")\n";
            reg++;
        }

        public static void ScanBoolean(string id)
        {
            buffer += "%" + reg + " = alloca i32, align 4\n";
            reg++;
            buffer += "%" + reg + " = getelementptr inbounds[3 x i8], [3 x i8] * @str_bool, i32 0, i32 0\n";
            reg++; 
            buffer += "% call = call i32(i8 *, ...) @__isoc99_scanf(i8 * % format_str, i32 * % " + reg + ")\n";
            reg++;
            buffer += "%int_loaded = load i32, i32* %" + (reg - 1) + ", align 4\n";
            reg++;
            buffer += "%" + id + " = icmp ne i32 %" + (reg - 1) + ", 0\n";
            reg++;
        }

        public static int loadInt(string id)
        {
            buffer += "%" + reg + " = load i32, i32* %" + id + "\n";
            reg++;
            return reg - 1;
        }

        public static int loadDouble(string id)
        {
            buffer += "%" + reg + " = load double, double* %" + id + "\n";
            reg++;
            return reg - 1;
        }

        public static void AddInteger(string val1, string val2, string type)
        {
            buffer += "%" + reg + " = add " + type + " " + val1 + ", " + val2 + "\n";
            reg++;
        }

        public static void AddDouble(string val1, string val2, string type)
        {
            buffer += "%" + reg + " = fadd " + type + " " + val1 + ", " + val2 + "\n";
            reg++;
        }

        public static void SubInteger(string v1, string v2)
        {
            buffer += "%" + reg + " = sub i32 " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void SubDouble(string v1, string v2)
        {
            buffer += "%" + reg + " = fsub double " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void MultInteger(string v1, string v2)
        {
            buffer += "%" + reg + " = mul i32 " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void MultDouble(string v1, string v2)
        {
            buffer += "%" + reg + " = fmul double " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void DivideInteger(string v1, string v2)
        {
            buffer += "%" + reg + " = sdiv i32 " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void DivideDouble(string v1, string v2)
        {
            buffer += "%" + reg + " = fdiv double " + v1 + ", " + v2 + "\n";
            reg++;
        }

        public static void compareVariables(string var1, string var2, string cond, string type)
        {
            buffer += "%" + reg + " = load " + type + ", " + type + "* " + var1 + "\n";
            reg++;
            buffer += "%" + reg + " = load " + type + ", " + type + "* " + var2 + "\n";
            reg++;
            buffer += "%" + reg + " = icmp " + cond + " " + type + " %" + (reg - 2) + ", %" + (reg - 1) + "\n";
            reg++;
        }





        public static void generateLLVMFile()
        {
            string result = "";
            result += "declare i32 @printf(i8*, ...)\n";
            result += "declare i32 @__isoc99_scanf(i8*, ...)\n";
            result += "@strp = constant [4 x i8] c\"%d\\0A\\00\"\n";
            result += "@strs = constant [3 x i8] c\"%d\\00\"\n";
            result += "@strpd = constant [4 x i8] c\"%f\\0A\\00\"\n";
            result += "@strf = constant [4 x i8] c\"%f\\0A\\00\"\n";
            result += "@strd = constant [4 x i8] c\"%lf\\00\"\n";
            result += "@strl = constant [6 x i8] c\"%lld\\0A\\00\"\n";
            result += "@strsd = constant [4 x i8] c\"%lf\\00\"\n";
            result += "@str_bool = private unnamed_addr constant [3 x i8] c\"%d\\00\", align 1\n";
            result += "@str_true = private unnamed_addr constant [5 x i8] c\"true\\00\", align 1\r\n";
            result += "@str_false = private unnamed_addr constant [6 x i8] c\"false\\00\", align 1\n";
            result += "@str_fmt = private unnamed_addr constant [3 x i8] c\"%s\\00\", align 1\n";

            result += header;
            result += "define i32 @main() nounwind{\n";
            result += mainText;
            result += "ret i32 0 }\n";


            string filePath = "C:\\Users\\Tomasz\\llvm\\JFiK\\output.ll";

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to file: " + ex.Message);
            }
        }
    }
}
