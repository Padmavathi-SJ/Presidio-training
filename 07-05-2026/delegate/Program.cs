using System;

namespace BankingFEApplication
{
    internal class Program
    {
        public delegate void MyDelegate(int n1, int n2);//Declare the type

        MyDelegate delegateRef;//refference for the type

        public void Add(int num1, int num2)//Method that could be delegated
        {
            var result = num1 + num2;
            Console.WriteLine($"The sum of {num1} and {num2} is {result}");
        }

        public void Product(int num1, int num2)//Method that could be delegated
        {
            var result = num1 * num2;
            Console.WriteLine($"The product of {num1} and {num2} is {result}");
        }

        public Program()//Constructore for instan
        {
            delegateRef = new MyDelegate(Product);
        }

        void Calculate(MyDelegate del) //takes functionality as parameter
        {
            del(100, 200);
        }
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Calculate(program.delegateRef);
        }
    }
}