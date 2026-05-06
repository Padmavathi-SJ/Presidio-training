using System;

namespace Test{
    class Program{
        public static void Main(string[] args){
            string secret = "MANGO";
            string guess = "";

            int attempts = 1;
            int max_attempts = 6;

            string feedback = "";

            

            while(attempts <= max_attempts){
                Console.WriteLine("enter your guess: ");
                guess = Console.ReadLine().Trim().ToUpper();
                char[] temp = new char[5];

            for(int i=0; i<5; i++){
                if(guess[i] == secret[i]){
                    temp[i] = 'G'; //correct position
                } else if (!secret.Contains(guess[i])){
                    temp[i] = 'X'; // not present
                } else{
                    temp[i] = 'Y'; // present but in wrong position
                }
            }
            
            if(guess == secret){
                    switch (attempts){
                    case 1:
                        feedback = "Genius!";

                        break;
                    case 2:
                        feedback = "Excellent!";
                        break;
                    case 3:
                        feedback = "Great job!";
                        break;
                    case 4:
                        feedback = "Good work!";
                        break;
                    case 5:
                        feedback = "Nice try!";
                        break;
                    case 6:
                        feedback = "That was close!";
                        break;
              }
              Console.WriteLine(feedback);
              return;
            }
            Console.WriteLine(temp);
            Console.WriteLine($"try again... you have still {attempts} attempts");
            attempts++;
            
            }

            Console.WriteLine($"game over,and the secret is {secret}");



        }
    }
}