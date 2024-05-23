using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JFiK
{
    class LLVMGenerator
    {
        static string header = "";
        static string mainText = "";
        static int reg = 1;
        static int br = 0;
        static string buffer = "";

        static Stack<int> brstack = new Stack<int>();


        public static void Read(int id)
        {
            buffer += "%" + reg + " = call i32 (i8*, ...) @__isoc99_scanf(i8* getelementptr inbounds ([3 x i8], [3 x i8]* @strs, i32 0, i32 0), i32* %" + id + ")\n";
            reg++;
        }

        public static void Write(int id)
        {

        }

        public static void CloseMain()
        {
            mainText += buffer;
        }








        public static void generateLLVMFile()
        {
            string result = "";
            result += "declare i32 @printf(i8*, ...)\n";
            result += "declare i32 @__isoc99_scanf(i8*, ...)\n";
            result += "@strp = constant [4 x i8] c\"%d\\0A\\00\"\n";
            result += "@strs = constant [3 x i8] c\"%d\\00\"\n";
            result += "@strf = constant [4 x i8] c\"%f\\0A\\00\"\n";
            result += "@strd = constant [4 x i8] c\"%lf\\00\"\n";
            result += "@strl = constant [6 x i8] c\"%lld\\0A\\00\"\n";

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
