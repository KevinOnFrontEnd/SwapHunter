namespace SwapHunter;

public static class ChiaHelper
{
    //there are one Trillion mojos per chia. 
    const double MOJOS_PER_CHIA = 1000000000000; 
    public static double ConvertToMojos(double amount)
    {
        return amount * MOJOS_PER_CHIA;
    }
}