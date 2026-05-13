using System;

namespace WordGuessingGame.Services{
    public class FeedbackGenerator{
        public string GenerateFeedback(string guess, string secret){
           guess = guess.ToUpper().Trim();
            secret = secret.ToUpper().Trim();
           
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
            
          return new string(temp);

        }
    }
}